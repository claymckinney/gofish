using System.Collections.ObjectModel;

namespace GoFishCore
{
    public interface IPlayer : IActor
    {
        ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; }
        int NumberCardsInHand { get; }
        string Name { get; set; }
    }
}
