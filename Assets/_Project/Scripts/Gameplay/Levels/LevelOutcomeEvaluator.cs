using System;
using System.Collections.Generic;
using WaterSortPuzzle.Gameplay.Bottles;

namespace WaterSortPuzzle.Gameplay.Levels
{
    public sealed class LevelOutcomeEvaluator
    {
        private readonly BottlePourService pourService = new BottlePourService();

        public LevelOutcome Evaluate(LevelState levelState)
        {
            if (levelState == null)
            {
                throw new ArgumentNullException(nameof(levelState));
            }

            if (levelState.IsCompleted)
            {
                return LevelOutcome.Completed;
            }

            IReadOnlyList<BottleState> bottles = levelState.Bottles;

            foreach (BottleState bottle in bottles)
            {
                if (bottle.IsEmpty)
                {
                    return LevelOutcome.InProgress;
                }
            }

            for (int sourceIndex = 0; sourceIndex < bottles.Count; sourceIndex++)
            {
                for (int destinationIndex = 0;
                     destinationIndex < bottles.Count;
                     destinationIndex++)
                {
                    if (sourceIndex == destinationIndex)
                    {
                        continue;
                    }

                    if (pourService.CanPour(
                            bottles[sourceIndex],
                            bottles[destinationIndex]))
                    {
                        return LevelOutcome.InProgress;
                    }
                }
            }

            return LevelOutcome.Failed;
        }
    }
}
