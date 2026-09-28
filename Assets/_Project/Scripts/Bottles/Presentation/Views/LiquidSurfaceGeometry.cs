using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    internal static class LiquidSurfaceGeometry
    {
        private const int BoundarySearchIterationCount = 24;
        internal const float MaximumSurfaceAngle = 89.9f;

        public static Rect ExtendTop(Rect bounds, float maximumY)
        {
            return Rect.MinMaxRect(
                bounds.xMin,
                bounds.yMin,
                bounds.xMax,
                Mathf.Max(bounds.yMax, maximumY));
        }

        public static float CalculateAreaPreservingFillRatio(
            Rect referenceBounds,
            Rect renderingBounds,
            float referenceFillRatio)
        {
            if (referenceBounds.width <= 0f || referenceBounds.height <= 0f)
            {
                throw new ArgumentException("Reference bounds must have a positive size.", nameof(referenceBounds));
            }

            if (renderingBounds.width <= 0f || renderingBounds.height <= 0f)
            {
                throw new ArgumentException("Rendering bounds must have a positive size.", nameof(renderingBounds));
            }

            float referenceArea = referenceBounds.width * referenceBounds.height;
            float renderingArea = renderingBounds.width * renderingBounds.height;

            return Mathf.Clamp01(referenceFillRatio) * referenceArea / renderingArea;
        }

        public static void BuildLayerPolygon(
            Rect bounds,
            float surfaceSlope,
            float lowerFillRatio,
            float upperFillRatio,
            List<Vector2> polygon,
            List<Vector2> scratch)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException(nameof(polygon));
            }

            if (scratch == null)
            {
                throw new ArgumentNullException(nameof(scratch));
            }

            float lowerBoundary = CalculateBoundaryIntercept(bounds, surfaceSlope, lowerFillRatio);
            float upperBoundary = CalculateBoundaryIntercept(bounds, surfaceSlope, upperFillRatio);

            polygon.Clear();

            polygon.Add(new Vector2(bounds.xMin, bounds.yMin));
            polygon.Add(new Vector2(bounds.xMax, bounds.yMin));
            polygon.Add(new Vector2(bounds.xMax, bounds.yMax));
            polygon.Add(new Vector2(bounds.xMin, bounds.yMax));

            scratch.Clear();

            ClipAgainstBoundary(polygon, scratch, surfaceSlope, upperBoundary, true);

            polygon.Clear();

            ClipAgainstBoundary(scratch, polygon, surfaceSlope, lowerBoundary, false);
        }

        public static void BuildWavyLayerStrip(
            Rect bounds,
            float surfaceSlope,
            float lowerFillRatio,
            float upperFillRatio,
            int segmentCount,
            LiquidSurfaceWave surfaceWave,
            float[] waveOffsets,
            Vector2[] stripPositions)
        {
            if (segmentCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentCount));
            }

            int sampleCount = segmentCount + 1;

            if (waveOffsets == null || waveOffsets.Length < sampleCount)
            {
                throw new ArgumentException(
                    "Wave offsets must contain one value per surface sample.",
                    nameof(waveOffsets));
            }

            if (stripPositions == null || stripPositions.Length < sampleCount * 2)
            {
                throw new ArgumentException(
                    "Strip positions must contain two values per surface sample.",
                    nameof(stripPositions));
            }

            float lowerIntercept = CalculateBoundaryIntercept(bounds, surfaceSlope, lowerFillRatio);
            float upperIntercept = CalculateBoundaryIntercept(bounds, surfaceSlope, upperFillRatio);

            float averageWaveOffset = CalculateWaveOffsets(segmentCount, surfaceWave, waveOffsets);

            for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++)
            {
                float segmentProgress = segmentIndex / (float)segmentCount;

                float positionX = Mathf.Lerp(bounds.xMin, bounds.xMax, segmentProgress);

                float lowerPositionY = Mathf.Clamp(
                    surfaceSlope * positionX + lowerIntercept,
                    bounds.yMin,
                    bounds.yMax);

                float upperPositionY = Mathf.Clamp(
                    surfaceSlope * positionX + upperIntercept,
                    bounds.yMin,
                    bounds.yMax);

                int lowerPositionIndex = segmentIndex * 2;
                stripPositions[lowerPositionIndex] = new Vector2(positionX, lowerPositionY);
                stripPositions[lowerPositionIndex + 1] = new Vector2(positionX, upperPositionY);
            }

            float waveScale = CalculateWaveScale(
                segmentCount,
                stripPositions,
                waveOffsets,
                averageWaveOffset);

            for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++)
            {
                int lowerPositionIndex = segmentIndex * 2;
                Vector2 upperPosition = stripPositions[lowerPositionIndex + 1];
                upperPosition.y +=
                    (waveOffsets[segmentIndex] - averageWaveOffset) *
                    waveScale;
                upperPosition.y = Mathf.Max(
                    stripPositions[lowerPositionIndex].y,
                    upperPosition.y);
                stripPositions[lowerPositionIndex + 1] = upperPosition;
            }
        }

        internal static float CalculateBoundaryIntercept(Rect bounds, float surfaceSlope, float fillRatio)
        {
            float clampedFillRatio = Mathf.Clamp01(fillRatio);

            float slopeAtMinimumX = surfaceSlope * bounds.xMin;
            float slopeAtMaximumX = surfaceSlope * bounds.xMax;

            float lowerIntercept = bounds.yMin - Mathf.Max(slopeAtMinimumX, slopeAtMaximumX);
            float upperIntercept = bounds.yMax - Mathf.Min(slopeAtMinimumX, slopeAtMaximumX);

            if (Mathf.Approximately(clampedFillRatio, 0f))
            {
                return lowerIntercept;
            }

            if (Mathf.Approximately(clampedFillRatio, 1f))
            {
                return upperIntercept;
            }

            float targetArea = bounds.width * bounds.height * clampedFillRatio;

            for (int iteration = 0; iteration < BoundarySearchIterationCount; iteration++)
            {
                float candidateIntercept = (lowerIntercept + upperIntercept) * 0.5f;

                float candidateArea = CalculateAreaBelowLine(bounds, surfaceSlope, candidateIntercept);

                if (candidateArea < targetArea)
                {
                    lowerIntercept = candidateIntercept;
                }
                else
                {
                    upperIntercept = candidateIntercept;
                }
            }

            return (lowerIntercept + upperIntercept) * 0.5f;
        }

        private static float CalculateAreaBelowLine(Rect bounds, float surfaceSlope, float intercept)
        {
            if (Mathf.Approximately(surfaceSlope, 0f))
            {
                float filledHeight = Mathf.Clamp(intercept - bounds.yMin, 0f, bounds.height);
                return bounds.width * filledHeight;
            }

            float areaAtMaximumX = CalculateAreaPrimitive(bounds.xMax, bounds, surfaceSlope, intercept);
            float areaAtMinimumX = CalculateAreaPrimitive(bounds.xMin, bounds, surfaceSlope, intercept);

            return areaAtMaximumX - areaAtMinimumX;
        }

        private static float CalculateAreaPrimitive(float x, Rect bounds, float surfaceSlope, float intercept)
        {
            float heightAboveBottom = surfaceSlope * x + intercept - bounds.yMin;
            float heightAboveTop = heightAboveBottom - bounds.height;

            return (PositiveSquare(heightAboveBottom) - PositiveSquare(heightAboveTop)) / (2f * surfaceSlope);
        }

        private static float CalculateWaveOffsets(
            int segmentCount,
            LiquidSurfaceWave surfaceWave,
            float[] waveOffsets)
        {
            float weightedOffsetTotal = 0f;

            for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++)
            {
                float positionRatio = segmentIndex / (float)segmentCount;

                float waveOffset = surfaceWave.CalculateOffset(positionRatio);
                waveOffsets[segmentIndex] = waveOffset;

                float sampleWeight = segmentIndex == 0 || segmentIndex == segmentCount
                    ? 0.5f
                    : 1f;
                weightedOffsetTotal += waveOffset * sampleWeight;
            }

            return weightedOffsetTotal / segmentCount;
        }

        private static float CalculateWaveScale(
            int segmentCount,
            Vector2[] stripPositions,
            float[] waveOffsets,
            float averageWaveOffset)
        {
            float waveScale = 1f;

            for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++)
            {
                float centeredWaveOffset = waveOffsets[segmentIndex] - averageWaveOffset;

                if (centeredWaveOffset >= 0f)
                {
                    continue;
                }

                int lowerPositionIndex = segmentIndex * 2;
                float availableLiquidHeight =
                    stripPositions[lowerPositionIndex + 1].y -
                    stripPositions[lowerPositionIndex].y;
                waveScale = Mathf.Min(waveScale, availableLiquidHeight / -centeredWaveOffset);
            }

            return Mathf.Clamp01(waveScale);
        }

        private static float PositiveSquare(float value)
        {
            float positiveValue = Mathf.Max(0f, value);
            return positiveValue * positiveValue;
        }

        private static void ClipAgainstBoundary(
            IReadOnlyList<Vector2> input,
            List<Vector2> output,
            float slope,
            float intercept,
            bool keepBelow)
        {
            if (input.Count == 0)
            {
                return;
            }

            Vector2 previous = input[input.Count - 1];

            float previousDistance = CalculateBoundaryDistance(previous, slope, intercept);

            bool previousInside = keepBelow ? previousDistance <= 0f : previousDistance >= 0f;

            for (int index = 0; index < input.Count; index++)
            {
                Vector2 current = input[index];

                float currentDistance = CalculateBoundaryDistance(current, slope, intercept);

                bool currentInside = keepBelow ? currentDistance <= 0f : currentDistance >= 0f;

                if (currentInside != previousInside)
                {
                    float intersectionProgress = previousDistance /
                        (previousDistance - currentDistance);

                    output.Add(Vector2.LerpUnclamped(previous, current, intersectionProgress));
                }

                if (currentInside)
                {
                    output.Add(current);
                }

                previous = current;
                previousDistance = currentDistance;
                previousInside = currentInside;
            }
        }

        private static float CalculateBoundaryDistance(Vector2 point, float slope, float intercept)
        {
            return point.y - (slope * point.x + intercept);
        }
    }
}
