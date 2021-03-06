﻿using GoFishCore;
using Microsoft.Extensions.Logging;

namespace GoFishActors
{
    public class Player : PlayerBase, IPlayer
    {
        public Player(IDealer dealer, ILogger<Player> logger) : base(dealer, logger)
        {
        }
    }
}
