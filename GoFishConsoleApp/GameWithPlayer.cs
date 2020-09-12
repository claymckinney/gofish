using GoFishCore;
using System;

namespace GoFishConsoleApp
{
    internal class GameWithPlayer : IGameWithPlayer
    {
        internal GameWithPlayer(Func<PlayerType, IPlayer> playerResolver, IDealer dealer, string PlayerName)
        {
            var player1 = playerResolver(PlayerType.Computer);
            player1.Name = "Cody";
            var player2 = playerResolver(PlayerType.Computer);
            player2.Name = "Deb";
            var player3 = playerResolver(PlayerType.Computer);
            player3.Name = "Joel";
            var player4 = playerResolver(PlayerType.Computer);
            player4.Name = "Robert";
            var player5 = playerResolver(PlayerType.Human);
            player5.Name = PlayerName;
            dealer.RegisterPlayer(player1);
            dealer.RegisterPlayer(player2);
            dealer.RegisterPlayer(player3);
            dealer.RegisterPlayer(player4);
            dealer.RegisterPlayer(player5);
            dealer.StartGame();
        }
    }
}
