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

        private BottleLiquidGraphic liquidGraphic;

        internal void RefreshFrom(BottleLiquidGraphic source)
        {
            liquidGraphic = source;
            SetVerticesDirty();
        }

        public override Texture mainTexture => hiddenLiquidSprite != null
        ? hiddenLiquidSprite.texture
        : base.mainTexture;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (hiddenLiquidSprite == null || liquidGraphic == null)
            {
                return;
            }

            float referenceLayerHeight = liquidGraphic.ReferenceLayerHeight;

            if (Mathf.Approximately(referenceLayerHeight, 0f))
            {
                return;
            }

            for (int layerIndex = 0; layerIndex < liquidGraphic.LayerCount; layerIndex++)
            {
                if (!liquidGraphic.IsLayerHidden(layerIndex))
                {
                    continue;
                }

                IReadOnlyList<Vector2> positions = liquidGraphic.GetLayerGeometry(
                        layerIndex,
                        out bool usesTriangleStrip);

                if (usesTriangleStrip)
                {
                    AddTexturedTriangleStrip(vertexHelper, positions, referenceLayerHeight);
                    continue;
                }

                AddTexturedPolygon(vertexHelper, positions, referenceLayerHeight);
            }
        }

        private void AddTexturedTriangleStrip(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> positions,
            float referenceLayerHeight)
        {
            if (positions.Count < 4)
            {
                return;
            }

            CalculateVisualBounds(
                positions,
                out Vector2 minimumVisualPosition,
                out Vector2 maximumVisualPosition);

            Vector2 visualCenter = (minimumVisualPosition + maximumVisualPosition) * 0.5f;

            float referenceLayerWidth = CalculateReferenceLayerWidth(referenceLayerHeight);

            Vector4 spriteUv = DataUtility.GetOuterUV(hiddenLiquidSprite);

            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int positionIndex = 0; positionIndex < positions.Count; positionIndex++)
            {
                AddTexturedVertex(
                    vertexHelper,
                    positions[positionIndex],
                    visualCenter,
                    referenceLayerWidth,
                    referenceLayerHeight,
                    spriteUv);
            }

            LiquidMeshTopology.AddTriangleStripTriangles(vertexHelper, firstVertexIndex, positions.Count);
        }

        private void AddTexturedPolygon(
            VertexHelper vertexHelper,
            IReadOnlyList<Vector2> positions,
            float referenceLayerHeight)
        {
            if (positions.Count < 3)
            {
                return;
            }

            Vector2 visualCenter = CalculatePolygonVisualCenter(positions);

            float referenceLayerWidth = CalculateReferenceLayerWidth(referenceLayerHeight);

            Vector4 spriteUv = DataUtility.GetOuterUV(hiddenLiquidSprite);

            int firstVertexIndex = vertexHelper.currentVertCount;

            for (int positionIndex = 0; positionIndex < positions.Count; positionIndex++)
            {
                AddTexturedVertex(
                    vertexHelper,
                    positions[positionIndex],
                    visualCenter,
                    referenceLayerWidth,
                    referenceLayerHeight,
                    spriteUv);
            }

            LiquidMeshTopology.AddPolygonTriangles(vertexHelper, firstVertexIndex, positions.Count);
        }

        private void AddTexturedVertex(
            VertexHelper vertexHelper,
            Vector2 position,
            Vector2 visualCenter,
            float referenceLayerWidth,
            float referenceLayerHeight,
            Color color,
            Vector4 spriteUv)
        {
            Vector2 visualPosition = RotateToWorldOrientation(position);

            float horizontalRatio = 0.5f + (visualPosition.x - visualCenter.x) / referenceLayerWidth;
            float verticalRatio = 0.5f + (visualPosition.y - visualCenter.y) / referenceLayerHeight;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertex.uv0 = new Vector2(
                Mathf.LerpUnclamped(spriteUv.x, spriteUv.z, horizontalRatio),
                Mathf.LerpUnclamped(spriteUv.y, spriteUv.w, verticalRatio));
            vertexHelper.AddVert(vertex);
        }

        private void CalculateVisualBounds(
            IReadOnlyList<Vector2> positions,
            out Vector2 minimum,
            out Vector2 maximum)
        {
            minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            for (int positionIndex = 0; positionIndex < positions.Count; positionIndex++)
            {
                Vector2 visualPosition = RotateToWorldOrientation(positions[positionIndex]);
                minimum = Vector2.Min(minimum, visualPosition);
                maximum = Vector2.Max(maximum, visualPosition);
            }
        }

        private Vector2 CalculatePolygonVisualCenter(IReadOnlyList<Vector2> positions)
        {
            float totalArea = 0f;
            Vector2 weightedCenter = Vector2.zero;
            Vector2 first = RotateToWorldOrientation(positions[0]);

            for (int positionIndex = 1; positionIndex < positions.Count - 1; positionIndex++)
            {
                Vector2 second = RotateToWorldOrientation(positions[positionIndex]);
                Vector2 third = RotateToWorldOrientation(positions[positionIndex + 1]);

                float triangleArea = Mathf.Abs(Vector3.Cross(second - first, third - first).z) * 0.5f;
                weightedCenter +=(first + second + third) / 3f * triangleArea;
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
            float spriteAspectRatio = hiddenLiquidSprite.rect.width / hiddenLiquidSprite.rect.height;
            return referenceLayerHeight * spriteAspectRatio;
        }

        private Vector2 RotateToWorldOrientation(Vector2 position)
        {
            return Quaternion.Euler(0f, 0f, liquidGraphic.BottleAngle) * position;
        }
    }
}
