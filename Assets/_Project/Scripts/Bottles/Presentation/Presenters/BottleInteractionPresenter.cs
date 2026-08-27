using System;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleInteractionPresenter
    {
        private readonly BottleInteractionService interactionService = new BottleInteractionService();

        private BottleView selectedSourceView;

        public event Action PourCompleted;
        public event Action<SoundEffectId> SoundEffectRequested;

        public void Initialize(BottleCollectionView bottleCollectionView)
        {
            if (bottleCollectionView == null)
            {
                throw new ArgumentNullException(nameof(bottleCollectionView));
            }

            bottleCollectionView.BottleClicked += HandleBottleClicked;
        }

        private void HandleBottleClicked(BottleView clickedBottleView)
        {
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
            bool selectionCancelled = ReferenceEquals(sourceView, clickedBottleView);
            int pouredLiquidCount = interactionService.Select(clickedBottleView.State);
            selectedSourceView = null;
            sourceView.AnimateDeselection();

            if (pouredLiquidCount == 0)
            {
                RequestSoundEffect(selectionCancelled
                    ? SoundEffectId.BottleReleased
                    : SoundEffectId.InvalidMove);
                return;
            }

            sourceView.Refresh();
            PourCompleted?.Invoke();
            clickedBottleView.Refresh();
        }

        private void RequestSoundEffect(SoundEffectId soundEffectId)
        {
            SoundEffectRequested?.Invoke(soundEffectId);
        }
    }
}
