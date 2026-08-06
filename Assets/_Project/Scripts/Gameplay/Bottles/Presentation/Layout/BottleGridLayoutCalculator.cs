using System;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout
{
    public sealed class BottleGridLayoutCalculator
    {
        public BottleGridLayoutResult Calculate(
            Vector2 containerSize,
            int bottleCount,
            int maximumColumnCount,
            float bottleAspectRatio,
            Vector2 spacing,
            RectOffset padding)
        {
            ValidateArguments(
                containerSize,
                bottleCount,
                maximumColumnCount,
                bottleAspectRatio,
                padding);

            int availableColumnCount = Mathf.Min(bottleCount, maximumColumnCount);

            int bestColumnCount = 0;
            Vector2 bestCellSize = Vector2.zero;

            for (int columnCount = 1; columnCount <= availableColumnCount; columnCount++)
            {
                int rowCount = Mathf.CeilToInt((float)bottleCount / columnCount);

                float availableWidth = containerSize.x - padding.horizontal - spacing.x * (columnCount - 1);
                float availableHeight = containerSize.y - padding.vertical - spacing.y * (rowCount - 1);

                if (availableWidth <= 0f || availableHeight <= 0f)
                {
                    continue;
                }

                float cellWidthLimit = availableWidth / columnCount;
                float cellHeightLimit = availableHeight / rowCount;

                float cellHeight = Mathf.Min(cellHeightLimit, cellWidthLimit / bottleAspectRatio);
                Vector2 cellSize = new Vector2(cellHeight * bottleAspectRatio, cellHeight);

                if (cellSize.y > bestCellSize.y)
                {
                    bestColumnCount = columnCount;
                    bestCellSize = cellSize;
                }
            }

            if (bestColumnCount == 0)
            {
                throw new InvalidOperationException(
                    "The bottle grid does not fit inside its container.");
            }

            return new BottleGridLayoutResult(
                bestColumnCount,
                bestCellSize);
        }

        private static void ValidateArguments(
            Vector2 containerSize,
            int bottleCount,
            int maximumColumnCount,
            float bottleAspectRatio,
            RectOffset padding)
        {
            if (containerSize.x <= 0f || containerSize.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(containerSize),
                    "Container dimensions must be greater than zero.");
            }

            if (bottleCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bottleCount),
                    "Bottle count must be greater than zero.");
            }

            if (maximumColumnCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumColumnCount),
                    "Maximum column count must be greater than zero.");
            }

            if (bottleAspectRatio <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bottleAspectRatio),
                    "Bottle aspect ratio must be greater than zero.");
            }

            if (padding == null)
            {
                throw new ArgumentNullException(nameof(padding));
            }
        }
    }
}
