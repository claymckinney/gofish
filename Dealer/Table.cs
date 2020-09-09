using GoFishCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GoFishActors
{
    public class Table : ITable
    {
        // The table knows who asked who for what, and the result. Messages to the table are public.
        // The table knows which pairs are face up on the table in front of each player.
        // What the table knows, we all know. It's public.
        // The table knows the score. 
        // The table knows how many players there are in the game. 2-8 players. Knows their names.

        private readonly List<IPlayer> _players;
        public ReadOnlyCollection<IPlayer> Players { get; private set; }

        public Table()
        {
            _players = new List<IPlayer>();
            Players = _players.AsReadOnly();
        }

        public void RegisterPlayer(IPlayer player)
        {
            _players.Add(player);
        }
    }
}
