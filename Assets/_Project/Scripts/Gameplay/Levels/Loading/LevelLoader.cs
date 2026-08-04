using System;
using System.Collections.Generic;
using UnityEngine;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Loading;
using WaterSortPuzzle.Levels.Validation;

namespace WaterSortPuzzle.Gameplay.Levels.Loading
{
    public sealed class LevelLoader
    {
        private readonly LevelDataLoader dataLoader = new LevelDataLoader();
        private readonly LevelValidator validator = new LevelValidator();

        public LevelState Load(TextAsset levelFile)
        {
            LevelData levelData = dataLoader.Load(levelFile);
            IReadOnlyList<string> validationErrors = validator.Validate(levelData);

            if (validationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Level file '{levelFile.name}' is invalid:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, validationErrors));
            }

            return new LevelState(levelData);
        }
    }
}
