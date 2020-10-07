namespace GoFishCore
{
    public interface ICard
    {
        int ID { get; }
        string DisplayName { get; }

        Fish Fish { get; }
    }
}
