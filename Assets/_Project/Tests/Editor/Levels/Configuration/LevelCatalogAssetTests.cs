using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Levels.Validation;

namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    public sealed class LevelCatalogAssetTests
    {
        [Test]
        public void Catalog_ContainsUniqueValidLevelFiles()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelDataLoader loader = new LevelDataLoader();
            LevelValidator validator = new LevelValidator();
            HashSet<int> levelNumbers = new HashSet<int>();

            Assert.That(catalog.LevelFiles, Is.Not.Empty);

            foreach (TextAsset levelFile in catalog.LevelFiles)
            {
                Assert.That(levelFile, Is.Not.Null);

                LevelData level = loader.Load(levelFile);
                IReadOnlyList<string> errors = validator.Validate(level);

                Assert.That(
                    errors,
                    Is.Empty,
                    $"{levelFile.name} is invalid:\n{string.Join("\n", errors)}");
                Assert.That(
                    levelNumbers.Add(level.LevelNumber),
                    Is.True,
                    $"Level number {level.LevelNumber} is duplicated.");
            }
        }

        private static LevelFileCatalog FindCatalog()
        {
            string[] catalogGuids = AssetDatabase.FindAssets("t:LevelFileCatalog");

            Assert.That(
                catalogGuids.Length,
                Is.EqualTo(1),
                "The project must contain exactly one LevelFileCatalog asset.");

            string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
            LevelFileCatalog catalog =
                AssetDatabase.LoadAssetAtPath<LevelFileCatalog>(catalogPath);

            Assert.That(catalog, Is.Not.Null);
            return catalog;
        }
    }
}
