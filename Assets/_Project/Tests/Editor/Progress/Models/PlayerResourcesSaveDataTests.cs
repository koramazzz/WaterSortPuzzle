using System;
using NUnit.Framework;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerResourcesSaveDataTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresSaveData()
        {
            PlayerResourcesSaveData saveData =
                new PlayerResourcesSaveData(100, 3, 2500);

            Assert.That(saveData.Gold, Is.EqualTo(100));
            Assert.That(saveData.Lives, Is.EqualTo(3));
            Assert.That(saveData.NextLifeTimestamp, Is.EqualTo(2500));
        }

        [Test]
        public void Constructor_WithNegativeGold_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResourcesSaveData(-1, 1, 1));
        }

        [TestCase(-1)]
        [TestCase(GameBalance.MaximumLives + 1)]
        public void Constructor_WithOutOfRangeLives_Throws(int lives)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResourcesSaveData(1, lives, 1));
        }

        [Test]
        public void Constructor_WithNegativeTimestamp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResourcesSaveData(1, 1, -1));
        }
    }
}
