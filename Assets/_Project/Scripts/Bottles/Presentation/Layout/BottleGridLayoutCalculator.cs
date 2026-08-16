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

            Vector2[] anchoredPositions = CalculateAnchoredPositions(
                bottleCount,
                bestColumnCount,
                bestCellSize,
                spacing,
                padding);

            return new BottleGridLayoutResult(
                bestColumnCount,
                bestCellSize,
                anchoredPositions);
        }

        private static Vector2[] CalculateAnchoredPositions(
            int bottleCount,
            int columnCount,
            Vector2 cellSize,
            Vector2 spacing,
            RectOffset padding)
        {
            int rowCount = Mathf.CeilToInt((float)bottleCount / columnCount);
            float gridHeight = cellSize.y * rowCount + spacing.y * (rowCount - 1);

            Vector2 contentCenter = new Vector2((padding.left - padding.right) * 0.5f, (padding.bottom - padding.top) * 0.5f);

            Vector2[] anchoredPositions = new Vector2[bottleCount];

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                int firstBottleIndex = rowIndex * columnCount;
                int bottlesInRow = Mathf.Min(columnCount, bottleCount - firstBottleIndex);

                float rowWidth = cellSize.x * bottlesInRow + spacing.x * (bottlesInRow - 1);
                float firstBottleX = contentCenter.x - rowWidth * 0.5f + cellSize.x * 0.5f;

                float bottleY = contentCenter.y + gridHeight * 0.5f - cellSize.y * 0.5f - rowIndex * (cellSize.y + spacing.y);

                for (int columnIndex = 0; columnIndex < bottlesInRow; columnIndex++)
                {
                    int bottleIndex = firstBottleIndex + columnIndex;

                    float bottleX = firstBottleX + columnIndex * (cellSize.x + spacing.x);

                    anchoredPositions[bottleIndex] = new Vector2(bottleX, bottleY);
                }
            }

            return anchoredPositions;
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
