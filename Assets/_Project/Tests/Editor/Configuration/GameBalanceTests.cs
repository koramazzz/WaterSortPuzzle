using NUnit.Framework;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Tests.EditMode.Configuration
{
    public sealed class GameBalanceTests
    {
        [Test]
        public void ResourceValues_AreWithinValidRanges()
        {
            Assert.That(GameBalance.MaximumLives, Is.Positive);
            Assert.That(GameBalance.InitialGold, Is.Not.Negative);
            Assert.That(GameBalance.LifeRefillDurationSeconds, Is.Positive);
            Assert.That(GameBalance.BaseGoldReward, Is.Positive);
        }
    }
}
