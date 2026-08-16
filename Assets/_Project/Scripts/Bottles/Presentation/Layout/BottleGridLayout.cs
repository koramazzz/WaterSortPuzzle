using System;
using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout
{
    public sealed class BottleGridLayout : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int maximumColumnCount;
        [SerializeField, Min(0.01f)] private float bottleAspectRatio;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private RectOffset padding = new RectOffset();

        [Header("Animation")]
        [SerializeField] private BottleGridLayoutAnimator additionAnimator;

        private readonly BottleGridLayoutCalculator calculator = new BottleGridLayoutCalculator();

        public void Arrange(int bottleCount)
        {
            BottleGridLayoutResult result = Calculate(bottleCount);

                for (int bottleIndex = 0; bottleIndex < bottleCount; bottleIndex++)
            {
                PositionBottle(
                    (RectTransform)transform.GetChild(bottleIndex),
                    result.CellSize,
                    result.AnchoredPositions[bottleIndex]);
            }
        }

        public void AnimateBottleAddition(RectTransform addedBottle)
        {
            if (addedBottle == null)
            {
                throw new ArgumentNullException(nameof(addedBottle));
            }

            ConfigureBottle(addedBottle);

            BottleGridLayoutResult result = Calculate(transform.childCount);

            additionAnimator.Play(
                (RectTransform)transform,
                result.CellSize,
                result.AnchoredPositions,
                addedBottle);
        }

        private BottleGridLayoutResult Calculate(int bottleCount)
        {
            RectTransform container = (RectTransform)transform;
            return calculator.Calculate(
                container.rect.size,
                bottleCount,
                maximumColumnCount,
                bottleAspectRatio,
                spacing,
                padding);
        }

        private static void PositionBottle(
            RectTransform bottle,
            Vector2 bottleSize,
            Vector2 anchoredPosition)
        {
            ConfigureBottle(bottle);
            bottle.sizeDelta = bottleSize;
            bottle.anchoredPosition = anchoredPosition;
        }

        private static void ConfigureBottle(RectTransform bottle)
        {
            bottle.anchorMin = new Vector2(0.5f, 0.5f);
            bottle.anchorMax = new Vector2(0.5f, 0.5f);
            bottle.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
