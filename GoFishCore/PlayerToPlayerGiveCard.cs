namespace GoFishCore
{
    public class PlayerToPlayerGiveCard : IMessage
    {
        public PlayerToPlayerGiveCard(IPlayer sender, ICard card)
        {
            Sender = sender;
            Card = card;
        }

        public IPlayer Sender { get; }
        public ICard Card { get; }
    }
}
