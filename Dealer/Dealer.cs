using GoFishCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolsCore;

namespace GoFishActors
{
    public class Dealer : IDealer, IActor
    {
        // The dealer has all the cards. Shuffles. Deals. Gives the top card when they "Go Fish". Gives cards for them to "Draw up to 5"
        // The dealer knows how many cards are left. Notifies the table when there are no more.
        // The dealer is not a player.
        // The dealer knows whose turn it is. The dealer knows the rules. The dealer prompts the players.

        private List<ICard> cardsInDeck = new List<ICard>(); // It's the "deck" during the deal, but then it becomes the "draw pile" same thing.
        private ITable _table;
        private IAnnouncements _announcements;

        public Dealer(ITable table, IAnnouncements announcements)
        {
            _table = table;
            _announcements = announcements;
        }

        public void Handle(IMessage message)
        {
            switch (message)
            {
                case PlayerToDealerDrawCard cardRequest when cardsInDeck.Any():
                    var card = cardsInDeck.FirstOrDefault();
                    cardsInDeck.Remove(card);
                    _announcements.Add($"Dealer gives a card to {cardRequest.Sender.Name}. There are now {cardsInDeck.Count()} cards left in the draw pile.");
                    cardRequest.Sender.Handle(new DealerToPlayerGiveCard(card));
                    break;
                case PlayerToDealerDrawCard cardRequest:
                    _announcements.Add($"There are no cards left in the draw pile.");
                    _announcements.ReadAll();
                    cardRequest.Sender.Handle(new DealerToPlayerNoCardsLeft());
                    break;
                case PlayerToDealerAskForCards cardsRequest when cardsInDeck.Any():
                    GiveCardsToPlayer(cardsRequest.Number, cardsRequest.Sender);
                    break;
                case PlayerToDealerAskForCards cardsRequest:
                    _announcements.Add($"There are no cards left in the draw pile.");
                    cardsRequest.Sender.Handle(new DealerToPlayerNoCardsLeft());
                    break;
                case PlayerToDealerTurnOver turnOver:
                    if (!IsGameOver())
                    {
                        TellNextPlayerToTakeTurn();
                    }
                    else
                    {
                        WhoWon();
                    }
                    break;
                default:
                    _announcements.Add($"Dealer recieved an unhandled message. {message}");
                    _announcements.ReadAll();
                    break;
            }
        }

        private void BuildNewDeck()
        {
            int counter = 0;
            for (int i = 0; i < 4; i++){
                foreach (Fish fish in (Fish[])Enum.GetValues(typeof(Fish)))
                {
                    var card = new Card(++counter, fish.ToString(), fish);
                    cardsInDeck.Add(card);
                }
            }
        }

        public void StartGame()
        {
            BuildNewDeck();
            cardsInDeck.Shuffle();
            // Tell each player to Reset their Hand
            foreach(var player in _table.Players)
            {
                player.Handle(new DealerAskPlayerToResetHand());
            }
            foreach(var player in _table.Players)
            {
                DealCardsToPlayer(5, player);
            }
            TellNextPlayerToTakeTurn();
        }

        private void DealCardsToPlayer(int numberOfCards, IPlayer player)
        {
            var cards = cardsInDeck.GetRange(0, numberOfCards);
            cardsInDeck.RemoveRange(0, numberOfCards);
            _announcements.Add($"Dealer has removed 5 cards from the deck and is dealing them to {player.Name}");
            player.Handle(new DealerToPlayerDealCards(cards));
        }

        private void GiveCardsToPlayer(int numberOfCards, IPlayer player)
        {
            if (numberOfCards > cardsInDeck.Count())
            {
                numberOfCards = cardsInDeck.Count();
            }
            var cards = cardsInDeck.GetRange(0, numberOfCards);
            cardsInDeck.RemoveRange(0, numberOfCards);
            _announcements.Add($"Dealer has removed {numberOfCards} cards from the draw pile and is giving them to {player.Name}. Draw pile now has {cardsInDeck.Count()} cards.");
            player.Handle(new DealerToPlayerGiveCards(cards));
        }

        private int WhosTurnIsItIndex = 0;

        private void TellNextPlayerToTakeTurn()
        {
            var player = _table.Players[WhosTurnIsItIndex];
            WhosTurnIsItIndex++;
            if (WhosTurnIsItIndex == _table.Players.Count())
            {
                WhosTurnIsItIndex = 0;
            }
            _announcements.Add($"Dealer is telling {player.Name} that it is their turn.");
            player.Handle(new DealerToPlayerItsYourTurn());
        }

        private bool IsGameOver()
        {
            // Game is over when the Draw Pile is empty and every player's hand is empty.
            // When "Shark" is added to the game, then there will be one player with a card left.
            int cardsInPlay = cardsInDeck.Count + _table.Players.Sum(x => x.NumberCardsInHand);            
            return cardsInPlay < 2;
        }

        private void WhoWon()
        {
            _announcements.Add("The game is over.");
            foreach(var player in _table.Players)
            {
                _announcements.Add($"{player.Name} has laid down {player.PairsOnTable.Count()} pairs.");
                foreach(var pair in player.PairsOnTable)
                {
                    _announcements.Add($"{pair.Item1.Fish}, {pair.Item2.Fish}");
                }
            }

            _announcements.ReadAll();
        }
    }
}
