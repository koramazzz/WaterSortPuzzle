using System;
using NUnit.Framework;
using UnityEditor;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Gameplay.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels
{
    public sealed class LevelCatalogLoaderTests
    {
        [Test]
        public void Load_WithValidIndex_LoadsSelectedLevel()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelCatalogLoader loader = new LevelCatalogLoader();

            LevelState state = loader.Load(catalog, 0);

            Assert.That(state.LevelNumber, Is.EqualTo(1));
            Assert.That(state.BottleCapacity, Is.EqualTo(4));
            Assert.That(state.Bottles.Count, Is.EqualTo(5));
        }

        [Test]
        public void Load_WithNullCatalog_ThrowsArgumentNullException()
        {
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentNullException>(() => loader.Load(null, 0));
        }

        [Test]
        public void Load_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => loader.Load(FindCatalog(), -1));
        }

        [Test]
        public void Load_WithIndexEqualToCount_ThrowsArgumentOutOfRangeException()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => loader.Load(catalog, catalog.LevelFiles.Count));
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
