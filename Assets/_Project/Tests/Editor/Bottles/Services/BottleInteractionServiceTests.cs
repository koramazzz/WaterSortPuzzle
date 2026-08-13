using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles
{
    public sealed class BottleInteractionServiceTests
    {
        [Test]
        public void Select_WithAvailableSource_SelectsSource()
        {
            BottleState source = CreateBottle(new[] { "blue" });
            BottleInteractionService service = new BottleInteractionService();

            int pouredLiquidCount = service.Select(source);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(service.SelectedSource, Is.SameAs(source));
        }

        [Test]
        public void Select_WithEmptyBottle_DoesNotSelectSource()
        {
            BottleState emptyBottle = CreateBottle(Array.Empty<string>());
            BottleInteractionService service = new BottleInteractionService();

            int pouredLiquidCount = service.Select(emptyBottle);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(service.SelectedSource, Is.Null);
        }

        [Test]
        public void Select_WithCompletedBottle_DoesNotSelectSource()
        {
            BottleState completedBottle = CreateBottle(
                new[] { "blue", "blue", "blue", "blue" });
            BottleInteractionService service = new BottleInteractionService();

            int pouredLiquidCount = service.Select(completedBottle);

            Assert.That(completedBottle.IsCompleted, Is.True);
            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(service.SelectedSource, Is.Null);
        }

        [Test]
        public void Select_WithSelectedSource_PoursAndClearsSelection()
        {
            BottleState source = CreateBottle(new[] { "blue", "blue" });
            BottleState destination = CreateBottle(Array.Empty<string>());
            BottleInteractionService service = new BottleInteractionService();
            service.Select(source);

            int pouredLiquidCount = service.Select(destination);

            Assert.That(pouredLiquidCount, Is.EqualTo(2));
            Assert.That(source.IsEmpty, Is.True);
            Assert.That(
                destination.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "blue", "blue" }));
            Assert.That(service.SelectedSource, Is.Null);
        }

        [Test]
        public void Select_WithSameBottle_ClearsSelectionWithoutPouring()
        {
            BottleState bottle = CreateBottle(new[] { "blue" });
            BottleInteractionService service = new BottleInteractionService();
            service.Select(bottle);

            int pouredLiquidCount = service.Select(bottle);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(bottle.LiquidIdsBottomToTop, Is.EqualTo(new[] { "blue" }));
            Assert.That(service.SelectedSource, Is.Null);
        }

        [Test]
        public void Select_WithRejectedDestination_ClearsSelectionWithoutPouring()
        {
            BottleState source = CreateBottle(new[] { "blue" });
            BottleState destination = CreateBottle(new[] { "red" });
            BottleInteractionService service = new BottleInteractionService();
            service.Select(source);

            int pouredLiquidCount = service.Select(destination);

            Assert.That(pouredLiquidCount, Is.Zero);
            Assert.That(source.LiquidIdsBottomToTop, Is.EqualTo(new[] { "blue" }));
            Assert.That(destination.LiquidIdsBottomToTop, Is.EqualTo(new[] { "red" }));
            Assert.That(service.SelectedSource, Is.Null);
        }

        [Test]
        public void Select_WithNullBottle_ThrowsArgumentNullException()
        {
            BottleInteractionService service = new BottleInteractionService();

            Assert.Throws<ArgumentNullException>(() => service.Select(null));
        }

        private static BottleState CreateBottle(
            string[] liquidIdsBottomToTop,
            int[] hiddenLiquidIndices = null)
        {
            BottleJsonData jsonData = new BottleJsonData
            {
                liquidIdsBottomToTop = liquidIdsBottomToTop,
                hiddenLiquidIndices = hiddenLiquidIndices ?? Array.Empty<int>()
            };
            string json = JsonUtility.ToJson(jsonData);
            BottleData bottleData = JsonUtility.FromJson<BottleData>(json);

            return new BottleState(4, bottleData);
        }

        [Serializable]
        private sealed class BottleJsonData
        {
            public string[] liquidIdsBottomToTop;
            public int[] hiddenLiquidIndices;
        }
    }
}
