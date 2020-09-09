using GoFishActors;
using GoFishCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ToolsCore;

namespace GoFishConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new ServiceCollection();
            ConfigureServices(collection);

            IServiceProvider serviceProvider = collection.BuildServiceProvider();

            var table = serviceProvider.GetService<ITable>();
            var dealer = serviceProvider.GetService<IDealer>();
            var player1 = serviceProvider.GetService<IPlayer>();
            player1.Name = "Cody";
            var player2 = serviceProvider.GetService<IPlayer>();
            player2.Name = "Deb";
            var player3 = serviceProvider.GetService<IPlayer>();
            player3.Name = "Joel";
            var player4 = serviceProvider.GetService<IPlayer>();
            player4.Name = "Robert";
            var player5 = serviceProvider.GetService<IPlayer>();
            player5.Name = "Clay";
            table.RegisterPlayer(player1);
            table.RegisterPlayer(player2);
            table.RegisterPlayer(player3);
            table.RegisterPlayer(player4);
            table.RegisterPlayer(player5);
            dealer.StartGame();
            // How will console know when game is over?
            // View a log of all the moves in the game

            // Without viewing the log, play 100 games and see how many times each player won. Should be evenly distributed.
            // Between games, reset each players cards in hand and pairs laid down to empty. DealerToPlayerStartNewGame message.

            if (serviceProvider is IDisposable)
            {
                ((IDisposable)serviceProvider).Dispose();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddScoped<IAnnouncements, Announcements>();
            services.AddScoped<ITable, Table>();
            services.AddScoped<IDealer, Dealer>();
            services.AddTransient<IPlayer, Player>();
        }
    }
}
