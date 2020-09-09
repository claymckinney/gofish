using GoFishCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolsCore;

namespace GoFishActors
{
    public class Player : IPlayer, IActor
    {
        private readonly IDealer _dealer;
        private readonly IAnnouncements _announcements;
        private bool drawPileIsEmpty;

        private readonly List<(ICard, ICard)> _pairsOnTable;
        public ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; private set; }

        private List<ICard> cardsInHand; // I'd like to take the actual card, add it to this
        // list, then take it out of the list to make pairs on the table or give it to another player. I don't
        // want it to be a copy of the card, and quietly destroy the old one. But, that isn't possible if the
        // card is serialized, passed around via http, and is deserialized. Nope, being silly. The client browser
        // will only have a static view of the thing, but the actual card could still exist in the Player's
        // memory. In the Player instance's cardsInHand field...
        // Still, it's shared state. If I don't carefully remove my own reference to the card object from my hand
        // that I'm sending to the other player, we could both have a reference to the same card in our hand.
        // For a future improvement, I need a sentient deck to be the source of truth for who has which card.

        public int NumberCardsInHand => cardsInHand.Count;

        public string Name { get; set; }

        public Player(IDealer dealer, IAnnouncements announcements)
        {
            _dealer = dealer;
            _announcements = announcements;
            _pairsOnTable = new List<(ICard, ICard)>();
            PairsOnTable = _pairsOnTable.AsReadOnly();
            cardsInHand = new List<ICard>();
        }

        public void Handle(IMessage message) // Messages I'm recieving. I'm the "ToPlayer" in all of these.
        {
            switch (message)
            {
                case DealerAskPlayerToResetHand resetHand:
                    ResetHand();
                    break;
                case DealerToPlayerDealCards dealtCards:
                    cardsInHand.AddRange(dealtCards.Cards);
                    _announcements.Add($"Dealer gave {Name} {dealtCards.Cards.Count()}. {Name} now has {cardsInHand.Count()} cards.");
                    break;
                case PlayerToPlayerGimmeFish fishRequest when cardsInHand.Where(x => x.Fish == fishRequest.Fish).Any():
                    FishRequestAffirmative(fishRequest);
                    break;
                case PlayerToPlayerGimmeFish fishRequest:
                    _announcements.Add($"{Name} is telling {fishRequest.Sender.Name} to \"Go Fish\".");
                    fishRequest.Sender.Handle(new PlayerToPlayerGoFish(sender: this));
                    break;
                case DealerToPlayerItsYourTurn myTurn:
                    LayDownMatches();
                    if (cardsInHand.Any())
                    {
                        AskForFish();
                    }
                    else
                    {
                        DrawUpToFive();
                    }
                    break;
                case PlayerToPlayerGiveCard gotCard:
                    HandleCard(gotCard.Card);
                    break;
                case PlayerToPlayerGoFish goFish when drawPileIsEmpty:
                    TurnIsOver();
                    break;
                case PlayerToPlayerGoFish goFish:
                    _announcements.Add($"{Name} is asking the dealer for a card.");
                    _dealer.Handle(new PlayerToDealerDrawCard(sender:this));
                    break;
                case DealerToPlayerGiveCard gotCard:
                    HandleCard(gotCard.Card);
                    break;
                case DealerToPlayerNoCardsLeft noCard: // Response to either fishing in the draw pile or drawing up to 5
                    _announcements.Add($"Dealer gave {Name} no cards because the draw pile is empty.");
                    drawPileIsEmpty = true;
                    TurnIsOver();
                    break;
                case DealerToPlayerGiveCards gotCards: // Response to drawing up to 5
                    cardsInHand.AddRange(gotCards.Cards);
                    _announcements.Add($"Dealer gave {Name} {gotCards.Cards.Count()} cards.");
                    TurnIsOver();
                    break;
                default:
                    _announcements.Add($"{Name} recieved a message that is unhandled. {message}");
                    _announcements.ReadAll();
                    break;
            }
        }

        private void ResetHand()
        {
            cardsInHand.Clear();
            _pairsOnTable.Clear();
            _announcements.Add($"{Name} has {cardsInHand.Count()} cards and {PairsOnTable.Count} pairs laid down on the table.");
        }

        private void FishRequestAffirmative(PlayerToPlayerGimmeFish fishRequest)
        {
            var cardToGive = cardsInHand.Where(x => x.Fish == fishRequest.Fish).FirstOrDefault();
            cardsInHand.Remove(cardToGive);
            _announcements.Add($"{Name} is giving {cardToGive.Fish} to {fishRequest.Sender.Name} and now has {cardsInHand.Count()} cards.");
            fishRequest.Sender.Handle(
                    new PlayerToPlayerGiveCard(
                        sender: this,
                        card: cardToGive)
                );
        }

        private void AskForFish()
        {
            // Check if any known Fish (from the Table conversation) are in my hand.
            //      ex: Peter asked for a crab, go fish, drew, didn't lay down. So Peter has a crab. Table knows this.
            // Choose a Fish to request

            cardsInHand.Shuffle();
            var pickACardAnyCard = cardsInHand.First();
            var fish = pickACardAnyCard.Fish;
            List<IPlayer> playersThatAreNotMe = new List<IPlayer>();
            playersThatAreNotMe.AddRange(_dealer.Players);
            playersThatAreNotMe.Remove(this);
            List<IPlayer> playersWithNoCards = new List<IPlayer>();
            foreach(var player in playersThatAreNotMe)
            {
                if (player.NumberCardsInHand == 0) playersWithNoCards.Add(player);
            }
            IList<IPlayer> result = playersThatAreNotMe.Except(playersWithNoCards).ToList();
            if (result.Count > 0)
            {
                result.Shuffle();
                var victim = result.First();
                _announcements.Add($"{Name} is asking {victim.Name} for a {fish}.");
                victim.Handle(new PlayerToPlayerGimmeFish(sender: this, fish));
            }
            else
            {
                _announcements.Add($"{Name} wanted to ask for a fish, but none of the other players have any cards.");
                TurnIsOver();
            }
        }

        private void LayDownMatches()
        {
            foreach (Fish fish in (Fish[])Enum.GetValues(typeof(Fish)))
            {
                var matches = cardsInHand.Where(x => x.Fish == fish).ToArray<ICard>();
                if(matches.Count() > 1)
                {
                    _announcements.Add($"{Name} is laying down a pair of {fish}.");
                    _pairsOnTable.Add((matches[0], matches[1]));
                    cardsInHand.Remove(matches[0]);
                    cardsInHand.Remove(matches[1]);
                }
            }
        }

        private void DrawUpToFive()
        {
            int numberNeeded = 5 - cardsInHand.Count;
            if(numberNeeded > 0 && !drawPileIsEmpty)
            {
                _announcements.Add($"{Name} is asking the Dealer for {numberNeeded} cards to draw up to 5.");
                _dealer.Handle(new PlayerToDealerAskForCards(sender: this, numberNeeded));
            }
            else
            {
                TurnIsOver();
            }
        }

        private void HandleCard(ICard card)
        {
            cardsInHand.Add(card);
            LayDownMatches();
            DrawUpToFive();
        }

        private void TurnIsOver()
        {
            _announcements.Add($"{Name}'s turn is over. {Name} has {cardsInHand.Count()} cards.");
            _dealer.Handle(new PlayerToDealerTurnOver(sender: this));
        }
    }
}
