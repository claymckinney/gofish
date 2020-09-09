namespace GoFishCore
{
    public class PlayerToPlayerGimmeFish : IMessage
    {
        public PlayerToPlayerGimmeFish(IPlayer sender, Fish fish)
        {
            Sender = sender;
            Fish = fish;
        }

        public IPlayer Sender { get; }
        public Fish Fish { get; }
    }
}
