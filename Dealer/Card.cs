using GoFishCore;

namespace GoFishActors
{
    public class Card : ICard
    {   // I'd like to take the actual card, pass it around, make pairs on the table or give it to another player. But, that
        // isn't how it works. I create the card once and then pass around references, but many references to the
        // same card can exist. So I have to be careful. I have to destroy references as they go out of date.
        // Still, it's shared state. If I don't carefully remove my own reference to the card object from my hand
        // that I'm sending to the other player, we could both have a reference to the same card in our hand.
        // For a future improvement, I need a sentient deck to be the source of truth for who has which card.
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
