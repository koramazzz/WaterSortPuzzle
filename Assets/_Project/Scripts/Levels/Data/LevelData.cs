using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Levels
{
    [Serializable]
    public sealed class LevelData
    {
        [SerializeField] private int levelNumber;
        [SerializeField] private int bottleCapacity;
        [SerializeField] private BottleData[] bottles = Array.Empty<BottleData>();

        public int LevelNumber => levelNumber;
        public int BottleCapacity => bottleCapacity;
        public IReadOnlyList<BottleData> Bottles => bottles;
    }
}
