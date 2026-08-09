using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Gameplay.Levels.Loading;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels
{
    public sealed class LevelLoaderTests
    {
        [Test]
        public void Load_WithValidFile_CreatesLevelState()
        {
            const string json = @"
            {
              ""levelNumber"": 7,
              ""bottleCapacity"": 4,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": [0]
                },
                {
                  ""liquidIdsBottomToTop"": [],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";
            TextAsset levelFile = new TextAsset(json)
            {
                name = "valid_level"
            };

            try
            {
                LevelLoader loader = new LevelLoader();

                LevelState state = loader.Load(levelFile);

                Assert.That(state.LevelNumber, Is.EqualTo(7));
                Assert.That(state.BottleCapacity, Is.EqualTo(4));
                Assert.That(state.Bottles.Count, Is.EqualTo(2));
                Assert.That(state.Bottles[0].TopLiquidId, Is.EqualTo("blue"));
                Assert.That(state.Bottles[0].IsLiquidHidden(0), Is.True);
                Assert.That(state.Bottles[1].IsEmpty, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(levelFile);
            }
        }

        [Test]
        public void Load_WithInvalidLevel_ThrowsAllValidationErrors()
        {
            const string json = @"
            {
              ""levelNumber"": 0,
              ""bottleCapacity"": 0,
              ""bottles"": []
            }";
            TextAsset levelFile = new TextAsset(json)
            {
                name = "invalid_level"
            };

            try
            {
                LevelLoader loader = new LevelLoader();

                InvalidOperationException exception =
                    Assert.Throws<InvalidOperationException>(
                        () => loader.Load(levelFile));

                Assert.That(exception.Message, Does.Contain("invalid_level"));
                Assert.That(exception.Message, Does.Contain("positive level number"));
                Assert.That(exception.Message, Does.Contain("positive bottle capacity"));
                Assert.That(exception.Message, Does.Contain("at least one bottle"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(levelFile);
            }
        }

        [Test]
        public void Load_WithNullFile_ThrowsArgumentNullException()
        {
            LevelLoader loader = new LevelLoader();

            Assert.Throws<ArgumentNullException>(() => loader.Load(null));
        }
    }
}
