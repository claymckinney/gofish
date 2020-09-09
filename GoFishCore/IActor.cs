namespace GoFishCore
{
    public interface IActor
    {
        void Handle(IMessage message);
    }
}
