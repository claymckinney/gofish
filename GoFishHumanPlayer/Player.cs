using GoFishCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GoFishHumanPlayer
{
    public class Player : IPlayer, IActor
    {
        private readonly IDealer _dealer;
        private readonly ILogger _logger;
        private bool drawPileIsEmpty;

        private readonly List<(ICard, ICard)> _pairsOnTable;
        public ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; private set; }

        private List<ICard> cardsInHand;

        public int NumberCardsInHand => cardsInHand.Count;

        public string Name { get; set; }

        public Player(IDealer dealer, ILogger<Player> logger)
        {
            _dealer = dealer;
            _logger = logger;
            _pairsOnTable = new List<(ICard, ICard)>();
            PairsOnTable = _pairsOnTable.AsReadOnly();
            cardsInHand = new List<ICard>();
        }

        public void Handle(IMessage message) // Must handle all the "...ToPlayer..." messages, from other players and from the dealer
        {
            switch (message)
            {
                case DealerAskPlayerToResetHand resetHand:
                    ResetHand();
                    break;
                case DealerToPlayerDealCards dealtCards:
                    cardsInHand.AddRange(dealtCards.Cards);
                    _logger.LogInformation($"Dealer gave {Name} {dealtCards.Cards.Count()}. {Name} now has {cardsInHand.Count()} cards.");
                    break;
                case PlayerToPlayerGimmeFish fishRequest when cardsInHand.Where(x => x.Fish == fishRequest.Fish).Any():
                    FishRequestAffirmative(fishRequest);
                    break;
                case PlayerToPlayerGimmeFish fishRequest:
                    _logger.LogInformation($"{Name} is telling {fishRequest.Sender.Name} to \"Go Fish\".");
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
                    _logger.LogInformation($"{Name} is asking the dealer for a card.");
                    _dealer.Handle(new PlayerToDealerDrawCard(sender: this));
                    break;
                case DealerToPlayerGiveCard gotCard:
                    HandleCard(gotCard.Card);
                    break;
                case DealerToPlayerNoCardsLeft noCard: // Response to either fishing in the draw pile or drawing up to 5
                    _logger.LogInformation($"Dealer gave {Name} no cards because the draw pile is empty.");
                    drawPileIsEmpty = true;
                    TurnIsOver();
                    break;
                case DealerToPlayerGiveCards gotCards: // Response to drawing up to 5
                    cardsInHand.AddRange(gotCards.Cards);
                    _logger.LogInformation($"Dealer gave {Name} {gotCards.Cards.Count()} cards.");
                    TurnIsOver();
                    break;
                default:
                    _logger.LogInformation($"{Name} recieved a message that is unhandled. {message}");
                    break;
            }
        }

        private void ResetHand()
        {
            cardsInHand.Clear();
            _pairsOnTable.Clear();
            _logger.LogInformation($"{Name} has {cardsInHand.Count()} cards and {PairsOnTable.Count} pairs laid down on the table.");
        }

        private void FishRequestAffirmative(PlayerToPlayerGimmeFish fishRequest)
        {
            var cardToGive = cardsInHand.Where(x => x.Fish == fishRequest.Fish).FirstOrDefault();
            cardsInHand.Remove(cardToGive);
            _logger.LogInformation($"{Name} is giving {cardToGive.Fish} to {fishRequest.Sender.Name} and now has {cardsInHand.Count()} cards.");
            fishRequest.Sender.Handle(
                    new PlayerToPlayerGiveCard(
                        sender: this,
                        card: cardToGive)
                );
        }

        private void AskForFish()
        {
            _logger.LogInformation("Choose a card:");
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                _logger.LogInformation($"Press {i} for {cardsInHand[i].DisplayName}");
            }
            var cardNumberString = Console.ReadLine();
            if (!int.TryParse(cardNumberString, out int cardNumber))
            {
                _logger.LogInformation("You didn't pick a number correctly. Sorry.");
                AskForFish();
            }
            if (cardNumber > cardsInHand.Count - 1)
            {
                _logger.LogInformation("The number you picked isn't available.");
                AskForFish();
            }
            var pickACardAnyCard = cardsInHand[cardNumber];

            _logger.LogInformation("Choose a victim:");
            var fish = pickACardAnyCard.Fish;
            List<IPlayer> playersThatAreNotMe = new List<IPlayer>();
            playersThatAreNotMe.AddRange(_dealer.Players);
            playersThatAreNotMe.Remove(this);
            List<IPlayer> playersWithNoCards = new List<IPlayer>();
            foreach (var player in playersThatAreNotMe)
            {
                if (player.NumberCardsInHand == 0) playersWithNoCards.Add(player);
            }
            IList<IPlayer> result = playersThatAreNotMe.Except(playersWithNoCards).ToList();
            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    _logger.LogInformation($"Press {i} for {result[i].Name}, who has {result[i].NumberCardsInHand} cards.");
                }
                var playerNumberString = Console.ReadLine();
                if (!int.TryParse(playerNumberString, out int playerNumber))
                {
                    _logger.LogInformation("You didn't pick a number correctly. Sorry.");
                    _logger.LogInformation("Bailing out of game.");
                    return;
                }
                if (playerNumber > result.Count - 1)
                {
                    _logger.LogInformation("The number you picked isn't available.");
                    _logger.LogInformation("Bailing out of game.");
                    return;
                }
                var victim = result[playerNumber];
                _logger.LogInformation($"{Name} is asking {victim.Name} for a {fish}.");
                victim.Handle(new PlayerToPlayerGimmeFish(sender: this, fish));
            }
            else
            {
                _logger.LogInformation($"{Name} wanted to ask for a fish, but none of the other players have any cards.");
                TurnIsOver();
            }
        }

        private void LayDownMatches()
        {
            foreach (Fish fish in (Fish[])Enum.GetValues(typeof(Fish)))
            {
                var matches = cardsInHand.Where(x => x.Fish == fish).ToArray<ICard>();
                if (matches.Count() > 1)
                {
                    _logger.LogInformation($"{Name} is laying down a pair of {fish}.");
                    _pairsOnTable.Add((matches[0], matches[1]));
                    cardsInHand.Remove(matches[0]);
                    cardsInHand.Remove(matches[1]);
                }
            }
        }

        private void DrawUpToFive()
        {
            int numberNeeded = 5 - cardsInHand.Count;
            if (numberNeeded > 0 && !drawPileIsEmpty)
            {
                _logger.LogInformation($"{Name} is asking the Dealer for {numberNeeded} cards to draw up to 5.");
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
            _logger.LogInformation($"{Name}'s turn is over. {Name} has {cardsInHand.Count()} cards.");
            _dealer.Handle(new PlayerToDealerTurnOver(sender: this));
        }
    }
}
