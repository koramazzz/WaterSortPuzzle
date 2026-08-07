using System;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleInteractionPresenter
    {
        private readonly BottleInteractionService interactionService = new BottleInteractionService();

        private BottleView selectedSourceView;

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
                return;
            }

            BottleView sourceView = selectedSourceView;
            int pouredLiquidCount = interactionService.Select(clickedBottleView.State);
            selectedSourceView = null;

            if (pouredLiquidCount == 0)
            {
                return;
            }

            sourceView.Refresh();
            clickedBottleView.Refresh();
        }
    }
}
