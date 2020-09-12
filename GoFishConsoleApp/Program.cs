using GoFishCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace GoFishConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any()) return;

            var collection = new ServiceCollection();
            ConfigureServices(collection, args[0]);

            IServiceProvider serviceProvider = collection.BuildServiceProvider();

            if (args[0] == "test")
            {
                serviceProvider.GetService<IIntegrationTest>();
            }
            else
            {
                serviceProvider.GetService<IGameWithPlayer>();
            }

            // TODO:
            // Without viewing the log, play 100 games and see how many times each player won. Should be evenly distributed.
            // Between games, reset each players cards in hand and pairs laid down to empty. DealerToPlayerStartNewGame message.

            if (serviceProvider is IDisposable)
            {
                ((IDisposable)serviceProvider).Dispose();
            }
        }

        private static void ConfigureServices(IServiceCollection services, string PlayerName)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddScoped<IDealer, GoFishActors.Dealer>();
            services.AddLogging(configure => configure.AddConsole())
                .AddScoped<Func<PlayerType, IPlayer>>(playerProvider => key =>
            {
                switch (key)
                {
                    case PlayerType.Computer:
                        return new GoFishActors.Player(
                            playerProvider.GetRequiredService<IDealer>(),
                            playerProvider.GetRequiredService<ILogger<GoFishActors.Player>>());
                    case PlayerType.Human:
                        return new GoFishHumanPlayer.Player(
                            playerProvider.GetRequiredService<IDealer>(),
                            playerProvider.GetRequiredService<ILogger<GoFishHumanPlayer.Player>>());
                    default:
                        return null;
                }
            });
            services.AddTransient<IIntegrationTest, IntegrationTest>();
            services.AddTransient<IGameWithPlayer>(x =>
                new GameWithPlayer(x.GetRequiredService<Func<PlayerType, IPlayer>>(),
                                    x.GetRequiredService<IDealer>(),
                                    PlayerName));
        }
    }
}
