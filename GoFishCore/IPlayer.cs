using System.Collections.ObjectModel;

namespace GoFishCore
{
    public interface IPlayer<T> where T : class, IPlayer<T>, IActor
    {
        ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; }
        int NumberCardsInHand { get; }
        string Name { get; set; }
    }
}
