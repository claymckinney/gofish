using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolsCore;

namespace GoFishCore
{
    public abstract class PlayerBase : IPlayer
    {
        protected readonly IDealer _dealer;
        protected readonly ILogger _logger;
        protected bool drawPileIsEmpty;

        private readonly List<(ICard, ICard)> _pairsOnTable;
        public ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; private set; }

        protected List<ICard> cardsInHand;

        public int NumberCardsInHand => cardsInHand.Count;

        public string Name { get; set; }

        public PlayerBase(IDealer dealer, ILogger<IPlayer> logger)
        {
            _dealer = dealer;
            _logger = logger;
            _pairsOnTable = new List<(ICard, ICard)>();
            PairsOnTable = _pairsOnTable.AsReadOnly();
            cardsInHand = new List<ICard>();
        }

        protected virtual void AskForFish()
        {
            // TODO: Check if any known Fish (from the conversation) are in my hand.
            //      ex: Peter asked for a crab, go fish, drew, didn't lay down. So Peter has a crab.
            //      For the players or the dealer to keep up with the context clues like this, all
            //      messages would need to be "read" by everyone, without giving away game secrets.
            //      This would be a major refactor.

            // For now, pick a random card to request and pick a random player to request it from.
            cardsInHand.Shuffle();
            var pickACardAnyCard = cardsInHand.First();
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
                result.Shuffle();
                var victim = result.First();
                _logger.LogInformation($"{Name} is asking {victim.Name} for a {fish}.");
                victim.Handle(new PlayerToPlayerGimmeFish(sender: this, fish));
            }
            else
            {
                _logger.LogInformation($"{Name} wanted to ask for a fish, but none of the other players have any cards.");
                TurnIsOver();
            }
        }

        public void Handle(DealerToPlayerDealCards dealtCards)
        {
            cardsInHand.AddRange(dealtCards.Cards);
            _logger.LogInformation($"Dealer gave {Name} {dealtCards.Cards.Count()}. {Name} now has {cardsInHand.Count()} cards.");
        }

        public void Handle(DealerToPlayerGiveCard gotCard)
        {
            HandleCard(gotCard.Card);
        }

        public void Handle(DealerToPlayerGiveCards gotCards)
        {
            // Response to drawing up to 5
            cardsInHand.AddRange(gotCards.Cards);
            _logger.LogInformation($"Dealer gave {Name} {gotCards.Cards.Count()} cards.");
            TurnIsOver();
        }

        public void Handle(DealerToPlayerItsYourTurn message)
        {
            LayDownMatches();
            if (cardsInHand.Any())
            {
                AskForFish();
            }
            else
            {
                DrawUpToFive();
            }
        }

        public void Handle(DealerToPlayerNoCardsLeft message)
        {
            _logger.LogInformation($"Dealer gave {Name} no cards because the draw pile is empty.");
            drawPileIsEmpty = true;
            TurnIsOver();
        }

        public void Handle(DealerToPlayerResetHand message)
        {
            cardsInHand.Clear();
            _pairsOnTable.Clear();
            _logger.LogInformation($"{Name} has {cardsInHand.Count()} cards and {PairsOnTable.Count} pairs laid down on the table.");
        }

        public void Handle(PlayerToPlayerGimmeFish message)
        {
            switch (message)
            {
                case PlayerToPlayerGimmeFish fishRequest when cardsInHand.Where(x => x.Fish == fishRequest.Fish).Any():
                    FishRequestAffirmative(fishRequest);
                    break;
                case PlayerToPlayerGimmeFish fishRequest:
                    _logger.LogInformation($"{Name} is telling {fishRequest.Sender.Name} to \"Go Fish\".");
                    fishRequest.Sender.Handle(new PlayerToPlayerGoFish(sender: this));
                    break;
                default:
                    _logger.LogInformation($"{Name} recieved a PlayerToPlayerGimmeFish message that is unhandled. {message}");
                    break;
            }
        }

        public void Handle(PlayerToPlayerGiveCard gotCard)
        {
            HandleCard(gotCard.Card);
        }

        public void Handle(PlayerToPlayerGoFish message)
        {
            switch (message)
            {
                case PlayerToPlayerGoFish goFish when drawPileIsEmpty:
                    TurnIsOver();
                    break;
                case PlayerToPlayerGoFish goFish:
                    _logger.LogInformation($"{Name} is asking the dealer for a card.");
                    _dealer.Handle(new PlayerToDealerDrawCard(sender: this));
                    break;
                default:
                    _logger.LogInformation($"{Name} recieved a PlayerToPlayerGoFish message that is unhandled. {message}");
                    break;
            }
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

        protected void TurnIsOver()
        {
            _logger.LogInformation($"{Name}'s turn is over. {Name} has {cardsInHand.Count()} cards.");
            _dealer.Handle(new PlayerToDealerTurnOver(sender: this));
        }
    }
}
