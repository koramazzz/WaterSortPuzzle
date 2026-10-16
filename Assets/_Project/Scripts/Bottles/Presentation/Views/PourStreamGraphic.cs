using System;
using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PourStreamGraphic : MaskableGraphic
    {
        private const int ArcLengthSampleMultiplier = 4;

        [SerializeField, Min(2)] private int segmentCount;
        [SerializeField, Range(0f, 1f)] private float destinationWidthRatio;
        [SerializeField, Range(0f, 1f)]
        private float sourceTangentLengthRatio;
        [SerializeField, Range(0f, 1f)]
        private float destinationTangentLengthRatio;

        [Header("Flow Motion")]
        [SerializeField, Min(0f)] private float lateralWaveAmplitudeRatio;
        [SerializeField, Min(0f)] private float lateralWaveCycleCount;
        [SerializeField, Range(0f, 1f)] private float widthVariationRatio;

        private Vector2 sourceInterior;
        private Vector2 sourceMouth;
        private Vector2 sourceControl;
        private Vector2 destinationControl;
        private Vector2 destinationMouth;
        private Vector2 destinationInterior;
        private float sourceExtensionEndProgress;
        private float curveEndProgress;
        private float visibleStart;
        private float visibleEnd = 1f;
        private float streamWidth;
        private float flowPhase;
        private float[] cumulativePathLengths = Array.Empty<float>();
        private Vector2[] centerPositions = Array.Empty<Vector2>();
        private float[] segmentWidths = Array.Empty<float>();
        private float totalPathLength;

        public void SetPath(
            Vector2 sourceInteriorPosition,
            Vector2 sourceMouthPosition,
            Vector2 destinationMouthPosition,
            Vector2 destinationInteriorPosition)
        {
            sourceInterior = sourceInteriorPosition;
            sourceMouth = sourceMouthPosition;
            destinationMouth = destinationMouthPosition;
            destinationInterior = destinationInteriorPosition;
            CalculateControls();
            CalculatePathSections();
            CalculateArcLengthSamples();
            SetVerticesDirty();
        }

        public void SetVisibleRange(float startProgress, float endProgress)
        {
            visibleStart = startProgress;
            visibleEnd = endProgress;
            SetVerticesDirty();
        }

        public void SetWidth(float width)
        {
            streamWidth = width;
            SetVerticesDirty();
        }

        internal void SetFlowPhase(float phase)
        {
            if (Mathf.Approximately(flowPhase, phase))
            {
                return;
            }

            flowPhase = phase;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (segmentCount < 2 ||
                streamWidth <= 0f ||
                visibleStart >= visibleEnd)
            {
                return;
            }

            EnsureMeshDataCapacity();

            for (int segmentIndex = 0;
                 segmentIndex <= segmentCount;
                 segmentIndex++)
            {
                float segmentProgress = segmentIndex / (float)segmentCount;
                float pathDistanceProgress = Mathf.Lerp(
                    visibleStart,
                    visibleEnd,
                    segmentProgress);
                float rawPathProgress = ConvertDistanceToRawProgress(
                    pathDistanceProgress);
                Vector2 position = EvaluateRawPath(rawPathProgress);
                Vector2 tangent = EvaluateRawTangent(rawPathProgress);

                if (Mathf.Approximately(tangent.sqrMagnitude, 0f))
                {
                    tangent = destinationInterior - sourceInterior;
                }

                tangent.Normalize();
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                float curveProgress = CalculateCurveProgress(
                    rawPathProgress);
                float flowEnvelope = CalculateFlowEnvelope(
                    rawPathProgress,
                    curveProgress);
                float waveAngle = CalculateWaveAngle(curveProgress);
                float lateralOffset = Mathf.Sin(waveAngle) *
                    streamWidth *
                    lateralWaveAmplitudeRatio *
                    flowEnvelope;
                centerPositions[segmentIndex] =
                    position + normal * lateralOffset;
                float baseWidth = Mathf.Lerp(
                    streamWidth,
                    streamWidth * destinationWidthRatio,
                    pathDistanceProgress);
                float widthScale = 1f +
                    Mathf.Cos(waveAngle) *
                    widthVariationRatio *
                    flowEnvelope;
                segmentWidths[segmentIndex] = baseWidth * widthScale;
            }

            for (int segmentIndex = 0;
                 segmentIndex <= segmentCount;
                 segmentIndex++)
            {
                Vector2 tangent = CalculateDeformedTangent(segmentIndex);
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                Vector2 position = centerPositions[segmentIndex];
                float halfWidth = segmentWidths[segmentIndex] * 0.5f;
                AddVertex(vertexHelper, position - normal * halfWidth);
                AddVertex(vertexHelper, position + normal * halfWidth);

                if (segmentIndex == 0)
                {
                    continue;
                }

                int currentLeft = segmentIndex * 2;
                int previousLeft = currentLeft - 2;
                vertexHelper.AddTriangle(
                    previousLeft,
                    previousLeft + 1,
                    currentLeft + 1);
                vertexHelper.AddTriangle(
                    previousLeft,
                    currentLeft + 1,
                    currentLeft);
            }
        }

        private Vector2 CalculateDeformedTangent(int segmentIndex)
        {
            Vector2 tangent;

            if (segmentIndex == 0)
            {
                tangent = centerPositions[1] - centerPositions[0];
            }
            else if (segmentIndex == segmentCount)
            {
                tangent = centerPositions[segmentIndex] -
                    centerPositions[segmentIndex - 1];
            }
            else
            {
                tangent = centerPositions[segmentIndex + 1] -
                    centerPositions[segmentIndex - 1];
            }

            if (Mathf.Approximately(tangent.sqrMagnitude, 0f))
            {
                tangent = destinationInterior - sourceInterior;
            }

            return tangent.normalized;
        }

        private float CalculateCurveProgress(float rawPathProgress)
        {
            return Mathf.InverseLerp(
                sourceExtensionEndProgress,
                curveEndProgress,
                rawPathProgress);
        }

        private float CalculateFlowEnvelope(
            float rawPathProgress,
            float curveProgress)
        {
            if (rawPathProgress <= sourceExtensionEndProgress ||
                rawPathProgress >= curveEndProgress)
            {
                return 0f;
            }

            float sine = Mathf.Sin(curveProgress * Mathf.PI);
            return sine * sine;
        }

        private float CalculateWaveAngle(float curveProgress)
        {
            float waveProgress =
                curveProgress * lateralWaveCycleCount -
                flowPhase;
            return waveProgress * 2f * Mathf.PI;
        }

        private Vector2 EvaluateRawPath(float progress)
        {
            if (progress < sourceExtensionEndProgress)
            {
                float extensionProgress = Mathf.InverseLerp(
                    0f,
                    sourceExtensionEndProgress,
                    progress);
                return Vector2.Lerp(
                    sourceInterior,
                    sourceMouth,
                    extensionProgress);
            }

            if (progress < curveEndProgress)
            {
                float curveProgress = Mathf.InverseLerp(
                    sourceExtensionEndProgress,
                    curveEndProgress,
                    progress);
                float inverseProgress = 1f - curveProgress;
                return inverseProgress * inverseProgress * inverseProgress *
                        sourceMouth +
                    3f * inverseProgress * inverseProgress * curveProgress *
                        sourceControl +
                    3f * inverseProgress * curveProgress * curveProgress *
                        destinationControl +
                    curveProgress * curveProgress * curveProgress *
                        destinationMouth;
            }

            float destinationProgress = Mathf.InverseLerp(
                curveEndProgress,
                1f,
                progress);
            return Vector2.Lerp(
                destinationMouth,
                destinationInterior,
                destinationProgress);
        }

        private Vector2 EvaluateRawTangent(float progress)
        {
            if (progress < sourceExtensionEndProgress)
            {
                return sourceMouth - sourceInterior;
            }

            if (progress < curveEndProgress)
            {
                float curveProgress = Mathf.InverseLerp(
                    sourceExtensionEndProgress,
                    curveEndProgress,
                    progress);
                float inverseProgress = 1f - curveProgress;
                return 3f * inverseProgress * inverseProgress *
                        (sourceControl - sourceMouth) +
                    6f * inverseProgress * curveProgress *
                        (destinationControl - sourceControl) +
                    3f * curveProgress * curveProgress *
                        (destinationMouth - destinationControl);
            }

            return destinationInterior - destinationMouth;
        }

        private void CalculateControls()
        {
            Vector2 exteriorDirection = destinationMouth - sourceMouth;
            Vector2 sourceDirection = sourceMouth - sourceInterior;
            Vector2 destinationDirection =
                destinationInterior - destinationMouth;

            if (Mathf.Approximately(sourceDirection.sqrMagnitude, 0f))
            {
                sourceDirection = exteriorDirection;
            }

            if (Mathf.Approximately(destinationDirection.sqrMagnitude, 0f))
            {
                destinationDirection = exteriorDirection;
            }

            float exteriorDistance = exteriorDirection.magnitude;
            sourceControl = sourceMouth +
                sourceDirection.normalized * exteriorDistance *
                sourceTangentLengthRatio;
            destinationControl = destinationMouth -
                destinationDirection.normalized * exteriorDistance *
                destinationTangentLengthRatio;
        }

        private void CalculatePathSections()
        {
            float sourceExtensionLength = Vector2.Distance(
                sourceInterior,
                sourceMouth);
            float curveLength = Vector2.Distance(
                    sourceMouth,
                    sourceControl) +
                Vector2.Distance(
                    sourceControl,
                    destinationControl) +
                Vector2.Distance(
                    destinationControl,
                    destinationMouth);
            float destinationExtensionLength = Vector2.Distance(
                destinationMouth,
                destinationInterior);
            float totalLength = sourceExtensionLength +
                curveLength +
                destinationExtensionLength;

            if (Mathf.Approximately(totalLength, 0f))
            {
                sourceExtensionEndProgress = 0f;
                curveEndProgress = 1f;
                return;
            }

            sourceExtensionEndProgress = sourceExtensionLength / totalLength;
            curveEndProgress =
                (sourceExtensionLength + curveLength) / totalLength;
        }

        private void CalculateArcLengthSamples()
        {
            int sampleCount = Mathf.Max(
                2,
                segmentCount * ArcLengthSampleMultiplier);
            int requiredLength = sampleCount + 1;

            if (cumulativePathLengths.Length != requiredLength)
            {
                cumulativePathLengths = new float[requiredLength];
            }

            cumulativePathLengths[0] = 0f;
            Vector2 previousPosition = EvaluateRawPath(0f);

            for (int sampleIndex = 1;
                 sampleIndex <= sampleCount;
                 sampleIndex++)
            {
                float rawProgress = sampleIndex / (float)sampleCount;
                Vector2 position = EvaluateRawPath(rawProgress);
                cumulativePathLengths[sampleIndex] =
                    cumulativePathLengths[sampleIndex - 1] +
                    Vector2.Distance(previousPosition, position);
                previousPosition = position;
            }

            totalPathLength = cumulativePathLengths[sampleCount];
        }

        private float ConvertDistanceToRawProgress(float distanceProgress)
        {
            float clampedProgress = Mathf.Clamp01(distanceProgress);

            if (cumulativePathLengths.Length < 2 ||
                Mathf.Approximately(totalPathLength, 0f))
            {
                return clampedProgress;
            }

            if (Mathf.Approximately(clampedProgress, 0f) ||
                Mathf.Approximately(clampedProgress, 1f))
            {
                return clampedProgress;
            }

            float targetLength = totalPathLength * clampedProgress;
            int sampleIndex = FindLengthSampleIndex(targetLength);
            int sampleCount = cumulativePathLengths.Length - 1;
            float lowerLength = cumulativePathLengths[sampleIndex - 1];
            float upperLength = cumulativePathLengths[sampleIndex];
            float sampleProgress = Mathf.InverseLerp(
                lowerLength,
                upperLength,
                targetLength);
            return Mathf.Lerp(
                (sampleIndex - 1) / (float)sampleCount,
                sampleIndex / (float)sampleCount,
                sampleProgress);
        }

        private int FindLengthSampleIndex(float targetLength)
        {
            int lowerIndex = 1;
            int upperIndex = cumulativePathLengths.Length - 1;

            while (lowerIndex < upperIndex)
            {
                int middleIndex = lowerIndex +
                    (upperIndex - lowerIndex) / 2;

                if (cumulativePathLengths[middleIndex] < targetLength)
                {
                    lowerIndex = middleIndex + 1;
                }
                else
                {
                    upperIndex = middleIndex;
                }
            }

            return lowerIndex;
        }

        private void EnsureMeshDataCapacity()
        {
            int requiredLength = segmentCount + 1;

            if (centerPositions.Length == requiredLength)
            {
                return;
            }

            centerPositions = new Vector2[requiredLength];
            segmentWidths = new float[requiredLength];
        }

        private void AddVertex(VertexHelper vertexHelper, Vector2 position)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }
    }
}
