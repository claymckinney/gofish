using GoFishCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoFishHumanPlayer
{
    public class Player : PlayerBase, IPlayer, IActor
    {
        public Player(IDealer dealer, ILogger<Player> logger) : base(dealer, logger)
        {
        }

        protected override void AskForFish()
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
    }
}
