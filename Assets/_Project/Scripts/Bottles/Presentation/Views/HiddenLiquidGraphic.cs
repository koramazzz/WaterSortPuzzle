using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HiddenLiquidGraphic : MaskableGraphic
    {
        [SerializeField] private Sprite hiddenLiquidSprite;

        private readonly List<Vector2> layerPolygon = new List<Vector2>();
        private readonly List<Vector2> clippingScratch = new List<Vector2>();
        private float[] layerFillAmounts = Array.Empty<float>();
        private bool[] hiddenLayers = Array.Empty<bool>();
        private float[] surfaceWaveOffsets = Array.Empty<float>();
        private Vector2[] surfaceWaveStripPositions = Array.Empty<Vector2>();
        private float bottleAngle;
        private float surfaceAngle;
        private float pouringBoundsMaximumY;
        private int surfaceSegmentCount;
        private LiquidSurfaceWave surfaceWave;
        private bool isPouring;

        public override Texture mainTexture => hiddenLiquidSprite != null
            ? hiddenLiquidSprite.texture
            : s_WhiteTexture;

        public void SetLayers(
            IReadOnlyList<float> fillAmountsBottomToTop,
            IReadOnlyList<bool> hiddenLayersBottomToTop)
        {
            if (fillAmountsBottomToTop == null)
            {
                throw new ArgumentNullException(
                    nameof(fillAmountsBottomToTop));
            }

            if (hiddenLayersBottomToTop == null)
            {
                throw new ArgumentNullException(
                    nameof(hiddenLayersBottomToTop));
            }

            if (fillAmountsBottomToTop.Count !=
                hiddenLayersBottomToTop.Count)
            {
                throw new ArgumentException(
                    "Liquid fill amounts and hidden states must have " +
                    "matching counts.");
            }

            EnsureLayerCapacity(fillAmountsBottomToTop.Count);

            for (int layerIndex = 0;
                 layerIndex < fillAmountsBottomToTop.Count;
                 layerIndex++)
            {
                layerFillAmounts[layerIndex] = Mathf.Clamp01(
                    fillAmountsBottomToTop[layerIndex]);
                hiddenLayers[layerIndex] =
                    hiddenLayersBottomToTop[layerIndex];
            }

            SetVerticesDirty();
        }

        public void SetBottlePose(float angle, float liquidSurfaceAngle)
        {
            if (Mathf.Approximately(bottleAngle, angle) &&
                Mathf.Approximately(surfaceAngle, liquidSurfaceAngle))
            {
                return;
            }

            bottleAngle = angle;
            surfaceAngle = liquidSurfaceAngle;
            SetVerticesDirty();
        }

        internal void BeginPour(Vector3 mouthWorldPosition)
        {
            pouringBoundsMaximumY = rectTransform
                .InverseTransformPoint(mouthWorldPosition)
                .y;
            isPouring = true;
            SetVerticesDirty();
        }

        internal void EndPour()
        {
            if (!isPouring)
            {
                return;
            }

            isPouring = false;
            SetVerticesDirty();
        }

        internal void SetSurfaceWave(
            int segmentCount,
            LiquidSurfaceWave wave)
        {
            if (surfaceSegmentCount == segmentCount &&
                surfaceWave.ApproximatelyEquals(wave))
            {
                return;
            }

            surfaceSegmentCount = segmentCount;
            surfaceWave = wave;
            SetVerticesDirty();
        }

        internal void ClearSurfaceWave()
        {
            if (surfaceWave.ApproximatelyEquals(default))
            {
                return;
            }

            surfaceWave = default;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (hiddenLiquidSprite == null || layerFillAmounts.Length == 0)
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
            float referenceLayerHeight =
                referenceBounds.height / layerFillAmounts.Length;

            for (int layerIndex = 0;
                 layerIndex < layerFillAmounts.Length;
                 layerIndex++)
            {
                float layerFillAmount = layerFillAmounts[layerIndex];
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

                if (!hiddenLayers[layerIndex] ||
                    Mathf.Approximately(layerFillAmount, 0f))
                {
                    continue;
                }

                if (layerIndex == surfaceLayerIndex && HasSurfaceWave())
                {
                    AddWavyTexturedLayer(
                        vertexHelper,
                        renderingBounds,
                        surfaceSlope,
                        lowerFillRatio,
                        upperFillRatio,
                        referenceLayerHeight);
                    continue;
                }

                LiquidSurfaceGeometry.BuildLayerPolygon(
                    renderingBounds,
                    surfaceSlope,
                    lowerFillRatio,
                    upperFillRatio,
                    layerPolygon,
                    clippingScratch);
                AddTexturedPolygon(
                    vertexHelper,
                    layerPolygon,
                    referenceLayerHeight);
            }
        }

        private void AddWavyTexturedLayer(
            VertexHelper vertexHelper,
            Rect bounds,
            float surfaceSlope,
            float lowerFillRatio,
            float upperFillRatio,
            float referenceLayerHeight)
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
            CalculateVisualBounds(
                surfaceWaveStripPositions,
                out Vector2 minimumVisualPosition,
                out Vector2 maximumVisualPosition);
            Vector2 visualCenter =
                (minimumVisualPosition + maximumVisualPosition) * 0.5f;
            float referenceLayerWidth =
                CalculateReferenceLayerWidth(referenceLayerHeight);
            Vector4 spriteUv = DataUtility.GetOuterUV(hiddenLiquidSprite);
            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int segmentIndex = 0;
                 segmentIndex <= surfaceSegmentCount;
                 segmentIndex++)
            {
                int lowerPositionIndex = segmentIndex * 2;
                AddTexturedVertex(
                    vertexHelper,
                    surfaceWaveStripPositions[lowerPositionIndex],
                    visualCenter,
                    referenceLayerWidth,
                    referenceLayerHeight,
                    spriteUv);
                AddTexturedVertex(
                    vertexHelper,
                    surfaceWaveStripPositions[lowerPositionIndex + 1],
                    visualCenter,
                    referenceLayerWidth,
                    referenceLayerHeight,
                    spriteUv);

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

        private void AddTexturedPolygon(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> polygon,
            float referenceLayerHeight)
        {
            if (polygon.Count < 3)
            {
                return;
            }

            Vector2 visualCenter = CalculatePolygonVisualCenter(polygon);
            float referenceLayerWidth =
                CalculateReferenceLayerWidth(referenceLayerHeight);
            Vector4 spriteUv = DataUtility.GetOuterUV(hiddenLiquidSprite);
            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int vertexIndex = 0;
                 vertexIndex < polygon.Count;
                 vertexIndex++)
            {
                AddTexturedVertex(
                    vertexHelper,
                    polygon[vertexIndex],
                    visualCenter,
                    referenceLayerWidth,
                    referenceLayerHeight,
                    spriteUv);
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

        private void AddTexturedVertex(
            VertexHelper vertexHelper,
            Vector2 position,
            Vector2 visualCenter,
            float referenceLayerWidth,
            float referenceLayerHeight,
            Vector4 spriteUv)
        {
            Vector2 visualPosition = RotateToWorldOrientation(position);
            float horizontalRatio = 0.5f +
                (visualPosition.x - visualCenter.x) /
                referenceLayerWidth;
            float verticalRatio = 0.5f +
                (visualPosition.y - visualCenter.y) /
                referenceLayerHeight;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertex.uv0 = new Vector2(
                Mathf.LerpUnclamped(
                    spriteUv.x,
                    spriteUv.z,
                    horizontalRatio),
                Mathf.LerpUnclamped(
                    spriteUv.y,
                    spriteUv.w,
                    verticalRatio));
            vertexHelper.AddVert(vertex);
        }

        private void CalculateVisualBounds(
            IReadOnlyList<Vector2> positions,
            out Vector2 minimum,
            out Vector2 maximum)
        {
            minimum = new Vector2(
                float.PositiveInfinity,
                float.PositiveInfinity);
            maximum = new Vector2(
                float.NegativeInfinity,
                float.NegativeInfinity);

            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                Vector2 visualPosition = RotateToWorldOrientation(
                    positions[positionIndex]);
                minimum = Vector2.Min(minimum, visualPosition);
                maximum = Vector2.Max(maximum, visualPosition);
            }
        }

        private Vector2 CalculatePolygonVisualCenter(
            IReadOnlyList<Vector2> polygon)
        {
            float totalArea = 0f;
            Vector2 weightedCenter = Vector2.zero;
            Vector2 first = RotateToWorldOrientation(polygon[0]);

            for (int vertexIndex = 1;
                 vertexIndex < polygon.Count - 1;
                 vertexIndex++)
            {
                Vector2 second = RotateToWorldOrientation(
                    polygon[vertexIndex]);
                Vector2 third = RotateToWorldOrientation(
                    polygon[vertexIndex + 1]);
                float triangleArea = Mathf.Abs(Vector3.Cross(
                    second - first,
                    third - first).z) * 0.5f;
                weightedCenter +=
                    (first + second + third) / 3f * triangleArea;
                totalArea += triangleArea;
            }

            if (Mathf.Approximately(totalArea, 0f))
            {
                return first;
            }

            return weightedCenter / totalArea;
        }

        private float CalculateReferenceLayerWidth(float referenceLayerHeight)
        {
            float spriteAspectRatio =
                hiddenLiquidSprite.rect.width /
                hiddenLiquidSprite.rect.height;
            return referenceLayerHeight * spriteAspectRatio;
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
            return surfaceSegmentCount >= 2 && surfaceWave.IsVisible;
        }

        private Vector2 RotateToWorldOrientation(Vector2 position)
        {
            return Quaternion.Euler(0f, 0f, bottleAngle) * position;
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
            if (layerFillAmounts.Length == layerCount)
            {
                return;
            }

            layerFillAmounts = new float[layerCount];
            hiddenLayers = new bool[layerCount];
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
    }
}
