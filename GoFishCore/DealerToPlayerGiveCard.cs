namespace GoFishCore
{
    public class DealerToPlayerGiveCard : IMessage
    {
        public DealerToPlayerGiveCard(ICard card)
        {
            Card = card;
        }

        public ICard Card { get; }
    }
}
