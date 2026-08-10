using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels
{
    public sealed class LevelOutcomeEvaluatorTests
    {
        [Test]
        public void Evaluate_WithCompletedLevel_ReturnsCompleted()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""red""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            LevelOutcome outcome = Evaluate(json);

            Assert.That(outcome, Is.EqualTo(LevelOutcome.Completed));
        }

        [Test]
        public void Evaluate_WithEmptyBottle_ReturnsInProgress()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 4,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            LevelOutcome outcome = Evaluate(json);

            Assert.That(outcome, Is.EqualTo(LevelOutcome.InProgress));
        }

        [Test]
        public void Evaluate_WithOneAvailableBottlePair_ReturnsInProgress()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [""green""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [""blue""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            LevelOutcome outcome = Evaluate(json);

            Assert.That(outcome, Is.EqualTo(LevelOutcome.InProgress));
        }

        [Test]
        public void Evaluate_WithoutAvailableBottlePair_ReturnsFailed()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [""green""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [""blue"", ""red""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            LevelOutcome outcome = Evaluate(json);

            Assert.That(outcome, Is.EqualTo(LevelOutcome.Failed));
        }

        [Test]
        public void Evaluate_WhenMatchingTopGroupDoesNotFit_ReturnsFailed()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 4,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""green"", ""blue"", ""blue""],
                  ""hiddenLiquidIndices"": []
                },
                {
                  ""liquidIdsBottomToTop"": [""yellow"", ""red"", ""blue""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            LevelOutcome outcome = Evaluate(json);

            Assert.That(outcome, Is.EqualTo(LevelOutcome.Failed));
        }

        [Test]
        public void Evaluate_WithNullLevel_ThrowsArgumentNullException()
        {
            LevelOutcomeEvaluator evaluator = new LevelOutcomeEvaluator();

            Assert.Throws<ArgumentNullException>(() => evaluator.Evaluate(null));
        }

        private static LevelOutcome Evaluate(string json)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            LevelState levelState = new LevelState(levelData);
            LevelOutcomeEvaluator evaluator = new LevelOutcomeEvaluator();

            return evaluator.Evaluate(levelState);
        }
    }
}
