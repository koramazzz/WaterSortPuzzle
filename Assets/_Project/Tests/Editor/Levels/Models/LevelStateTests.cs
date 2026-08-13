using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels
{
    public sealed class LevelStateTests
    {
        private const string LevelJson = @"
        {
          ""levelNumber"": 11,
          ""difficulty"": 1,
          ""bottleCapacity"": 5,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""blue""],
              ""hiddenLiquidIndices"": [0]
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [""green"", ""green"", ""yellow""],
              ""hiddenLiquidIndices"": [1]
            }
          ]
        }";

        private const string CompletedLevelJson = @"
        {
          ""levelNumber"": 12,
          ""difficulty"": 1,
          ""bottleCapacity"": 2,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""red""],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [""blue"", ""blue""],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            }
          ]
        }";

        [Test]
        public void Constructor_WithLevelData_CreatesExpectedLevelState()
        {
            LevelData levelData = Deserialize(LevelJson);

            LevelState state = new LevelState(levelData);

            Assert.That(state.LevelNumber, Is.EqualTo(11));
            Assert.That(state.BottleCapacity, Is.EqualTo(5));
            Assert.That(state.Difficulty, Is.EqualTo(LevelDifficulty.Easy));
            Assert.That(state.Bottles.Count, Is.EqualTo(3));
            Assert.That(
                state.Bottles[0].LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red", "blue" }));
            Assert.That(state.Bottles[0].Capacity, Is.EqualTo(5));
            Assert.That(state.Bottles[0].IsLiquidHidden(0), Is.True);
            Assert.That(state.Bottles[1].IsEmpty, Is.True);
            Assert.That(state.Bottles[2].LiquidCount, Is.EqualTo(3));
            Assert.That(state.Bottles[2].IsLiquidHidden(1), Is.True);
            Assert.That(state.IsCompleted, Is.False);
        }

        [Test]
        public void IsCompleted_WithEmptyOrCompletedBottles_ReturnsTrue()
        {
            LevelState state = new LevelState(Deserialize(CompletedLevelJson));

            Assert.That(state.IsCompleted, Is.True);
        }

        [Test]
        public void Constructor_WhenSourceBottleArrayChanges_DoesNotChangeState()
        {
            LevelData levelData = Deserialize(LevelJson);
            LevelState state = new LevelState(levelData);
            BottleData[] sourceBottles = (BottleData[])levelData.Bottles;

            sourceBottles[0] = sourceBottles[1];

            Assert.That(state.Bottles[0].LiquidCount, Is.EqualTo(2));
            Assert.That(state.Bottles[1].IsEmpty, Is.True);
        }

        [Test]
        public void Bottles_WhenCastToList_RejectsChanges()
        {
            LevelState state = new LevelState(Deserialize(LevelJson));
            IList<BottleState> exposedBottles = state.Bottles as IList<BottleState>;

            Assert.That(exposedBottles, Is.Not.Null);
            Assert.Throws<NotSupportedException>(
                () => exposedBottles.Add(state.Bottles[0]));
        }

        [Test]
        public void Constructor_WithNullData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelState(null));
        }

        private static LevelData Deserialize(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }
    }
}
