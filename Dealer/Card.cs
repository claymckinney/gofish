using GoFishCore;

namespace GoFishActors
{
    public class Card : ICard
    {
        public int ID { get; }

        public string DisplayName { get; }

        public Fish Fish { get; }

        public Card(int id, string displayName, Fish fish)
        {
            ID = id;
            DisplayName = displayName;
            Fish = fish;
        }
    }
}
