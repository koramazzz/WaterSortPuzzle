using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout
{
    public sealed class BottleGridLayout : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumColumnCount;
        [SerializeField, Min(0.01f)] private float bottleAspectRatio;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private RectOffset padding = new RectOffset();

        private readonly BottleGridLayoutCalculator calculator = new BottleGridLayoutCalculator();

        public void Arrange(int bottleCount)
        {
            RectTransform container = (RectTransform)transform;

            BottleGridLayoutResult result = calculator.Calculate(
                container.rect.size,
                bottleCount,
                maximumColumnCount,
                bottleAspectRatio,
                spacing,
                padding);

            PositionBottles(container.rect, bottleCount, result);
        }

        private void PositionBottles(
            Rect containerRect,
            int bottleCount,
            BottleGridLayoutResult result)
        {
            int rowCount = Mathf.CeilToInt((float)bottleCount / result.ColumnCount);

            float gridHeight = result.CellSize.y * rowCount + spacing.y * (rowCount - 1);

            Vector2 contentCenter = new Vector2(
                containerRect.xMin + padding.left + (containerRect.width - padding.horizontal) * 0.5f,
                containerRect.yMin + padding.bottom + (containerRect.height - padding.vertical) * 0.5f);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                int firstBottleIndex = rowIndex * result.ColumnCount;

                int bottlesInRow = Mathf.Min(result.ColumnCount, bottleCount - firstBottleIndex);

                float rowWidth = result.CellSize.x * bottlesInRow + spacing.x * (bottlesInRow - 1);

                float firstBottleX = contentCenter.x - rowWidth * 0.5f + result.CellSize.x * 0.5f;
                float bottleY = contentCenter.y + gridHeight * 0.5f - result.CellSize.y * 0.5f - rowIndex * (result.CellSize.y + spacing.y);

                for (int columnIndex = 0; columnIndex < bottlesInRow; columnIndex++)
                {
                    int bottleIndex = firstBottleIndex + columnIndex;

                    float bottleX = firstBottleX + columnIndex * (result.CellSize.x + spacing.x);

                    PositionBottle(
                        (RectTransform)transform.GetChild(bottleIndex),
                        containerRect.center,
                        result.CellSize,
                        new Vector2(bottleX, bottleY));
                }
            }
        }

        private static void PositionBottle(
            RectTransform bottle,
            Vector2 containerCenter,
            Vector2 bottleSize,
            Vector2 localPosition)
        {
            bottle.anchorMin = new Vector2(0.5f, 0.5f);
            bottle.anchorMax = new Vector2(0.5f, 0.5f);
            bottle.pivot = new Vector2(0.5f, 0.5f);
            bottle.sizeDelta = bottleSize;
            bottle.anchoredPosition = localPosition - containerCenter;
        }
    }
}
