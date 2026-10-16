using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BottleLiquidGraphic : MaskableGraphic
    {
        [SerializeField, Min(2)] private int surfaceSegmentCount;
        [SerializeField, Min(0f)] private float surfaceAngleMultiplier;
        [SerializeField] private HiddenLiquidGraphic hiddenLiquidGraphic;

        private readonly List<Vector2> layerPolygon = new List<Vector2>();
        private readonly List<Vector2> clippingScratch = new List<Vector2>();

        private Color[] layerColors = Array.Empty<Color>();
        private float[] layerFillAmounts = Array.Empty<float>();
        private float[] surfaceWaveOffsets = Array.Empty<float>();
        private Vector2[] surfaceWaveStripPositions = Array.Empty<Vector2>();
        private float surfaceAngle;
        private LiquidSurfaceWave surfaceWave;
        private float pouringBoundsMaximumY;
        private bool isPouring;

        internal float SurfaceAngle => surfaceAngle;

        public void SetLayers(
            IReadOnlyList<Color> colorsBottomToTop,
            IReadOnlyList<float> fillAmountsBottomToTop,
            IReadOnlyList<bool> hiddenLayersBottomToTop)
        {
            if (colorsBottomToTop == null)
            {
                throw new ArgumentNullException(nameof(colorsBottomToTop));
            }

            if (fillAmountsBottomToTop == null)
            {
                throw new ArgumentNullException(nameof(fillAmountsBottomToTop));
            }

            if (hiddenLayersBottomToTop == null)
            {
                throw new ArgumentNullException(nameof(hiddenLayersBottomToTop));
            }

            if (colorsBottomToTop.Count != fillAmountsBottomToTop.Count ||
                colorsBottomToTop.Count != hiddenLayersBottomToTop.Count)
            {
                throw new ArgumentException(
                    "Liquid colors, fill amounts, and hidden states must have " +
                    "matching counts.");
            }

            if (hiddenLiquidGraphic == null)
            {
                throw new InvalidOperationException(
                    "A hidden liquid graphic is required.");
            }

            EnsureLayerCapacity(colorsBottomToTop.Count);

            for (int layerIndex = 0;
                 layerIndex < colorsBottomToTop.Count;
                 layerIndex++)
            {
                layerColors[layerIndex] = colorsBottomToTop[layerIndex];
                layerFillAmounts[layerIndex] = Mathf.Clamp01(
                    fillAmountsBottomToTop[layerIndex]);
            }

            hiddenLiquidGraphic.SetLayers(
                fillAmountsBottomToTop,
                hiddenLayersBottomToTop);
            SetVerticesDirty();
        }

        public void SetBottleAngle(float angle)
        {
            float targetSurfaceAngle = angle * surfaceAngleMultiplier;

            if (Mathf.Approximately(surfaceAngle, targetSurfaceAngle))
            {
                return;
            }

            surfaceAngle = targetSurfaceAngle;
            hiddenLiquidGraphic?.SetBottlePose(
                angle,
                targetSurfaceAngle);
            SetVerticesDirty();
        }

        public void BeginPour(Vector3 mouthWorldPosition)
        {
            pouringBoundsMaximumY = rectTransform
                .InverseTransformPoint(mouthWorldPosition)
                .y;
            isPouring = true;
            hiddenLiquidGraphic?.BeginPour(mouthWorldPosition);
            SetVerticesDirty();
        }

        public void EndPour()
        {
            if (!isPouring)
            {
                return;
            }

            isPouring = false;
            hiddenLiquidGraphic?.EndPour();
            SetVerticesDirty();
        }

        public void SetSurfaceWave(
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

            float clampedCenterRatio = Mathf.Clamp01(centerRatio);
            float clampedWidthRatio = Mathf.Clamp01(widthRatio);
            LiquidSurfaceWave targetSurfaceWave = new LiquidSurfaceWave(
                clampedCenterRatio,
                clampedWidthRatio,
                height,
                cycleCount,
                phase);

            if (surfaceWave.ApproximatelyEquals(targetSurfaceWave))
            {
                return;
            }

            surfaceWave = targetSurfaceWave;
            hiddenLiquidGraphic?.SetSurfaceWave(
                surfaceSegmentCount,
                surfaceWave);
            SetVerticesDirty();
        }

        public void ClearSurfaceWave()
        {
            if (surfaceWave.ApproximatelyEquals(default))
            {
                return;
            }

            surfaceWave = default;
            hiddenLiquidGraphic?.ClearSurfaceWave();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (layerFillAmounts.Length == 0)
            {
                return;
            }

            float relativeSurfaceAngle = Mathf.Clamp(
                -surfaceAngle,
                -LiquidSurfaceGeometry.MaximumSurfaceAngle,
                LiquidSurfaceGeometry.MaximumSurfaceAngle);
            float surfaceSlope = Mathf.Tan(
                relativeSurfaceAngle * Mathf.Deg2Rad);
            float cumulativeFillAmount = 0f;
            int surfaceLayerIndex = FindSurfaceLayerIndex();
            Rect referenceBounds = rectTransform.rect;
            Rect renderingBounds = GetRenderingBounds(referenceBounds);

            for (int layerIndex = 0;
                 layerIndex < layerFillAmounts.Length;
                 layerIndex++)
            {
                float layerFillAmount = layerFillAmounts[layerIndex];

                if (Mathf.Approximately(layerFillAmount, 0f))
                {
                    continue;
                }

                float lowerFillRatio =
                    LiquidSurfaceGeometry.CalculateAreaPreservingFillRatio(
                        referenceBounds,
                        renderingBounds,
                        cumulativeFillAmount / layerFillAmounts.Length);
                cumulativeFillAmount += layerFillAmount;
                float upperFillRatio =
                    LiquidSurfaceGeometry.CalculateAreaPreservingFillRatio(
                        referenceBounds,
                        renderingBounds,
                        cumulativeFillAmount / layerFillAmounts.Length);

                if (layerIndex == surfaceLayerIndex && HasSurfaceWave())
                {
                    AddWavySurfaceLayer(
                        vertexHelper,
                        renderingBounds,
                        surfaceSlope,
                        lowerFillRatio,
                        upperFillRatio,
                        layerColors[layerIndex]);
                    continue;
                }

                LiquidSurfaceGeometry.BuildLayerPolygon(
                    renderingBounds,
                    surfaceSlope,
                    lowerFillRatio,
                    upperFillRatio,
                    layerPolygon,
                    clippingScratch);
                AddPolygon(
                    vertexHelper,
                    layerPolygon,
                    layerColors[layerIndex]);
            }
        }

        private void AddWavySurfaceLayer(
            VertexHelper vertexHelper,
            Rect bounds,
            float surfaceSlope,
            float lowerFillRatio,
            float upperFillRatio,
            Color color)
        {
            EnsureSurfaceWaveCapacity();
            LiquidSurfaceGeometry.BuildWavyLayerStrip(
                bounds,
                surfaceSlope,
                lowerFillRatio,
                upperFillRatio,
                surfaceSegmentCount,
                surfaceWave,
                surfaceWaveOffsets,
                surfaceWaveStripPositions);
            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int segmentIndex = 0;
                 segmentIndex <= surfaceSegmentCount;
                 segmentIndex++)
            {
                int lowerPositionIndex = segmentIndex * 2;
                AddVertex(
                    vertexHelper,
                    surfaceWaveStripPositions[lowerPositionIndex],
                    color);
                AddVertex(
                    vertexHelper,
                    surfaceWaveStripPositions[lowerPositionIndex + 1],
                    color);

                if (segmentIndex == 0)
                {
                    continue;
                }

                int currentLowerVertex =
                    firstVertexIndex + segmentIndex * 2;
                int previousLowerVertex = currentLowerVertex - 2;
                vertexHelper.AddTriangle(
                    previousLowerVertex,
                    currentLowerVertex + 1,
                    previousLowerVertex + 1);
                vertexHelper.AddTriangle(
                    previousLowerVertex,
                    currentLowerVertex,
                    currentLowerVertex + 1);
            }
        }

        private int FindSurfaceLayerIndex()
        {
            for (int layerIndex = layerFillAmounts.Length - 1;
                 layerIndex >= 0;
                 layerIndex--)
            {
                if (!Mathf.Approximately(
                        layerFillAmounts[layerIndex],
                        0f))
                {
                    return layerIndex;
                }
            }

            return -1;
        }

        private bool HasSurfaceWave()
        {
            return surfaceSegmentCount >= 2 &&
                surfaceWave.IsVisible;
        }

        private Rect GetRenderingBounds(Rect referenceBounds)
        {
            return isPouring
                ? LiquidSurfaceGeometry.ExtendTop(
                    referenceBounds,
                    pouringBoundsMaximumY)
                : referenceBounds;
        }

        private void EnsureLayerCapacity(int layerCount)
        {
            if (layerColors.Length == layerCount)
            {
                return;
            }

            layerColors = new Color[layerCount];
            layerFillAmounts = new float[layerCount];
        }

        private void EnsureSurfaceWaveCapacity()
        {
            int sampleCount = surfaceSegmentCount + 1;

            if (surfaceWaveOffsets.Length == sampleCount)
            {
                return;
            }

            surfaceWaveOffsets = new float[sampleCount];
            surfaceWaveStripPositions = new Vector2[sampleCount * 2];
        }

        private static void AddPolygon(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> polygon,
            Color color)
        {
            if (polygon.Count < 3)
            {
                return;
            }

            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int vertexIndex = 0;
                 vertexIndex < polygon.Count;
                 vertexIndex++)
            {
                UIVertex vertex = UIVertex.simpleVert;
                vertex.position = polygon[vertexIndex];
                vertex.color = color;
                vertexHelper.AddVert(vertex);
            }

            for (int vertexIndex = 1;
                 vertexIndex < polygon.Count - 1;
                 vertexIndex++)
            {
                vertexHelper.AddTriangle(
                    firstVertexIndex,
                    firstVertexIndex + vertexIndex,
                    firstVertexIndex + vertexIndex + 1);
            }
        }

        private static void AddVertex(
            VertexHelper vertexHelper,
            Vector2 position,
            Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }
    }
}
