using System;
using NUnit.Framework;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerResourcesTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresResources()
        {
            PlayerResources resources = new PlayerResources(100, 3, 120);

            Assert.That(resources.Gold, Is.EqualTo(100));
            Assert.That(resources.Lives, Is.EqualTo(3));
            Assert.That(resources.SecondsUntilNextLife, Is.EqualTo(120));
        }

        [Test]
        public void Constructor_WithNegativeValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResources(-1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResources(1, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerResources(1, 1, -1));
        }
    }
}
