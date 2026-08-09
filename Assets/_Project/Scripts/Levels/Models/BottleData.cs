using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Levels
{
    [Serializable]
    public sealed class BottleData
    {
        [SerializeField] private string[] liquidIdsBottomToTop = Array.Empty<string>();
        [SerializeField] private int[] hiddenLiquidIndices = Array.Empty<int>();

        public IReadOnlyList<string> LiquidIdsBottomToTop => liquidIdsBottomToTop;
        public IReadOnlyList<int> HiddenLiquidIndices => hiddenLiquidIndices;
    }
}
