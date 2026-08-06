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

        public void Initialize(IReadOnlyList<BottleState> bottleStates)
        {
            if (bottleStates == null)
            {
                throw new ArgumentNullException(nameof(bottleStates));
            }

            foreach (BottleState bottleState in bottleStates)
            {
                BottleView bottleView = Instantiate(
                    bottlePrefab,
                    transform,
                    false);

                bottleView.Initialize(bottleState, colorPalette);
            }

            bottleGridLayout.Arrange(bottleStates.Count);
        }
    }
}
