using System;
using UnityEngine;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerPrefsLevelProgressStore
    {
        private const string CompletedLevelCountKey =
            "WaterSortPuzzle.Progress.CompletedLevelCount";
        private const int MinimumCompletedLevelCount = 0;

        public int LoadCompletedLevelCount(int levelCount)
        {
            ValidateLevelCount(levelCount);

            int savedCompletedLevelCount = PlayerPrefs.GetInt(
                CompletedLevelCountKey,
                MinimumCompletedLevelCount);

            return Mathf.Clamp(
                savedCompletedLevelCount,
                MinimumCompletedLevelCount,
                levelCount);
        }

        public void SaveCompletedLevelCount(
            int completedLevelCount,
            int levelCount)
        {
            ValidateCompletedLevelCount(completedLevelCount, levelCount);

            PlayerPrefs.SetInt(CompletedLevelCountKey, completedLevelCount);
            PlayerPrefs.Save();
        }

        private static void ValidateLevelCount(int levelCount)
        {
            if (levelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levelCount),
                    "Level count must be greater than zero.");
            }
        }

        private static void ValidateCompletedLevelCount(
            int completedLevelCount,
            int levelCount)
        {
            if (completedLevelCount < MinimumCompletedLevelCount ||
                completedLevelCount > levelCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(completedLevelCount),
                    "Completed level count must be between " +
                    $"{MinimumCompletedLevelCount} and {levelCount}.");
            }
        }
    }
}
