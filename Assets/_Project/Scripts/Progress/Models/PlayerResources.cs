using System;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerResources
    {
        public PlayerResources(
            int gold,
            int lives,
            int secondsUntilNextLife)
        {
            if (gold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gold));
            }

            if (lives < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lives));
            }

            if (secondsUntilNextLife < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(secondsUntilNextLife));
            }

            Gold = gold;
            Lives = lives;
            SecondsUntilNextLife = secondsUntilNextLife;
        }

        public int Gold { get; }

        public int Lives { get; }

        public int SecondsUntilNextLife { get; }
    }
}
