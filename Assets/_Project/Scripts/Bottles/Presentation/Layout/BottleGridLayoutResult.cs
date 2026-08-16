using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout
{
    public readonly struct BottleGridLayoutResult
    {
        public BottleGridLayoutResult(
            int columnCount,
            Vector2 cellSize,
            IReadOnlyList<Vector2> anchoredPositions)
        {
            if (anchoredPositions == null)
            {
                throw new ArgumentNullException(nameof(anchoredPositions));
            }

            ColumnCount = columnCount;
            CellSize = cellSize;
            AnchoredPositions = anchoredPositions;
        }

        public int ColumnCount { get; }

        public Vector2 CellSize { get; }

        public IReadOnlyList<Vector2> AnchoredPositions { get; }
    }
}
