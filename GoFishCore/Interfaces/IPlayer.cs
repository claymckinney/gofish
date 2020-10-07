using System.Collections.ObjectModel;

namespace GoFishCore
{
    public interface IPlayer
    {
        ReadOnlyCollection<(ICard, ICard)> PairsOnTable { get; }
        int NumberCardsInHand { get; }
        string Name { get; set; }
        void Handle(DealerToPlayerDealCards message);
        void Handle(DealerToPlayerGiveCard message);
        void Handle(DealerToPlayerGiveCards message);
        void Handle(DealerToPlayerItsYourTurn message);
        void Handle(DealerToPlayerNoCardsLeft message);
        void Handle(DealerToPlayerResetHand message);
        void Handle(PlayerToPlayerGimmeFish message);
        void Handle(PlayerToPlayerGiveCard message);
        void Handle(PlayerToPlayerGoFish message);
    }
}
