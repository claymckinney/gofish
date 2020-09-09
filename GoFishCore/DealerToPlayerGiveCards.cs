using System.Collections.Generic;

namespace GoFishCore
{
    public class DealerToPlayerGiveCards : IMessage
    {
        public DealerToPlayerGiveCards(List<ICard> cards)
        {
            Cards = cards;
        }

        public List<ICard> Cards { get; }
    }
}
