using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles
{
    public sealed class BottlePourServiceTests
    {
        [Test]
        public void CanPour_ToEmptyBottle_ReturnsTrueWithoutChangingBottles()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "red", "blue" },
                Array.Empty<int>());
            BottleState destination = CreateEmptyBottle();
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(source, destination);

            Assert.That(canPour, Is.True);
            Assert.That(
                source.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red", "blue" }));
            Assert.That(destination.IsEmpty, Is.True);
        }

        [Test]
        public void CanPour_ToMatchingBottle_ReturnsTrue()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "red", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(source, destination);

            Assert.That(canPour, Is.True);
        }

        [Test]
        public void CanPour_WithEmptySource_ReturnsFalse()
        {
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(
                CreateEmptyBottle(),
                CreateEmptyBottle());

            Assert.That(canPour, Is.False);
        }

        [Test]
        public void CanPour_WithFullDestination_ReturnsFalse()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "blue", "blue", "blue", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(source, destination);

            Assert.That(canPour, Is.False);
        }

        [Test]
        public void CanPour_WhenTopGroupExceedsDestinationSpace_ReturnsFalse()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue", "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "red", "blue", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(source, destination);

            Assert.That(canPour, Is.False);
        }

        [Test]
        public void CanPour_WithDifferentTopLiquids_ReturnsFalse()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "red" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(source, destination);

            Assert.That(canPour, Is.False);
        }

        [Test]
        public void CanPour_ToSameBottle_ReturnsFalse()
        {
            BottleState bottle = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            bool canPour = service.CanPour(bottle, bottle);

            Assert.That(canPour, Is.False);
        }

        [Test]
        public void CanPour_WithNullSource_ThrowsArgumentNullException()
        {
            BottlePourService service = new BottlePourService();

            Assert.Throws<ArgumentNullException>(
                () => service.CanPour(null, CreateEmptyBottle()));
        }

        [Test]
        public void CanPour_WithNullDestination_ThrowsArgumentNullException()
        {
            BottlePourService service = new BottlePourService();

            Assert.Throws<ArgumentNullException>(
                () => service.CanPour(CreateEmptyBottle(), null));
        }

        [Test]
        public void Pour_ToEmptyBottle_MovesVisibleMatchingTopBlock()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "red", "blue", "blue" },
                Array.Empty<int>());
            BottleState destination = CreateEmptyBottle();
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.EqualTo(2));
            Assert.That(source.LiquidIdsBottomToTop, Is.EqualTo(new[] { "red" }));
            Assert.That(
                destination.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "blue", "blue" }));
        }

        [Test]
        public void Pour_ToMatchingBottle_MovesVisibleMatchingTopBlock()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "red", "blue", "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "green", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.EqualTo(2));
            Assert.That(source.LiquidIdsBottomToTop, Is.EqualTo(new[] { "red" }));
            Assert.That(
                destination.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "green", "blue", "blue", "blue" }));
        }

        [Test]
        public void Pour_WhenTopGroupExceedsDestinationSpace_DoesNotChangeBottles()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue", "blue", "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "red", "blue", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(
                source.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "blue", "blue", "blue" }));
            Assert.That(
                destination.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red", "blue", "blue" }));
        }

        [Test]
        public void Pour_WithEmptySource_DoesNotChangeBottles()
        {
            BottleState source = CreateEmptyBottle();
            BottleState destination = CreateEmptyBottle();
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(source.IsEmpty, Is.True);
            Assert.That(destination.IsEmpty, Is.True);
        }

        [Test]
        public void Pour_WithFullDestination_DoesNotChangeBottles()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "blue", "blue", "blue", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(source.LiquidCount, Is.EqualTo(1));
            Assert.That(destination.LiquidCount, Is.EqualTo(4));
        }

        [Test]
        public void Pour_WithDifferentTopLiquids_DoesNotChangeBottles()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue" },
                Array.Empty<int>());
            BottleState destination = CreateBottle(
                4,
                new[] { "red" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(source.TopLiquidId, Is.EqualTo("blue"));
            Assert.That(destination.TopLiquidId, Is.EqualTo("red"));
        }

        [Test]
        public void Pour_ToSameBottle_DoesNotChangeBottle()
        {
            BottleState bottle = CreateBottle(
                4,
                new[] { "blue", "blue" },
                Array.Empty<int>());
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(bottle, bottle);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(bottle.LiquidCount, Is.EqualTo(2));
        }

        [Test]
        public void Pour_WhenHiddenLiquidIsExposed_StopsAndRevealsLiquid()
        {
            BottleState source = CreateBottle(
                4,
                new[] { "blue", "blue" },
                new[] { 0 });
            BottleState destination = CreateEmptyBottle();
            BottlePourService service = new BottlePourService();

            int pouredLiquidCount = service.Pour(source, destination);

            Assert.That(pouredLiquidCount, Is.EqualTo(1));
            Assert.That(source.LiquidCount, Is.EqualTo(1));
            Assert.That(source.TopLiquidId, Is.EqualTo("blue"));
            Assert.That(source.IsLiquidHidden(0), Is.False);
            Assert.That(destination.LiquidCount, Is.EqualTo(1));
        }

        [Test]
        public void Pour_WithNullSource_ThrowsArgumentNullException()
        {
            BottlePourService service = new BottlePourService();

            Assert.Throws<ArgumentNullException>(
                () => service.Pour(null, CreateEmptyBottle()));
        }

        [Test]
        public void Pour_WithNullDestination_ThrowsArgumentNullException()
        {
            BottlePourService service = new BottlePourService();

            Assert.Throws<ArgumentNullException>(
                () => service.Pour(CreateEmptyBottle(), null));
        }

        private static BottleState CreateEmptyBottle()
        {
            return CreateBottle(4, Array.Empty<string>(), Array.Empty<int>());
        }

        private static BottleState CreateBottle(
            int capacity,
            string[] liquidIdsBottomToTop,
            int[] hiddenLiquidIndices)
        {
            BottleJsonData jsonData = new BottleJsonData
            {
                liquidIdsBottomToTop = liquidIdsBottomToTop,
                hiddenLiquidIndices = hiddenLiquidIndices
            };
            string json = JsonUtility.ToJson(jsonData);
            BottleData bottleData = JsonUtility.FromJson<BottleData>(json);

            return new BottleState(capacity, bottleData);
        }

        [Serializable]
        private sealed class BottleJsonData
        {
            public string[] liquidIdsBottomToTop;
            public int[] hiddenLiquidIndices;
        }
    }
}
