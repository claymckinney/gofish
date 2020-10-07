using System.Collections.ObjectModel;

namespace GoFishCore
{
    public interface IDealer
    {
        ReadOnlyCollection<IPlayer> Players { get; }

        void RegisterPlayer(IPlayer player);

        void StartGame();
        void Handle(PlayerToDealerAskForCards message);
        void Handle(PlayerToDealerDrawCard message);
        void Handle(PlayerToDealerTurnOver message);
    }
}
