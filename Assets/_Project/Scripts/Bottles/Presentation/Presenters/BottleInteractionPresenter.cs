using System;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleInteractionPresenter
    {
        private readonly BottleInteractionService interactionService = new BottleInteractionService();

        private BottleCollectionView bottleCollectionView;
        private BottleView selectedSourceView;
        private bool isPourAnimationPlaying;

        public event Action PourCompleted;
        public event Action<SoundEffectId> SoundEffectRequested;

        public void Initialize(BottleCollectionView bottleCollectionView)
        {
            if (bottleCollectionView == null)
            {
                throw new ArgumentNullException(nameof(bottleCollectionView));
            }

            this.bottleCollectionView = bottleCollectionView;
            bottleCollectionView.BottleClicked += HandleBottleClicked;
        }

        private void HandleBottleClicked(BottleView clickedBottleView)
        {
            if (isPourAnimationPlaying)
            {
                return;
            }

            if (interactionService.SelectedSource == null)
            {
                interactionService.Select(clickedBottleView.State);
                selectedSourceView = interactionService.SelectedSource == null
                    ? null
                    : clickedBottleView;

                if (selectedSourceView != null)
                {
                    selectedSourceView.AnimateSelection();
                    RequestSoundEffect(SoundEffectId.BottleSelected);
                }
                else
                {
                    RequestSoundEffect(SoundEffectId.InvalidMove);
                }

                return;
            }

            BottleView sourceView = selectedSourceView;
            int sourceLiquidCountBefore = sourceView.State.LiquidCount;
            int destinationLiquidCountBefore = clickedBottleView.State.LiquidCount;
            bool selectionCancelled = ReferenceEquals(sourceView, clickedBottleView);
            int pouredLiquidCount = interactionService.Select(clickedBottleView.State);
            selectedSourceView = null;

            if (pouredLiquidCount == 0)
            {
                sourceView.AnimateDeselection();
                RequestSoundEffect(selectionCancelled
                    ? SoundEffectId.BottleReleased
                    : SoundEffectId.InvalidMove);
                return;
            }

            isPourAnimationPlaying = true;
            try
            {
                bottleCollectionView.AnimatePour(
                    sourceView,
                    clickedBottleView,
                    sourceLiquidCountBefore,
                    destinationLiquidCountBefore,
                    pouredLiquidCount,
                    () => RequestSoundEffect(SoundEffectId.ValidPour),
                    CompletePour,
                    CancelPour);
            }
            catch
            {
                CancelPour();
                throw;
            }
        }

        private void CompletePour()
        {
            isPourAnimationPlaying = false;
            PourCompleted?.Invoke();
        }

        private void CancelPour()
        {
            isPourAnimationPlaying = false;
        }

        private void RequestSoundEffect(SoundEffectId soundEffectId)
        {
            SoundEffectRequested?.Invoke(soundEffectId);
        }
    }
}
