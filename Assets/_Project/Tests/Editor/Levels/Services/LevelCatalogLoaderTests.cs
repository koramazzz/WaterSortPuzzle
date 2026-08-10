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
        public void TryLoad_WithAvailableLevel_LoadsNextLevel()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelCatalogLoader loader = new LevelCatalogLoader();

            bool wasLoaded = loader.TryLoad(catalog, 0, out LevelState state);

            Assert.That(wasLoaded, Is.True);
            Assert.That(state, Is.Not.Null);
            Assert.That(state.LevelNumber, Is.EqualTo(1));
            Assert.That(state.BottleCapacity, Is.EqualTo(3));
            Assert.That(state.Bottles.Count, Is.EqualTo(3));
        }

        [Test]
        public void TryLoad_WithNullCatalog_ThrowsArgumentNullException()
        {
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentNullException>(
                () => loader.TryLoad(null, 0, out _));
        }

        [Test]
        public void TryLoad_WithNegativeCompletedCount_ThrowsArgumentOutOfRangeException()
        {
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => loader.TryLoad(FindCatalog(), -1, out _));
        }

        [Test]
        public void TryLoad_WithAllLevelsCompleted_ReturnsFalse()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelCatalogLoader loader = new LevelCatalogLoader();

            bool wasLoaded = loader.TryLoad(
                catalog,
                catalog.LevelFiles.Count,
                out LevelState state);

            Assert.That(wasLoaded, Is.False);
            Assert.That(state, Is.Null);
        }

        [Test]
        public void TryLoad_WithCompletedCountAboveRange_ThrowsArgumentOutOfRangeException()
        {
            LevelFileCatalog catalog = FindCatalog();
            LevelCatalogLoader loader = new LevelCatalogLoader();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => loader.TryLoad(
                    catalog,
                    catalog.LevelFiles.Count + 1,
                    out _));
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
