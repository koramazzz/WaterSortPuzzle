using System;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerResourcesSaveData
    {

        public PlayerResourcesSaveData(
            int gold,
            int lives,
            long nextLifeTimestamp)
        {
            if (gold < GameBalance.MinimumGold)
            {
                throw new ArgumentOutOfRangeException(nameof(gold));
            }

            if (lives < GameBalance.MinimumLives || lives > GameBalance.MaximumLives)
            {
                throw new ArgumentOutOfRangeException(nameof(lives));
            }

            if (nextLifeTimestamp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nextLifeTimestamp));
            }

            Gold = gold;
            Lives = lives;
            NextLifeTimestamp = nextLifeTimestamp;
        }

        public int Gold { get; }

        public int Lives { get; }

        public long NextLifeTimestamp { get; }
    }
}
