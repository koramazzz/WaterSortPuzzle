using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class HiddenLiquidGraphicTests
    {
        private const float Width = 100f;
        private const float Height = 200f;

        private GameObject graphicObject;
        private BottleLiquidGraphic liquidGraphic;
        private HiddenLiquidGraphic graphic;
        private Texture2D texture;
        private Sprite sprite;

        [SetUp]
        public void SetUp()
        {
            graphicObject = new GameObject(
                "Bottle Liquid",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(BottleLiquidGraphic));
            RectTransform rectTransform =
                graphicObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(Width, Height);
            liquidGraphic =
                graphicObject.GetComponent<BottleLiquidGraphic>();
            GameObject hiddenGraphicObject = new GameObject(
                "Hidden Liquid",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(HiddenLiquidGraphic));
            hiddenGraphicObject.transform.SetParent(
                graphicObject.transform,
                false);
            hiddenGraphicObject.GetComponent<RectTransform>().sizeDelta =
                new Vector2(Width, Height);
            graphic =
                hiddenGraphicObject.GetComponent<HiddenLiquidGraphic>();
            texture = new Texture2D(2, 2);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f));

            SerializedObject serializedGraphic =
                new SerializedObject(graphic);
            serializedGraphic.FindProperty("hiddenLiquidSprite")
                .objectReferenceValue = sprite;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedLiquidGraphic =
                new SerializedObject(liquidGraphic);
            serializedLiquidGraphic.FindProperty("surfaceSegmentCount")
                .intValue = 16;
            serializedLiquidGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 1f;
            serializedLiquidGraphic.FindProperty("hiddenLiquidGraphic")
                .objectReferenceValue = graphic;
            serializedLiquidGraphic.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(graphicObject);
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [TestCase(-70f)]
        [TestCase(0f)]
        [TestCase(70f)]
        public void SetBottleAngle_PreservesHiddenLiquidVolume(
            float bottleAngle)
        {
            SetLayers(
                new[] { 1f, 1f, 0f, 0f },
                new[] { false, true, false, false });
            liquidGraphic.SetBottleAngle(bottleAngle);

            Mesh mesh = RenderGraphic();

            try
            {
                float expectedArea = Width * Height / 4f;
                Assert.That(
                    CalculateMeshArea(mesh),
                    Is.EqualTo(expectedArea).Within(0.1f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetLayers_WithVisibleLiquid_DoesNotRenderOverlay()
        {
            SetLayers(
                new[] { 1f, 1f },
                new[] { false, false });

            Mesh mesh = RenderGraphic();

            try
            {
                Assert.That(mesh.vertexCount, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetBottleAngle_KeepsHiddenSpriteScaleStable()
        {
            SetLayers(
                new[] { 1f, 1f, 0f, 0f },
                new[] { false, true, false, false });
            liquidGraphic.SetBottleAngle(0f);
            Mesh uprightMesh = RenderGraphic();
            liquidGraphic.SetBottleAngle(70f);
            Mesh tiltedMesh = RenderGraphic();

            try
            {
                Vector2 uprightScale = CalculateSpriteScale(uprightMesh, 0f);
                Vector2 tiltedScale = CalculateSpriteScale(tiltedMesh, 70f);

                Assert.That(
                    tiltedScale.x,
                    Is.EqualTo(uprightScale.x).Within(
                        uprightScale.x * 0.01f));
                Assert.That(
                    tiltedScale.y,
                    Is.EqualTo(uprightScale.y).Within(
                        uprightScale.y * 0.01f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(uprightMesh);
                UnityEngine.Object.DestroyImmediate(tiltedMesh);
            }
        }

        private void SetLayers(
            float[] fillAmounts,
            bool[] hiddenLayers)
        {
            liquidGraphic.SetLayers(
                new Color[fillAmounts.Length],
                fillAmounts,
                hiddenLayers);
        }

        private Mesh RenderGraphic()
        {
            graphic.Rebuild(CanvasUpdate.PreRender);
            return UnityEngine.Object.Instantiate(
                graphic.canvasRenderer.GetMesh());
        }

        private static float CalculateMeshArea(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float area = 0f;

            for (int triangleIndex = 0;
                 triangleIndex < triangles.Length;
                 triangleIndex += 3)
            {
                Vector3 first = vertices[triangles[triangleIndex]];
                Vector3 second = vertices[triangles[triangleIndex + 1]];
                Vector3 third = vertices[triangles[triangleIndex + 2]];
                area += Mathf.Abs(
                    Vector3.Cross(second - first, third - first).z) *
                    0.5f;
            }

            return area;
        }

        private static Vector2 RotateToWorldOrientation(
            Vector3 position,
            float bottleAngle)
        {
            return Quaternion.Euler(0f, 0f, bottleAngle) * position;
        }

        private static Vector2 CalculateSpriteScale(
            Mesh mesh,
            float bottleAngle)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] textureCoordinates = mesh.uv;
            Vector2 minimumVisualPosition = new Vector2(
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector2 maximumVisualPosition = new Vector2(
                float.NegativeInfinity,
                float.NegativeInfinity);
            Vector2 minimumTextureCoordinate = new Vector2(
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector2 maximumTextureCoordinate = new Vector2(
                float.NegativeInfinity,
                float.NegativeInfinity);

            for (int vertexIndex = 0;
                 vertexIndex < vertices.Length;
                 vertexIndex++)
            {
                Vector2 visualPosition = RotateToWorldOrientation(
                    vertices[vertexIndex],
                    bottleAngle);
                minimumVisualPosition = Vector2.Min(
                    minimumVisualPosition,
                    visualPosition);
                maximumVisualPosition = Vector2.Max(
                    maximumVisualPosition,
                    visualPosition);
                minimumTextureCoordinate = Vector2.Min(
                    minimumTextureCoordinate,
                    textureCoordinates[vertexIndex]);
                maximumTextureCoordinate = Vector2.Max(
                    maximumTextureCoordinate,
                    textureCoordinates[vertexIndex]);
            }

            Vector2 visualSize =
                maximumVisualPosition - minimumVisualPosition;
            Vector2 textureSize =
                maximumTextureCoordinate - minimumTextureCoordinate;
            return new Vector2(
                visualSize.x / textureSize.x,
                visualSize.y / textureSize.y);
        }
    }
}
