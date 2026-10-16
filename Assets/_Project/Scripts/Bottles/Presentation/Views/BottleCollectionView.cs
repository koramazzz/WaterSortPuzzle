using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleCollectionView : MonoBehaviour
    {
        [SerializeField] private BottleView bottlePrefab;
        [SerializeField] private LiquidColorPalette colorPalette;
        [SerializeField] private BottleGridLayout bottleGridLayout;
        [FormerlySerializedAs("pourAnimator")]
        [SerializeField] private LiquidTransferAnimator liquidTransferAnimator;

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

        public void AnimatePour(
            BottleView source,
            BottleView destination,
            int sourceLiquidCountBefore,
            int destinationLiquidCountBefore,
            int pouredLiquidCount,
            Action pourStarted,
            Action completed,
            Action canceled = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            bool cancellationHandled = false;

            void RefreshViews()
            {
                source.Refresh();
                destination.Refresh();
            }

            void CancelPour()
            {
                if (cancellationHandled)
                {
                    return;
                }

                cancellationHandled = true;

                try
                {
                    RefreshViews();
                }
                finally
                {
                    canceled?.Invoke();
                }
            }

            try
            {
                Color liquidColor = colorPalette.GetColor(
                    destination.State.TopLiquidId);
                LiquidTransferAnimationRequest request =
                    new LiquidTransferAnimationRequest(
                        source.PourAnimator,
                        destination.PourAnimator,
                        source.PrepareForPour(),
                        source.GetLiquidBounds(),
                        destination.GetLiquidBounds(),
                        source.State.Capacity,
                        sourceLiquidCountBefore,
                        destinationLiquidCountBefore,
                        pouredLiquidCount,
                        liquidColor);
                liquidTransferAnimator.Play(
                    request,
                    pourStarted,
                    (sourceProgress, destinationProgress) =>
                    {
                        source.ShowPourOutProgress(
                            sourceLiquidCountBefore,
                            pouredLiquidCount,
                            sourceProgress);
                        destination.ShowPourInProgress(
                            destinationLiquidCountBefore,
                            pouredLiquidCount,
                            liquidColor,
                            destinationProgress);
                    },
                    destination.PlayLiquidImpact,
                    RefreshViews,
                    completed,
                    CancelPour);
            }
            catch
            {
                try
                {
                    if (!cancellationHandled)
                    {
                        source.AnimateDeselection();
                    }
                }
                finally
                {
                    CancelPour();
                }

                throw;
            }
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
