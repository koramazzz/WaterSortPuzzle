using System;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    internal readonly struct LiquidSurfaceWave
    {
        public LiquidSurfaceWave(
            float centerRatio,
            float widthRatio,
            float height,
            float cycleCount,
            float phase)
        {
            if (widthRatio < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(widthRatio));
            }

            if (height < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (cycleCount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleCount));
            }

            CenterRatio = Mathf.Clamp01(centerRatio);
            WidthRatio = Mathf.Clamp01(widthRatio);
            Height = height;
            CycleCount = cycleCount;
            Phase = phase;
        }

        public float CenterRatio { get; }

        public float WidthRatio { get; }

        public float Height { get; }

        public float CycleCount { get; }

        public float Phase { get; }

        public bool IsVisible => WidthRatio > 0f && Height > 0f && CycleCount > 0f;

        public float CalculateOffset(float positionRatio)
        {
            float distanceFromImpact = Mathf.Abs(positionRatio - CenterRatio);

            if (distanceFromImpact >= WidthRatio)
            {
                return 0f;
            }

            float normalizedDistance = distanceFromImpact / WidthRatio;
            float envelope = 1f - normalizedDistance;
            envelope *= envelope;
            float oscillation = Mathf.Cos((normalizedDistance * CycleCount - Phase) * 2f * Mathf.PI);
            return oscillation * envelope * Height;
        }

        public bool ApproximatelyEquals(LiquidSurfaceWave other)
        {
            return Mathf.Approximately(CenterRatio, other.CenterRatio) &&
                Mathf.Approximately(WidthRatio, other.WidthRatio) &&
                Mathf.Approximately(Height, other.Height) &&
                Mathf.Approximately(CycleCount, other.CycleCount) &&
                Mathf.Approximately(Phase, other.Phase);
        }
    }
}
