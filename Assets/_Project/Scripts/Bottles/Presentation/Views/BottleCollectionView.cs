using System;
using System.Collections.Generic;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleCollectionView : MonoBehaviour
    {
        [SerializeField] private BottleView bottlePrefab;
        [SerializeField] private LiquidColorPalette colorPalette;
        [SerializeField] private BottleGridLayout bottleGridLayout;

        public event Action<BottleView> BottleClicked;
        public event Action BottleCompletionAnimationFinished;

        public void Initialize(IReadOnlyList<BottleState> bottleStates)
        {
            if (bottleStates == null)
            {
                throw new ArgumentNullException(nameof(bottleStates));
            }

            foreach (BottleState bottleState in bottleStates)
            {
                CreateBottleView(bottleState);
            }

            bottleGridLayout.Arrange(bottleStates.Count);
        }

        public void AddBottle(BottleState bottleState)
        {
            if (bottleState == null)
            {
                throw new ArgumentNullException(nameof(bottleState));
            }

            BottleView bottleView = CreateBottleView(bottleState);
            bottleGridLayout.AnimateBottleAddition((RectTransform)bottleView.transform);
        }

        private void HandleBottleClicked(BottleView bottleView)
        {
            BottleClicked?.Invoke(bottleView);
        }

        private void HandleBottleCompletionAnimationFinished()
        {
            BottleCompletionAnimationFinished?.Invoke();
        }

        private BottleView CreateBottleView(BottleState bottleState)
        {
            BottleView bottleView = Instantiate(
                bottlePrefab,
                transform,
                false);

            bottleView.Initialize(bottleState, colorPalette);
            bottleView.Clicked += HandleBottleClicked;
            bottleView.CompletionAnimationFinished += HandleBottleCompletionAnimationFinished;
            return bottleView;
        }
    }
}
