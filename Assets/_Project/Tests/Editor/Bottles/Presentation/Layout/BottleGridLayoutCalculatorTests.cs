using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation.Layout
{
    public sealed class BottleGridLayoutCalculatorTests
    {
        private readonly BottleGridLayoutCalculator calculator =
            new BottleGridLayoutCalculator();

        [Test]
        public void Calculate_WithFiveBottles_SelectsLargestFittingGrid()
        {
            BottleGridLayoutResult result = calculator.Calculate(
                new Vector2(600f, 600f),
                5,
                4,
                0.5f,
                new Vector2(20f, 20f),
                new RectOffset());

            Assert.That(result.ColumnCount, Is.EqualTo(3));
            Assert.That(result.CellSize.x, Is.EqualTo(145f).Within(0.001f));
            Assert.That(result.CellSize.y, Is.EqualTo(290f).Within(0.001f));
        }

        [Test]
        public void Calculate_WithPaddingAndSpacing_KeepsGridInsideContainer()
        {
            Vector2 containerSize = new Vector2(800f, 500f);
            Vector2 spacing = new Vector2(30f, 40f);
            RectOffset padding = new RectOffset(20, 20, 10, 10);

            BottleGridLayoutResult result = calculator.Calculate(
                containerSize,
                6,
                4,
                0.5f,
                spacing,
                padding);

            int rowCount = Mathf.CeilToInt(6f / result.ColumnCount);
            float gridWidth =
                result.CellSize.x * result.ColumnCount +
                spacing.x * (result.ColumnCount - 1) +
                padding.horizontal;
            float gridHeight =
                result.CellSize.y * rowCount +
                spacing.y * (rowCount - 1) +
                padding.vertical;

            Assert.That(gridWidth, Is.LessThanOrEqualTo(containerSize.x));
            Assert.That(gridHeight, Is.LessThanOrEqualTo(containerSize.y));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Calculate_WithNonPositiveBottleCount_Throws(
            int bottleCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(
                    Vector2.one,
                    bottleCount,
                    1,
                    1f,
                    Vector2.zero,
                    new RectOffset()));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Calculate_WithNonPositiveMaximumColumnCount_Throws(
            int maximumColumnCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(
                    Vector2.one,
                    1,
                    maximumColumnCount,
                    1f,
                    Vector2.zero,
                    new RectOffset()));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Calculate_WithNonPositiveBottleAspectRatio_Throws(
            float bottleAspectRatio)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(
                    Vector2.one,
                    1,
                    1,
                    bottleAspectRatio,
                    Vector2.zero,
                    new RectOffset()));
        }

        [TestCase(0f, 1f)]
        [TestCase(1f, 0f)]
        public void Calculate_WithNonPositiveContainerDimension_Throws(
            float width,
            float height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(
                    new Vector2(width, height),
                    1,
                    1,
                    1f,
                    Vector2.zero,
                    new RectOffset()));
        }

        [Test]
        public void Calculate_WithNullPadding_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => calculator.Calculate(
                    Vector2.one,
                    1,
                    1,
                    1f,
                    Vector2.zero,
                    null));
        }

        [Test]
        public void Calculate_WhenGridCannotFit_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => calculator.Calculate(
                    Vector2.one,
                    2,
                    2,
                    1f,
                    new Vector2(2f, 2f),
                    new RectOffset(1, 1, 1, 1)));
        }
    }
}
