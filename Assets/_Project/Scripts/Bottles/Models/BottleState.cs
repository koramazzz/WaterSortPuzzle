using System;
using System.Collections.Generic;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Gameplay.Bottles
{
    public sealed class BottleState
    {
        private readonly List<string> liquidIdsBottomToTop;
        private readonly IReadOnlyList<string> readOnlyLiquidIdsBottomToTop;
        private readonly HashSet<int> hiddenLiquidIndices;

        public BottleState(int capacity)
        {
            Capacity = capacity;
            liquidIdsBottomToTop = new List<string>();
            readOnlyLiquidIdsBottomToTop = liquidIdsBottomToTop.AsReadOnly();
            hiddenLiquidIndices = new HashSet<int>();
        }

        public BottleState(int capacity, BottleData initialData) : this(capacity)
        {
            if (initialData == null)
            {
                throw new ArgumentNullException(nameof(initialData));
            }

            liquidIdsBottomToTop.AddRange(initialData.LiquidIdsBottomToTop);
            hiddenLiquidIndices.UnionWith(initialData.HiddenLiquidIndices);
        }

        public int Capacity { get; }

        public IReadOnlyList<string> LiquidIdsBottomToTop => readOnlyLiquidIdsBottomToTop;

        public int LiquidCount => liquidIdsBottomToTop.Count;

        public int EmptySpace => Capacity - LiquidCount;

        public bool IsEmpty => LiquidCount == 0;

        public bool IsFull => EmptySpace == 0;

        public bool IsCompleted
        {
            get
            {
                if (IsEmpty || !IsFull)
                {
                    return false;
                }

                string firstLiquidId = liquidIdsBottomToTop[0];

                for (int liquidIndex = 1;
                     liquidIndex < LiquidCount;
                     liquidIndex++)
                {
                    if (!string.Equals(
                            liquidIdsBottomToTop[liquidIndex],
                            firstLiquidId,
                            StringComparison.Ordinal))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public string TopLiquidId => IsEmpty ? null : liquidIdsBottomToTop[TopLiquidIndex];

        private int TopLiquidIndex => LiquidCount - 1;

        public bool IsLiquidHidden(int liquidIndex)
        {
            if (liquidIndex < 0 || liquidIndex >= LiquidCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(liquidIndex),
                    liquidIndex,
                    $"Liquid index must be between 0 and {LiquidCount - 1}.");
            }

            return hiddenLiquidIndices.Contains(liquidIndex);
        }

        public void AddLiquid(string liquidId)
        {
            if (liquidId == null)
            {
                throw new ArgumentNullException(nameof(liquidId));
            }

            if (string.IsNullOrWhiteSpace(liquidId))
            {
                throw new ArgumentException(
                    "Liquid ID cannot be empty or whitespace.",
                    nameof(liquidId));
            }

            if (IsFull)
            {
                throw new InvalidOperationException(
                    "Cannot add a liquid to a full bottle.");
            }

            liquidIdsBottomToTop.Add(liquidId);
        }

        public string RemoveTopLiquid()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    "Cannot remove a liquid from an empty bottle.");
            }

            int removedLiquidIndex = TopLiquidIndex;
            string removedLiquidId = liquidIdsBottomToTop[removedLiquidIndex];

            liquidIdsBottomToTop.RemoveAt(removedLiquidIndex);
            hiddenLiquidIndices.Remove(removedLiquidIndex);
            RevealTopLiquid();

            return removedLiquidId;
        }

        private void RevealTopLiquid()
        {
            if (!IsEmpty)
            {
                hiddenLiquidIndices.Remove(TopLiquidIndex);
            }
        }
    }
}
