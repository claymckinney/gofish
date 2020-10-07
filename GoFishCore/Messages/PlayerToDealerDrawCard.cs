namespace GoFishCore
{
    public class PlayerToDealerDrawCard : IMessage
    {
        public PlayerToDealerDrawCard(IPlayer sender)
        {
            Sender = sender;
        }

        public IPlayer Sender { get; }
    }
}
