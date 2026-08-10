using System;
using NUnit.Framework;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Rewards;

namespace WaterSortPuzzle.Tests.EditMode.Levels
{
    public sealed class LevelRewardCalculatorTests
    {
        [TestCase(LevelDifficulty.Easy, 1)]
        [TestCase(LevelDifficulty.Medium, 2)]
        [TestCase(LevelDifficulty.Hard, 3)]
        public void CalculateGoldReward_UsesConfiguredBaseReward(
            LevelDifficulty difficulty,
            int expectedMultiplier)
        {
            LevelRewardCalculator calculator = new LevelRewardCalculator();

            int reward = calculator.CalculateGoldReward(difficulty);

            Assert.That(
                reward,
                Is.EqualTo(
                    GameBalance.BaseGoldReward * expectedMultiplier));
        }

        [TestCase(LevelDifficulty.Easy, 50)]
        [TestCase(LevelDifficulty.Medium, 100)]
        [TestCase(LevelDifficulty.Hard, 150)]
        public void CalculateGoldReward_WithDifficulty_ReturnsScaledReward(
            LevelDifficulty difficulty,
            int expectedReward)
        {
            LevelRewardCalculator calculator = new LevelRewardCalculator();

            int reward = calculator.CalculateGoldReward(50, difficulty);

            Assert.That(reward, Is.EqualTo(expectedReward));
        }

        [TestCase((LevelDifficulty)0)]
        [TestCase((LevelDifficulty)99)]
        public void CalculateGoldReward_WithInvalidDifficulty_Throws(
            LevelDifficulty difficulty)
        {
            LevelRewardCalculator calculator = new LevelRewardCalculator();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.CalculateGoldReward(
                    50,
                    difficulty));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CalculateGoldReward_WithNonPositiveBaseReward_Throws(
            int baseReward)
        {
            LevelRewardCalculator calculator = new LevelRewardCalculator();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.CalculateGoldReward(
                    baseReward,
                    LevelDifficulty.Easy));
        }
    }
}
