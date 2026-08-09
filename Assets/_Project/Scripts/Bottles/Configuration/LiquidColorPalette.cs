using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    [CreateAssetMenu(
        fileName = "LiquidColorPalette",
        menuName = "Water Sort Puzzle/Bottles/Liquid Color Palette")]
    public sealed class LiquidColorPalette : ScriptableObject
    {
        [SerializeField] private LiquidColorEntry[] entries =
            Array.Empty<LiquidColorEntry>();

        public Color GetColor(string liquidId)
        {
            if (string.IsNullOrWhiteSpace(liquidId))
            {
                throw new ArgumentException(
                    "Liquid ID cannot be empty.",
                    nameof(liquidId));
            }

            foreach (LiquidColorEntry entry in entries)
            {
                if (string.Equals(
                        entry.LiquidId,
                        liquidId,
                        StringComparison.Ordinal))
                {
                    return entry.Color;
                }
            }

            throw new KeyNotFoundException(
                $"Liquid color palette does not contain ID '{liquidId}'.");
        }

        [Serializable]
        private sealed class LiquidColorEntry
        {
            [SerializeField] private string liquidId;
            [SerializeField] private Color color;

            public string LiquidId => liquidId;
            public Color Color => color;
        }
    }
}
