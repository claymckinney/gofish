using System.Collections.ObjectModel;
using System.IO;

namespace GoFishCore
{
    public interface ITable
    {
        ReadOnlyCollection<IPlayer> Players { get; }

        void RegisterPlayer(IPlayer player);
    }
}
