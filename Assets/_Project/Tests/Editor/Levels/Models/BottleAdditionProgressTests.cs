using System;
using NUnit.Framework;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Gameplay.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels
{
    public sealed class BottleAdditionProgressTests
    {
        private BottleAdditionProgress progress;

        [SetUp]
        public void SetUp()
        {
            progress = new BottleAdditionProgress();
        }

        [Test]
        public void NewProgress_HasConfiguredAllowance()
        {
            Assert.That(progress.AddedBottleCount, Is.Zero);
            Assert.That(
                progress.RemainingAdditionCount,
                Is.EqualTo(GameBalance.MaximumBottleAdditionsPerLevel));
            Assert.That(progress.CanAddBottle, Is.True);
        }

        [Test]
        public void RecordBottleAdded_ConsumesOneAddition()
        {
            progress.RecordBottleAdded();

            Assert.That(progress.AddedBottleCount, Is.EqualTo(1));
            Assert.That(
                progress.RemainingAdditionCount,
                Is.EqualTo(
                    GameBalance.MaximumBottleAdditionsPerLevel - 1));
        }

        [Test]
        public void RecordBottleAdded_AfterConfiguredLimit_DisablesAddition()
        {
            UseEntireAllowance();

            Assert.That(progress.CanAddBottle, Is.False);
            Assert.That(progress.RemainingAdditionCount, Is.Zero);
            Assert.Throws<InvalidOperationException>(
                progress.RecordBottleAdded);
        }

        private void UseEntireAllowance()
        {
            for (int additionIndex = 0;
                 additionIndex < GameBalance.MaximumBottleAdditionsPerLevel;
                 additionIndex++)
            {
                progress.RecordBottleAdded();
            }
        }
    }
}
