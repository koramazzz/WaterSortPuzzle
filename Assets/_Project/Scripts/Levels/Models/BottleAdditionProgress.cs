using System;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Gameplay.Levels
{
    public sealed class BottleAdditionProgress
    {
        private int addedBottleCount;

        public int AddedBottleCount => addedBottleCount;

        public int RemainingAdditionCount => GameBalance.MaximumBottleAdditionsPerLevel - addedBottleCount;

        public bool CanAddBottle => RemainingAdditionCount > 0;

        public void RecordBottleAdded()
        {
            if (!CanAddBottle)
            {
                throw new InvalidOperationException("The bottle addition limit has been reached.");
            }

            addedBottleCount++;
        }
    }
}
