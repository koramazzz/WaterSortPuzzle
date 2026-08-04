using System;

namespace WaterSortPuzzle.Gameplay.Bottles
{
    public sealed class BottlePourService
    {
        public int Pour(BottleState source, BottleState destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            int pourAmount = CalculatePourAmount(source, destination);

            for (int liquidIndex = 0; liquidIndex < pourAmount; liquidIndex++)
            {
                destination.AddLiquid(source.RemoveTopLiquid());
            }

            return pourAmount;
        }

        private static int CalculatePourAmount(
            BottleState source,
            BottleState destination)
        {
            if (ReferenceEquals(source, destination) ||
                source.IsEmpty ||
                source.IsTopLiquidHidden ||
                destination.IsFull ||
                (!destination.IsEmpty && destination.IsTopLiquidHidden))
            {
                return 0;
            }

            string pouringLiquidId = source.TopLiquidId;

            if (!destination.IsEmpty &&
                !string.Equals(
                    destination.TopLiquidId,
                    pouringLiquidId,
                    StringComparison.Ordinal))
            {
                return 0;
            }

            int pourAmount = 0;

            for (int sourceIndex = source.LiquidCount - 1;
                 sourceIndex >= 0 && pourAmount < destination.EmptySpace;
                 sourceIndex--)
            {
                if (source.IsLiquidHidden(sourceIndex) ||
                    !string.Equals(
                        source.LiquidIdsBottomToTop[sourceIndex],
                        pouringLiquidId,
                        StringComparison.Ordinal))
                {
                    break;
                }

                pourAmount++;
            }

            return pourAmount;
        }
    }
}
