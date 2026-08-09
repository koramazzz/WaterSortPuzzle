using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout
{
    public readonly struct BottleGridLayoutResult
    {
        public BottleGridLayoutResult(int columnCount, Vector2 cellSize)
        {
            ColumnCount = columnCount;
            CellSize = cellSize;
        }

        public int ColumnCount { get; }
        public Vector2 CellSize { get; }
    }
}
