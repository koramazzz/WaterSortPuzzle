using System;

namespace WaterSortPuzzle.Gameplay.Bottles
{
    public sealed class BottleInteractionService
    {
        private readonly BottlePourService pourService = new BottlePourService();

        public BottleState SelectedSource { get; private set; }

        public int Select(BottleState bottle)
        {
            if (bottle == null)
            {
                throw new ArgumentNullException(nameof(bottle));
            }

            if (SelectedSource == null)
            {
                if (!bottle.IsEmpty && !bottle.IsTopLiquidHidden)
                {
                    SelectedSource = bottle;
                }

                return 0;
            }

            BottleState source = SelectedSource;
            SelectedSource = null;

            return pourService.Pour(source, bottle);
        }
    }
}
