using System;
using System.Collections.Generic;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Gameplay.Levels
{
    public sealed class LevelState
    {
        private readonly List<BottleState> bottles;
        private readonly IReadOnlyList<BottleState> readOnlyBottles;

        public LevelState(LevelData initialData)
        {
            if (initialData == null)
            {
                throw new ArgumentNullException(nameof(initialData));
            }

            LevelNumber = initialData.LevelNumber;
            BottleCapacity = initialData.BottleCapacity;
            Difficulty = initialData.Difficulty;
            bottles = new List<BottleState>(initialData.Bottles.Count);

            foreach (BottleData bottleData in initialData.Bottles)
            {
                bottles.Add(new BottleState(BottleCapacity, bottleData));
            }

            readOnlyBottles = bottles.AsReadOnly();
        }

        public int LevelNumber { get; }

        public int BottleCapacity { get; }

        public LevelDifficulty Difficulty { get; }

        public IReadOnlyList<BottleState> Bottles => readOnlyBottles;

        public bool IsCompleted
        {
            get
            {
                foreach (BottleState bottle in bottles)
                {
                    if (!bottle.IsSorted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
