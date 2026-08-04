using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Loading;

namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    public sealed class LevelDataLoaderTests
    {
        [Test]
        public void Load_WithValidJson_DeserializesLevelData()
        {
            TextAsset levelFile = new TextAsset(LevelJsonSamples.ValidLevel)
            {
                name = "valid_level"
            };

            try
            {
                LevelDataLoader loader = new LevelDataLoader();

                LevelData level = loader.Load(levelFile);

                Assert.That(level.LevelNumber, Is.EqualTo(7));
                Assert.That(level.BottleCapacity, Is.EqualTo(4));
                Assert.That(level.Bottles.Count, Is.EqualTo(2));
                Assert.That(
                    level.Bottles[0].LiquidIdsBottomToTop,
                    Is.EqualTo(new[] { "red", "blue" }));
                Assert.That(
                    level.Bottles[0].HiddenLiquidIndices,
                    Is.EqualTo(new[] { 1 }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(levelFile);
            }
        }

        [Test]
        public void Load_WithNullFile_ThrowsArgumentNullException()
        {
            LevelDataLoader loader = new LevelDataLoader();

            Assert.Throws<ArgumentNullException>(() => loader.Load(null));
        }

        [Test]
        public void Load_WithEmptyFile_ThrowsInvalidOperationException()
        {
            TextAsset levelFile = new TextAsset("  \n\t")
            {
                name = "empty_level"
            };

            try
            {
                LevelDataLoader loader = new LevelDataLoader();

                InvalidOperationException exception =
                    Assert.Throws<InvalidOperationException>(() => loader.Load(levelFile));

                Assert.That(exception.Message, Does.Contain("empty_level"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(levelFile);
            }
        }

        [Test]
        public void Load_WithMalformedJson_ThrowsInvalidOperationException()
        {
            TextAsset levelFile = new TextAsset("{\"levelNumber\":")
            {
                name = "malformed_level"
            };

            try
            {
                LevelDataLoader loader = new LevelDataLoader();

                InvalidOperationException exception =
                    Assert.Throws<InvalidOperationException>(() => loader.Load(levelFile));

                Assert.That(exception.Message, Does.Contain("malformed_level"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(levelFile);
            }
        }
    }
}
