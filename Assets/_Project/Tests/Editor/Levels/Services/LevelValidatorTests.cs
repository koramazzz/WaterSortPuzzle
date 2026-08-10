using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Validation;

namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    public sealed class LevelValidatorTests
    {
        [Test]
        public void Validate_WithValidLevel_ReturnsNoErrors()
        {
            LevelValidator validator = new LevelValidator();
            LevelData level = Deserialize(LevelJsonSamples.ValidLevel);

            IReadOnlyList<string> errors = validator.Validate(level);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_WithNullLevel_ThrowsArgumentNullException()
        {
            LevelValidator validator = new LevelValidator();

            Assert.Throws<ArgumentNullException>(() => validator.Validate(null));
        }

        [Test]
        public void Validate_WithUnknownDifficulty_ReturnsDifficultyError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
              ""difficulty"": 0,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("valid difficulty"));
        }

        [Test]
        public void Validate_WithInvalidLevelProperties_ReturnsAllErrors()
        {
            const string json = @"
            {
              ""levelNumber"": 0,
          ""difficulty"": 1,
              ""bottleCapacity"": 0,
              ""bottles"": []
            }";

            LevelValidator validator = new LevelValidator();
            LevelData level = Deserialize(json);

            IReadOnlyList<string> errors = validator.Validate(level);

            Assert.That(errors.Count, Is.EqualTo(3));
            Assert.That(errors[0], Does.Contain("positive level number"));
            Assert.That(errors[1], Does.Contain("positive bottle capacity"));
            Assert.That(errors[2], Does.Contain("at least one bottle"));
        }

        [Test]
        public void Validate_WithOverfilledBottle_ReturnsCapacityError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue"", ""green""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("contains 3 liquids"));
        }

        [Test]
        public void Validate_WithEmptyLiquidId_ReturnsLiquidIdError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", "" ""],
                  ""hiddenLiquidIndices"": []
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("empty liquid ID at index 1"));
        }

        [Test]
        public void Validate_WithDuplicateHiddenIndex_ReturnsDuplicateError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 3,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue"", ""green""],
                  ""hiddenLiquidIndices"": [0, 0]
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("duplicate hidden liquid index 0"));
        }

        [Test]
        public void Validate_WithOutOfRangeHiddenIndex_ReturnsRangeError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": [2]
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("valid range is 0 to 1"));
        }

        [Test]
        public void Validate_WithHiddenIndexOnEmptyBottle_ReturnsEmptyBottleError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [],
                  ""hiddenLiquidIndices"": [0]
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("bottle has no liquids"));
        }

        [Test]
        public void Validate_WithHiddenTopLiquid_ReturnsTopLiquidError()
        {
            const string json = @"
            {
              ""levelNumber"": 1,
          ""difficulty"": 1,
              ""bottleCapacity"": 2,
              ""bottles"": [
                {
                  ""liquidIdsBottomToTop"": [""red"", ""blue""],
                  ""hiddenLiquidIndices"": [1]
                }
              ]
            }";

            IReadOnlyList<string> errors = Validate(json);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain("top liquid cannot be hidden"));
        }

        private static IReadOnlyList<string> Validate(string json)
        {
            LevelValidator validator = new LevelValidator();
            return validator.Validate(Deserialize(json));
        }

        private static LevelData Deserialize(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }
    }
}
