using System;
using UnityEngine;
using WaterSortPuzzle.Levels.Sources;

namespace WaterSortPuzzle.Gameplay.Levels.Loading
{
    public sealed class LevelCatalogLoader
    {
        private readonly LevelLoader levelLoader = new LevelLoader();

        public LevelState Load(LevelFileCatalog catalog, int levelIndex)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (levelIndex < 0 || levelIndex >= catalog.LevelFiles.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(levelIndex),
                    levelIndex,
                    $"Level index is outside the catalog range. " +
                    $"The catalog contains {catalog.LevelFiles.Count} level files.");
            }

            TextAsset levelFile = catalog.LevelFiles[levelIndex];

            if (levelFile == null)
            {
                throw new InvalidOperationException(
                    $"Level catalog contains an empty file at index {levelIndex}.");
            }

            return levelLoader.Load(levelFile);
        }
    }
}
