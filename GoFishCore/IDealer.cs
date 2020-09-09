using System.Collections.ObjectModel;

namespace GoFishCore
{
    public interface IDealer : IActor
    {
        ReadOnlyCollection<IPlayer> Players { get; }

        void RegisterPlayer(IPlayer player);

        void StartGame();
    }
}
