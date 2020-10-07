namespace GoFishCore
{
    public class PlayerToDealerAskForCards : IMessage
    {
        public PlayerToDealerAskForCards(IPlayer sender, int number)
        {
            Sender = sender;
            Number = number;
        }

        public IPlayer Sender { get; }
        public int Number { get; }
    }
}
