namespace GoFishCore
{
    public class PlayerToPlayerGoFish : IMessage
    {
        public PlayerToPlayerGoFish(IPlayer sender)
        {
            Sender = sender;
        }

        public IPlayer Sender { get; }
    }
}
