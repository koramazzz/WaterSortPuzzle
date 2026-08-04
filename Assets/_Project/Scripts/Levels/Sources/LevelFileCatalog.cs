using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Levels.Sources
{
    [CreateAssetMenu(
        fileName = "LevelFileCatalog",
        menuName = "Water Sort Puzzle/Levels/Level File Catalog")]
    public sealed class LevelFileCatalog : ScriptableObject
    {
        [SerializeField] private TextAsset[] levelFiles = Array.Empty<TextAsset>();

        public IReadOnlyList<TextAsset> LevelFiles => levelFiles;
    }
}
