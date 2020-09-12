using GoFishCore;
using System;

namespace GoFishConsoleApp
{
    internal class IntegrationTest : IIntegrationTest
    {
        public IntegrationTest(Func<PlayerType, IPlayer> playerResolver, IDealer dealer)
        {
            var player1 = playerResolver(PlayerType.Computer);
            player1.Name = "Cody";
            var player2 = playerResolver(PlayerType.Computer);
            player2.Name = "Deb";
            var player3 = playerResolver(PlayerType.Computer);
            player3.Name = "Joel";
            var player4 = playerResolver(PlayerType.Computer);
            player4.Name = "Robert";
            var player5 = playerResolver(PlayerType.Computer);
            player5.Name = "Clay";
            dealer.RegisterPlayer(player1);
            dealer.RegisterPlayer(player2);
            dealer.RegisterPlayer(player3);
            dealer.RegisterPlayer(player4);
            dealer.RegisterPlayer(player5);
            dealer.StartGame();
        }
    }
}
