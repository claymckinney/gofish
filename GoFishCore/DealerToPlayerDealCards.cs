using System.Collections.Generic;

namespace GoFishCore
{
    public class DealerToPlayerDealCards : IMessage
    {
        public DealerToPlayerDealCards(List<ICard> cards)
        {
            Cards = cards;
        }

        public List<ICard> Cards { get; }
    }
}
