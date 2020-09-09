namespace GoFishCore
{
    public class PlayerToDealerTurnOver : IMessage
    {
        public PlayerToDealerTurnOver(IPlayer sender)
        {
            Player = sender;
        }

        public IPlayer Player { get; }
    }
}
