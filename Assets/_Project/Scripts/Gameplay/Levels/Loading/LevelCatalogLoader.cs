using System;
using UnityEngine;
using WaterSortPuzzle.Levels.Sources;

namespace WaterSortPuzzle.Gameplay.Levels.Loading
{
    public sealed class LevelCatalogLoader
    {
        private readonly LevelLoader levelLoader = new LevelLoader();

        public bool TryLoad(
            LevelFileCatalog catalog,
            int completedLevelCount,
            out LevelState levelState)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (completedLevelCount < 0 ||
                completedLevelCount > catalog.LevelFiles.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(completedLevelCount),
                    completedLevelCount,
                    $"Completed level count is outside the catalog range. " +
                    $"The catalog contains {catalog.LevelFiles.Count} level files.");
            }

            if (completedLevelCount == catalog.LevelFiles.Count)
            {
                levelState = null;
                return false;
            }

            TextAsset levelFile = catalog.LevelFiles[completedLevelCount];

            if (levelFile == null)
            {
                throw new InvalidOperationException(
                    $"Level catalog contains an empty file at index " +
                    $"{completedLevelCount}.");
            }

            levelState = levelLoader.Load(levelFile);
            return true;
        }
    }
}
