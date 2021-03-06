﻿using GoFishCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolsCore;

namespace GoFishActors
{
    public class Dealer : IDealer
    {
        // The dealer has all the cards. Shuffles. Deals. Gives the top card when they "Go Fish". Gives cards for them to "Draw up to 5"
        // The dealer knows how many cards are left.
        // The dealer is not a player.
        // The dealer knows who all the players are.
        // The dealer knows whose turn it is.

        private List<ICard> cardsInDeck = new List<ICard>(); // It's the "deck" during the deal, but then it becomes the "draw pile". Same thing.
        private readonly ILogger _logger;

        private readonly List<IPlayer> _players;
        public ReadOnlyCollection<IPlayer> Players { get; private set; }

        public Dealer(ILogger<Dealer> logger)
        {
            _logger = logger;
            _players = new List<IPlayer>();
            Players = _players.AsReadOnly();
        }

        public void RegisterPlayer(IPlayer player)
        {
            _players.Add(player);
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
            foreach(var player in Players)
            {
                player.Handle(new DealerToPlayerResetHand());
            }
            foreach(var player in Players)
            {
                DealCardsToPlayer(5, player);
            }
            TellNextPlayerToTakeTurn();
        }

        public void Handle(PlayerToDealerAskForCards message)
        {
            switch (message)
            {
                case PlayerToDealerAskForCards cardsRequest when cardsInDeck.Any():
                    GiveCardsToPlayer(cardsRequest.Number, cardsRequest.Sender);
                    break;
                case PlayerToDealerAskForCards cardsRequest:
                    _logger.LogInformation($"There are no cards left in the draw pile.");
                    cardsRequest.Sender.Handle(new DealerToPlayerNoCardsLeft());
                    break;
                default:
                    _logger.LogInformation($"Dealer recieved an unhandled PlayerToDealerAskForCards message. {message}");
                    break;
            }
        }

        public void Handle(PlayerToDealerDrawCard message)
        {
            switch (message)
            {
                case PlayerToDealerDrawCard cardRequest when cardsInDeck.Any():
                    var card = cardsInDeck.FirstOrDefault();
                    cardsInDeck.Remove(card);
                    _logger.LogInformation($"Dealer gives a card to {cardRequest.Sender.Name}. There are now {cardsInDeck.Count()} cards left in the draw pile.");
                    cardRequest.Sender.Handle(new DealerToPlayerGiveCard(card));
                    break;
                case PlayerToDealerDrawCard cardRequest:
                    _logger.LogInformation($"There are no cards left in the draw pile.");
                    cardRequest.Sender.Handle(new DealerToPlayerNoCardsLeft());
                    break;
                default:
                    _logger.LogInformation($"Dealer recieved an unhandled PlayerToDealerDrawCard message. {message}");
                    break;
            }
        }

        public void Handle(PlayerToDealerTurnOver message)
        {
            if (!IsGameOver())
            {
                TellNextPlayerToTakeTurn();
            }
            else
            {
                WhoWon();
            }
        }

        private void DealCardsToPlayer(int numberOfCards, IPlayer player)
        {
            var cards = cardsInDeck.GetRange(0, numberOfCards);
            cardsInDeck.RemoveRange(0, numberOfCards);
            _logger.LogInformation($"Dealer has removed 5 cards from the deck and is dealing them to {player.Name}");
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
            _logger.LogInformation($"Dealer has removed {numberOfCards} cards from the draw pile and is giving them to {player.Name}. Draw pile now has {cardsInDeck.Count()} cards.");
            player.Handle(new DealerToPlayerGiveCards(cards));
        }

        private int WhosTurnIsItIndex = 0;

        private void TellNextPlayerToTakeTurn()
        {
            var player = Players[WhosTurnIsItIndex];
            WhosTurnIsItIndex++;
            if (WhosTurnIsItIndex == Players.Count())
            {
                WhosTurnIsItIndex = 0;
            }
            _logger.LogInformation($"Dealer is telling {player.Name} that it is their turn.");
            player.Handle(new DealerToPlayerItsYourTurn());
        }

        private bool IsGameOver()
        {
            // Game is over when the Draw Pile is empty and every player's hand is empty.
            // When "Shark" is added to the game, then there will be one player with a card left.
            int cardsInPlay = cardsInDeck.Count + Players.Sum(x => x.NumberCardsInHand);            
            return cardsInPlay < 2;
        }

        private void WhoWon()
        {
            _logger.LogInformation("The game is over.");
            foreach(var player in Players)
            {
                _logger.LogInformation($"{player.Name} has laid down {player.PairsOnTable.Count()} pairs.");
                foreach(var pair in player.PairsOnTable)
                {
                    _logger.LogInformation($"{pair.Item1.Fish}, {pair.Item2.Fish}");
                }
            }
        }
    }
}
