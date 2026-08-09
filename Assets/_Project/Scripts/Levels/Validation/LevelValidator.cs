using System;
using System.Collections.Generic;

namespace WaterSortPuzzle.Levels.Validation
{
    public sealed class LevelValidator
    {
        public IReadOnlyList<string> Validate(LevelData level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            List<string> errors = new List<string>();
            string levelContext = level.LevelNumber > 0
                ? $"Level {level.LevelNumber}"
                : "Level";

            if (level.LevelNumber <= 0)
            {
                errors.Add("Level must have a positive level number.");
            }

            if (level.BottleCapacity <= 0)
            {
                errors.Add($"{levelContext} must have a positive bottle capacity.");
            }

            ValidateBottles(level, levelContext, errors);

            return errors.AsReadOnly();
        }

        private static void ValidateBottles(
            LevelData level,
            string levelContext,
            ICollection<string> errors)
        {
            IReadOnlyList<BottleData> bottles = level.Bottles;

            if (bottles == null)
            {
                errors.Add($"{levelContext}'s bottles collection cannot be null.");
                return;
            }

            if (bottles.Count == 0)
            {
                errors.Add($"{levelContext} must contain at least one bottle.");
                return;
            }

            for (int bottleIndex = 0; bottleIndex < bottles.Count; bottleIndex++)
            {
                BottleData bottle = bottles[bottleIndex];
                string bottleContext = $"{levelContext}, bottle index {bottleIndex}";

                if (bottle == null)
                {
                    errors.Add($"{bottleContext} cannot be null.");
                    continue;
                }

                ValidateBottle(bottle, level.BottleCapacity, bottleContext, errors);
            }
        }

        private static void ValidateBottle(
            BottleData bottle,
            int bottleCapacity,
            string bottleContext,
            ICollection<string> errors)
        {
            IReadOnlyList<string> liquidIds = bottle.LiquidIdsBottomToTop;

            if (liquidIds == null)
            {
                errors.Add($"{bottleContext}'s liquid IDs collection cannot be null.");
            }
            else
            {
                if (bottleCapacity > 0 && liquidIds.Count > bottleCapacity)
                {
                    errors.Add(
                        $"{bottleContext} contains {liquidIds.Count} liquids, " +
                        $"but its capacity is {bottleCapacity}.");
                }

                ValidateLiquidIds(liquidIds, bottleContext, errors);
            }

            ValidateHiddenLiquidIndices(
                bottle.HiddenLiquidIndices,
                liquidIds,
                bottleContext,
                errors);
        }

        private static void ValidateLiquidIds(
            IReadOnlyList<string> liquidIds,
            string bottleContext,
            ICollection<string> errors)
        {
            for (int liquidIndex = 0; liquidIndex < liquidIds.Count; liquidIndex++)
            {
                if (string.IsNullOrWhiteSpace(liquidIds[liquidIndex]))
                {
                    errors.Add(
                        $"{bottleContext} has an empty liquid ID at index {liquidIndex}.");
                }
            }
        }

        private static void ValidateHiddenLiquidIndices(
            IReadOnlyList<int> hiddenLiquidIndices,
            IReadOnlyList<string> liquidIds,
            string bottleContext,
            ICollection<string> errors)
        {
            if (hiddenLiquidIndices == null)
            {
                errors.Add(
                    $"{bottleContext}'s hidden liquid indices collection cannot be null.");
                return;
            }

            HashSet<int> uniqueIndices = new HashSet<int>();

            for (int index = 0; index < hiddenLiquidIndices.Count; index++)
            {
                int hiddenLiquidIndex = hiddenLiquidIndices[index];

                if (!uniqueIndices.Add(hiddenLiquidIndex))
                {
                    errors.Add(
                        $"{bottleContext} contains duplicate hidden liquid index " +
                        $"{hiddenLiquidIndex}.");
                }

                if (liquidIds != null && liquidIds.Count == 0)
                {
                    errors.Add(
                        $"{bottleContext} contains hidden liquid index {hiddenLiquidIndex}, " +
                        "but the bottle has no liquids.");
                }
                else if (liquidIds != null &&
                         (hiddenLiquidIndex < 0 || hiddenLiquidIndex >= liquidIds.Count))
                {
                    errors.Add(
                        $"{bottleContext} contains hidden liquid index {hiddenLiquidIndex}, " +
                        $"but its valid range is 0 to {liquidIds.Count - 1}.");
                }
                else if (liquidIds != null &&
                         hiddenLiquidIndex == liquidIds.Count - 1)
                {
                    errors.Add(
                        $"{bottleContext}'s top liquid cannot be hidden.");
                }
            }
        }
    }
}
