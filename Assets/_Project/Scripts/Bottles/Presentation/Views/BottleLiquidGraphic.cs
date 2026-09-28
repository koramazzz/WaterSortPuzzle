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

        private readonly List<Vector2> clippingScratch = new List<Vector2>();

        private Color[] layerColors = Array.Empty<Color>();
        private float[] layerFillAmounts = Array.Empty<float>();
        private bool[] hiddenLayers = Array.Empty<bool>();

        private List<Vector2>[] layerGeometry = Array.Empty<List<Vector2>>();
        private bool[] layerUsesTriangleStrip = Array.Empty<bool>();

        private float[] surfaceWaveOffsets = Array.Empty<float>();
        private Vector2[] surfaceWaveStripPositions = Array.Empty<Vector2>();

        private float bottleAngle;
        private float surfaceAngle;
        private LiquidSurfaceWave surfaceWave;
        private float pouringBoundsMaximumY;
        private bool isPouring;
        private bool isLayerGeometryDirty = true;

        internal float SurfaceAngle => surfaceAngle;

        internal float BottleAngle => bottleAngle;

        internal int LayerCount => layerFillAmounts.Length;
        internal float ReferenceLayerHeight => LayerCount == 0
            ? 0f
            : rectTransform.rect.height / LayerCount;

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
                    "Liquid colors, fill amounts, and hidden states must have matching counts.");
            }

            if (hiddenLiquidGraphic == null)
            {
                throw new InvalidOperationException("A hidden liquid graphic is required.");
            }

            EnsureLayerCapacity(colorsBottomToTop.Count);

            for (int layerIndex = 0; layerIndex < colorsBottomToTop.Count; layerIndex++)
            {
                layerColors[layerIndex] = colorsBottomToTop[layerIndex];
                layerFillAmounts[layerIndex] = Mathf.Clamp01(fillAmountsBottomToTop[layerIndex]);
                hiddenLayers[layerIndex] = hiddenLayersBottomToTop[layerIndex];
            }

            InvalidateLayerGeometry();
        }

        public void SetBottleAngle(float angle)
        {
            float targetSurfaceAngle = angle * surfaceAngleMultiplier;

            if (Mathf.Approximately(bottleAngle, angle) &&
                Mathf.Approximately(surfaceAngle, targetSurfaceAngle))
            {
                return;
            }

            bottleAngle = angle;
            surfaceAngle = targetSurfaceAngle;
            InvalidateLayerGeometry();
        }

        public void BeginPour(Vector3 mouthWorldPosition)
        {
            pouringBoundsMaximumY = rectTransform
                .InverseTransformPoint(mouthWorldPosition)
                .y;
            isPouring = true;
            InvalidateLayerGeometry();
        }

        public void EndPour()
        {
            if (!isPouring)
            {
                return;
            }

            isPouring = false;
            InvalidateLayerGeometry();
        }

        public void SetSurfaceWave(
            float centerRatio,
            float widthRatio,
            float height,
            float cycleCount,
            float phase)
        {
            LiquidSurfaceWave targetSurfaceWave = new LiquidSurfaceWave(
                centerRatio,
                widthRatio,
                height,
                cycleCount,
                phase);

            if (surfaceWave.ApproximatelyEquals(targetSurfaceWave))
            {
                return;
            }

            surfaceWave = targetSurfaceWave;
            InvalidateLayerGeometry();
        }

        public void ClearSurfaceWave()
        {
            if (surfaceWave.ApproximatelyEquals(default))
            {
                return;
            }

            surfaceWave = default;
            InvalidateLayerGeometry();
        }

        internal bool IsLayerHidden(int layerIndex)
        {
            return hiddenLayers[layerIndex];
        }

        internal IReadOnlyList<Vector2> GetLayerGeometry(int layerIndex, out bool usesTriangleStrip)
        {
            EnsureLayerGeometry();
            usesTriangleStrip = layerUsesTriangleStrip[layerIndex];
            return layerGeometry[layerIndex];
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            EnsureLayerGeometry();

            for (int layerIndex = 0; layerIndex < layerGeometry.Length; layerIndex++)
            {
                IReadOnlyList<Vector2> positions = layerGeometry[layerIndex];

                if (layerUsesTriangleStrip[layerIndex])
                {
                    AddTriangleStrip(vertexHelper, positions, layerColors[layerIndex]);
                    continue;
                }

                AddPolygon(vertexHelper, positions, layerColors[layerIndex]);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            InvalidateLayerGeometry();
        }

        private void InvalidateLayerGeometry()
        {
            isLayerGeometryDirty = true;
            SetVerticesDirty();
            hiddenLiquidGraphic?.RefreshFrom(this);
        }

        private void EnsureLayerGeometry()
        {
            if (!isLayerGeometryDirty)
            {
                return;
            }

            ClearLayerGeometry();

            if (layerFillAmounts.Length == 0)
            {
                isLayerGeometryDirty = false;
                return;
            }

            Rect referenceBounds = rectTransform.rect;

            if (referenceBounds.width <= 0f || referenceBounds.height <= 0f)
            {
                isLayerGeometryDirty = false;
                return;
            }

            Rect renderingBounds = isPouring
                ? LiquidSurfaceGeometry.ExtendTop(referenceBounds, pouringBoundsMaximumY)
                : referenceBounds;

            float relativeSurfaceAngle = Mathf.Clamp(-surfaceAngle,
                -LiquidSurfaceGeometry.MaximumSurfaceAngle,
                 LiquidSurfaceGeometry.MaximumSurfaceAngle);

            float surfaceSlope = Mathf.Tan(relativeSurfaceAngle * Mathf.Deg2Rad);

            int surfaceLayerIndex = FindSurfaceLayerIndex();

            float cumulativeFillAmount = 0f;

            for (int layerIndex = 0; layerIndex < layerFillAmounts.Length; layerIndex++)
            {
                float layerFillAmount = layerFillAmounts[layerIndex];

                float lowerFillRatio = CalculateRenderingFillRatio(
                    referenceBounds,
                    renderingBounds,
                    cumulativeFillAmount);

                cumulativeFillAmount += layerFillAmount;

                float upperFillRatio = CalculateRenderingFillRatio(
                    referenceBounds,
                    renderingBounds,
                    cumulativeFillAmount);

                if (Mathf.Approximately(layerFillAmount, 0f))
                {
                    continue;
                }

                if (layerIndex == surfaceLayerIndex && HasSurfaceWave())
                {
                    PrepareWavyLayerGeometry(
                        layerIndex,
                        renderingBounds,
                        surfaceSlope,
                        lowerFillRatio,
                        upperFillRatio);
                    continue;
                }

                LiquidSurfaceGeometry.BuildLayerPolygon(
                    renderingBounds,
                    surfaceSlope,
                    lowerFillRatio,
                    upperFillRatio,
                    layerGeometry[layerIndex],
                    clippingScratch);
            }

            isLayerGeometryDirty = false;
        }

        private void ClearLayerGeometry()
        {
            for (int layerIndex = 0; layerIndex < layerGeometry.Length; layerIndex++)
            {
                layerGeometry[layerIndex].Clear();
                layerUsesTriangleStrip[layerIndex] = false;
            }
        }

        private float CalculateRenderingFillRatio(
            Rect referenceBounds,
            Rect renderingBounds,
            float cumulativeFillAmount)
        {
            return LiquidSurfaceGeometry.CalculateAreaPreservingFillRatio(
                referenceBounds,
                renderingBounds,
                cumulativeFillAmount / layerFillAmounts.Length);
        }

        private int FindSurfaceLayerIndex()
        {
            for (int layerIndex = layerFillAmounts.Length - 1; layerIndex >= 0; layerIndex--)
            {
                if (!Mathf.Approximately(layerFillAmounts[layerIndex], 0f))
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

        private void PrepareWavyLayerGeometry(
            int layerIndex,
            Rect bounds,
            float surfaceSlope,
            float lowerFillRatio,
            float upperFillRatio)
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

            List<Vector2> positions = layerGeometry[layerIndex];

            int positionCount = (surfaceSegmentCount + 1) * 2;
            for (int positionIndex = 0; positionIndex < positionCount; positionIndex++)
            {
                positions.Add(surfaceWaveStripPositions[positionIndex]);
            }

            layerUsesTriangleStrip[layerIndex] = true;
        }

        private void EnsureLayerCapacity(int layerCount)
        {
            if (layerColors.Length == layerCount)
            {
                return;
            }

            layerColors = new Color[layerCount];
            layerFillAmounts = new float[layerCount];
            hiddenLayers = new bool[layerCount];

            layerGeometry = new List<Vector2>[layerCount];
            layerUsesTriangleStrip = new bool[layerCount];

            for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
            {
                layerGeometry[layerIndex] = new List<Vector2>();
            }
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
            IReadOnlyList<Vector2> positions,
            Color color)
        {
            if (positions.Count < 3)
            {
                return;
            }

            int firstVertexIndex = AddVertices(vertexHelper, positions, color);

            LiquidMeshTopology.AddPolygonTriangles(vertexHelper, firstVertexIndex, positions.Count);
        }

        private static void AddTriangleStrip(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> positions,
            Color color)
        {
            if (positions.Count < 4)
            {
                return;
            }

            int firstVertexIndex = AddVertices(vertexHelper, positions, color);

            LiquidMeshTopology.AddTriangleStripTriangles(vertexHelper, firstVertexIndex, positions.Count);
        }

        private static int AddVertices(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> positions,
            Color color)
        {
            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int positionIndex = 0; positionIndex < positions.Count; positionIndex++)
            {
                AddVertex(vertexHelper, positions[positionIndex], color);
            }

            return firstVertexIndex;
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }
    }
}
