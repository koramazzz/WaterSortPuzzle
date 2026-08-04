using System;
using UnityEngine;

namespace WaterSortPuzzle.Levels.Loading
{
    public sealed class LevelDataLoader
    {
        public LevelData Load(TextAsset levelFile)
        {
            if (levelFile == null)
            {
                throw new ArgumentNullException(nameof(levelFile));
            }

            if (string.IsNullOrWhiteSpace(levelFile.text))
            {
                throw new InvalidOperationException(
                    $"Level file '{levelFile.name}' is empty.");
            }

            try
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(levelFile.text);

                return levelData ?? throw new InvalidOperationException(
                    $"Level file '{levelFile.name}' could not be deserialized.");
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(
                    $"Level file '{levelFile.name}' contains invalid JSON.",
                    exception);
            }
        }
    }
}
