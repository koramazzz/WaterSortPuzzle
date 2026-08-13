using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles
{
    public sealed class BottleStateTests
    {
        private const string PartiallyFilledBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": [0]
        }";

        private const string EmptyBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [],
          ""hiddenLiquidIndices"": []
        }";

        private const string VisiblePartiallyFilledBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": []
        }";

        private const string HiddenLowerBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": [0]
        }";

        private const string FullBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue"", ""green"", ""yellow""],
          ""hiddenLiquidIndices"": []
        }";

        private const string FullSingleColorBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""red"", ""red"", ""red""],
          ""hiddenLiquidIndices"": []
        }";

        private const string PartiallyFilledSingleColorBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""red""],
          ""hiddenLiquidIndices"": []
        }";

        [Test]
        public void Constructor_WithInitialData_CreatesExpectedState()
        {
            BottleData initialData = Deserialize(PartiallyFilledBottleJson);

            BottleState state = new BottleState(4, initialData);

            Assert.That(state.Capacity, Is.EqualTo(4));
            Assert.That(state.LiquidCount, Is.EqualTo(2));
            Assert.That(state.EmptySpace, Is.EqualTo(2));
            Assert.That(state.LiquidIdsBottomToTop, Is.EqualTo(
                new[] { "red", "blue" }));
            Assert.That(state.IsEmpty, Is.False);
            Assert.That(state.IsFull, Is.False);
            Assert.That(state.TopLiquidId, Is.EqualTo("blue"));
            Assert.That(state.IsLiquidHidden(0), Is.True);
            Assert.That(state.IsLiquidHidden(1), Is.False);
        }

        [Test]
        public void Constructor_WithEmptyBottle_CreatesEmptyState()
        {
            BottleState state = new BottleState(4, Deserialize(EmptyBottleJson));

            Assert.That(state.LiquidCount, Is.Zero);
            Assert.That(state.EmptySpace, Is.EqualTo(4));
            Assert.That(state.IsEmpty, Is.True);
            Assert.That(state.IsFull, Is.False);
            Assert.That(state.IsCompleted, Is.False);
            Assert.That(state.TopLiquidId, Is.Null);
        }

        [Test]
        public void Constructor_WithFullBottle_CreatesFullState()
        {
            BottleState state = new BottleState(4, Deserialize(FullBottleJson));

            Assert.That(state.LiquidCount, Is.EqualTo(4));
            Assert.That(state.EmptySpace, Is.Zero);
            Assert.That(state.IsFull, Is.True);
            Assert.That(state.IsCompleted, Is.False);
        }

        [Test]
        public void IsCompleted_WithFullSingleColorBottle_ReturnsTrue()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(FullSingleColorBottleJson));

            Assert.That(state.IsCompleted, Is.True);
        }

        [Test]
        public void IsCompleted_WithPartiallyFilledSingleColorBottle_ReturnsFalse()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(PartiallyFilledSingleColorBottleJson));

            Assert.That(state.IsCompleted, Is.False);
        }

        [Test]
        public void AddLiquid_WithEmptyBottle_AddsVisibleTopLiquid()
        {
            BottleState state = new BottleState(4, Deserialize(EmptyBottleJson));
            IReadOnlyList<string> exposedLiquidIds = state.LiquidIdsBottomToTop;

            state.AddLiquid("green");

            Assert.That(exposedLiquidIds, Is.EqualTo(new[] { "green" }));
            Assert.That(state.LiquidCount, Is.EqualTo(1));
            Assert.That(state.EmptySpace, Is.EqualTo(3));
            Assert.That(state.TopLiquidId, Is.EqualTo("green"));
        }

        [Test]
        public void AddLiquid_WithPartiallyFilledBottle_AppendsLiquidToTop()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(VisiblePartiallyFilledBottleJson));

            state.AddLiquid("green");

            Assert.That(
                state.LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red", "blue", "green" }));
            Assert.That(state.TopLiquidId, Is.EqualTo("green"));
        }

        [Test]
        public void AddLiquid_WithFullBottle_ThrowsInvalidOperationException()
        {
            BottleState state = new BottleState(4, Deserialize(FullBottleJson));

            Assert.Throws<InvalidOperationException>(() => state.AddLiquid("red"));
            Assert.That(state.LiquidCount, Is.EqualTo(4));
        }

        [Test]
        public void AddLiquid_WithNullId_ThrowsArgumentNullException()
        {
            BottleState state = new BottleState(4, Deserialize(EmptyBottleJson));

            Assert.Throws<ArgumentNullException>(() => state.AddLiquid(null));
        }

        [TestCase("")]
        [TestCase(" ")]
        public void AddLiquid_WithEmptyId_ThrowsArgumentException(string liquidId)
        {
            BottleState state = new BottleState(4, Deserialize(EmptyBottleJson));

            Assert.Throws<ArgumentException>(() => state.AddLiquid(liquidId));
        }

        [Test]
        public void RemoveTopLiquid_WithNonEmptyBottle_RemovesAndReturnsTopLiquid()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(VisiblePartiallyFilledBottleJson));

            string removedLiquidId = state.RemoveTopLiquid();

            Assert.That(removedLiquidId, Is.EqualTo("blue"));
            Assert.That(state.LiquidIdsBottomToTop, Is.EqualTo(new[] { "red" }));
            Assert.That(state.LiquidCount, Is.EqualTo(1));
            Assert.That(state.EmptySpace, Is.EqualTo(3));
            Assert.That(state.TopLiquidId, Is.EqualTo("red"));
        }

        [Test]
        public void RemoveTopLiquid_WithHiddenNextLiquid_RevealsNextLiquid()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(HiddenLowerBottleJson));

            state.RemoveTopLiquid();

            Assert.That(state.TopLiquidId, Is.EqualTo("red"));
            Assert.That(state.IsLiquidHidden(0), Is.False);
        }

        [Test]
        public void RemoveTopLiquid_WithEmptyBottle_ThrowsInvalidOperationException()
        {
            BottleState state = new BottleState(4, Deserialize(EmptyBottleJson));

            Assert.Throws<InvalidOperationException>(() => state.RemoveTopLiquid());
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void IsLiquidHidden_WithOutOfRangeIndex_ThrowsArgumentOutOfRangeException(
            int liquidIndex)
        {
            BottleState state = new BottleState(
                4,
                Deserialize(PartiallyFilledBottleJson));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.IsLiquidHidden(liquidIndex));
        }

        [Test]
        public void Constructor_WhenSourceChanges_DoesNotChangeState()
        {
            BottleData initialData = Deserialize(PartiallyFilledBottleJson);
            BottleState state = new BottleState(4, initialData);
            string[] sourceLiquidIds = (string[])initialData.LiquidIdsBottomToTop;
            int[] sourceHiddenIndices = (int[])initialData.HiddenLiquidIndices;

            sourceLiquidIds[1] = "green";
            sourceHiddenIndices[0] = 1;

            Assert.That(state.TopLiquidId, Is.EqualTo("blue"));
            Assert.That(state.IsLiquidHidden(0), Is.True);
            Assert.That(state.IsLiquidHidden(1), Is.False);
        }

        [Test]
        public void LiquidIdsBottomToTop_WhenCastToList_RejectsChanges()
        {
            BottleState state = new BottleState(
                4,
                Deserialize(PartiallyFilledBottleJson));
            IList<string> exposedLiquidIds =
                state.LiquidIdsBottomToTop as IList<string>;

            Assert.That(exposedLiquidIds, Is.Not.Null);
            Assert.Throws<NotSupportedException>(
                () => exposedLiquidIds.Add("green"));
        }

        [Test]
        public void Constructor_WithNullData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BottleState(4, null));
        }

        private static BottleData Deserialize(string json)
        {
            return JsonUtility.FromJson<BottleData>(json);
        }
    }
}
