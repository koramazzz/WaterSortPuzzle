using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class BottleLiquidGraphicTests
    {
        private const float Width = 100f;
        private const float Height = 200f;

        private GameObject graphicObject;
        private BottleLiquidGraphic graphic;
        private HiddenLiquidGraphic hiddenGraphic;
        private Texture2D hiddenTexture;
        private Sprite hiddenSprite;

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
            graphic = graphicObject.GetComponent<BottleLiquidGraphic>();
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
            hiddenGraphic =
                hiddenGraphicObject.GetComponent<HiddenLiquidGraphic>();
            hiddenTexture = new Texture2D(2, 2);
            hiddenSprite = Sprite.Create(
                hiddenTexture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f));
            SerializedObject serializedHiddenGraphic =
                new SerializedObject(hiddenGraphic);
            serializedHiddenGraphic.FindProperty("hiddenLiquidSprite")
                .objectReferenceValue = hiddenSprite;
            serializedHiddenGraphic.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject serializedGraphic =
                new SerializedObject(graphic);
            serializedGraphic.FindProperty("surfaceSegmentCount").intValue =
                16;
            serializedGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 1f;
            serializedGraphic.FindProperty("hiddenLiquidGraphic")
                .objectReferenceValue = hiddenGraphic;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(graphicObject);
            UnityEngine.Object.DestroyImmediate(hiddenSprite);
            UnityEngine.Object.DestroyImmediate(hiddenTexture);
        }

        [TestCase(-70f)]
        [TestCase(0f)]
        [TestCase(70f)]
        public void SetBottleAngle_PreservesLiquidVolume(float bottleAngle)
        {
            graphic.SetLayers(
                new[] { Color.red, Color.blue, Color.green, Color.yellow },
                new[] { 1f, 1f, 0.5f, 0f },
                new[] { false, false, false, false });
            graphic.SetBottleAngle(bottleAngle);

            Mesh mesh = RenderGraphic();

            try
            {
                float expectedArea = Width * Height * 2.5f / 4f;
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
        public void SetBottleAngle_KeepsSurfaceHorizontalInWorldSpace()
        {
            const float bottleAngle = 60f;
            RectTransform rectTransform =
                graphicObject.GetComponent<RectTransform>();
            rectTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                bottleAngle);
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear },
                new[] { 1f, 0f },
                new[] { false, false });
            graphic.SetBottleAngle(bottleAngle);

            Mesh mesh = RenderGraphic();

            try
            {
                List<Vector3> surfaceVertices = FindHighestWorldVertices(
                    rectTransform,
                    mesh.vertices);

                Assert.That(surfaceVertices, Has.Count.EqualTo(2));
                Assert.That(
                    surfaceVertices[0].y,
                    Is.EqualTo(surfaceVertices[1].y).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetBottleAngle_WithReducedResponseCreatesNarrowerWedge()
        {
            const float bottleAngle = 30f;
            SerializedObject serializedGraphic =
                new SerializedObject(graphic);
            serializedGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 0.5f;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear },
                new[] { 1f, 0f },
                new[] { false, false });
            graphic.SetBottleAngle(bottleAngle);

            Mesh mesh = RenderGraphic();

            try
            {
                float minimumSurfaceHeight = float.PositiveInfinity;
                float maximumSurfaceHeight = float.NegativeInfinity;

                foreach (Vector3 vertex in mesh.vertices)
                {
                    if (vertex.y <= -Height * 0.5f + 0.001f)
                    {
                        continue;
                    }

                    minimumSurfaceHeight = Mathf.Min(
                        minimumSurfaceHeight,
                        vertex.y);
                    maximumSurfaceHeight = Mathf.Max(
                        maximumSurfaceHeight,
                        vertex.y);
                }

                float unscaledSurfaceHeightRange = Width * Mathf.Tan(
                    bottleAngle * Mathf.Deg2Rad);
                Assert.That(
                    maximumSurfaceHeight - minimumSurfaceHeight,
                    Is.LessThan(unscaledSurfaceHeightRange));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void BeginPour_WhenBottleTiltsExtendsLiquidAndPreservesVolume()
        {
            const float mouthOffset = 40f;
            Rect referenceBounds = graphic.rectTransform.rect;
            Vector2 mouthPosition = new Vector2(
                referenceBounds.center.x,
                referenceBounds.yMax + mouthOffset);
            Vector3 mouthWorldPosition =
                graphic.rectTransform.TransformPoint(mouthPosition);
            graphic.SetLayers(
                new[] { Color.cyan, Color.cyan },
                new[] { 1f, 1f },
                new[] { false, false });

            graphic.BeginPour(mouthWorldPosition);
            graphic.SetBottleAngle(30f);
            Mesh pouringMesh = RenderGraphic();
            graphic.EndPour();
            Mesh restingMesh = RenderGraphic();

            try
            {
                float expectedArea = Width * Height;
                Assert.That(
                    CalculateMeshArea(pouringMesh),
                    Is.EqualTo(expectedArea).Within(0.1f));
                Assert.That(
                    pouringMesh.bounds.max.y,
                    Is.GreaterThan(referenceBounds.yMax));
                Assert.That(
                    CalculateMeshArea(restingMesh),
                    Is.EqualTo(expectedArea).Within(0.1f));
                Assert.That(
                    restingMesh.bounds.max.y,
                    Is.EqualTo(referenceBounds.yMax).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(pouringMesh);
                UnityEngine.Object.DestroyImmediate(restingMesh);
            }
        }

        [Test]
        public void BeginPour_WithHiddenLiquidExtendsOverlayAndPreservesVolume()
        {
            Rect referenceBounds = graphic.rectTransform.rect;
            Vector2 mouthPosition = new Vector2(
                referenceBounds.center.x,
                referenceBounds.yMax + 40f);
            graphic.SetLayers(
                new[] { Color.cyan },
                new[] { 1f },
                new[] { true });

            graphic.BeginPour(
                graphic.rectTransform.TransformPoint(mouthPosition));
            graphic.SetBottleAngle(30f);
            hiddenGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                hiddenGraphic.canvasRenderer.GetMesh());

            try
            {
                Assert.That(
                    CalculateMeshArea(mesh),
                    Is.EqualTo(Width * Height).Within(0.1f));
                Assert.That(
                    mesh.bounds.max.y,
                    Is.GreaterThan(referenceBounds.yMax));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void BeginPour_WithBuriedHiddenLiquidCentersSpriteOnLayer()
        {
            const float bottleAngle = 80f;
            SerializedObject serializedGraphic =
                new SerializedObject(graphic);
            serializedGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 0.5f;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();
            Rect referenceBounds = graphic.rectTransform.rect;
            Vector2 mouthPosition = new Vector2(
                referenceBounds.center.x,
                referenceBounds.yMax + 40f);
            graphic.SetLayers(
                new[]
                {
                    Color.cyan,
                    Color.red,
                    Color.clear,
                    Color.clear
                },
                new[] { 1f, 1f, 0f, 0f },
                new[] { true, false, false, false });

            graphic.BeginPour(
                graphic.rectTransform.TransformPoint(mouthPosition));
            graphic.SetBottleAngle(bottleAngle);
            hiddenGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                hiddenGraphic.canvasRenderer.GetMesh());

            try
            {
                Vector2 layerCenter = Quaternion.Euler(
                        0f,
                        0f,
                        bottleAngle) *
                    CalculateMeshCentroid(mesh);
                Vector2 spriteCenter = CalculateSpriteCenter(
                    mesh,
                    bottleAngle,
                    Height / 4f);
                Assert.That(
                    spriteCenter.x,
                    Is.EqualTo(layerCenter.x).Within(0.001f));
                Assert.That(
                    spriteCenter.y,
                    Is.EqualTo(layerCenter.y).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetLayers_WithDifferentCountsThrows()
        {
            Assert.Throws<ArgumentException>(() =>
                graphic.SetLayers(
                    new[] { Color.red },
                    Array.Empty<float>(),
                    new[] { false }));
        }

        [Test]
        public void SetSurfaceWave_DeformsSurfaceWithoutChangingVolume()
        {
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear, Color.clear, Color.clear },
                new[] { 1f, 0f, 0f, 0f },
                new[] { false, false, false, false });
            graphic.SetSurfaceWave(
                0.5f,
                0.5f,
                10f,
                1f,
                0.25f);

            Mesh mesh = RenderGraphic();

            try
            {
                float expectedArea = Width * Height / 4f;
                Assert.That(
                    CalculateMeshArea(mesh),
                    Is.EqualTo(expectedArea).Within(0.1f));
                Assert.That(
                    CalculateUpperSurfaceHeightRange(mesh),
                    Is.GreaterThan(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [TestCase(-0.1f, 1f, 1f, "widthRatio")]
        [TestCase(1f, -0.1f, 1f, "height")]
        [TestCase(1f, 1f, -0.1f, "cycleCount")]
        public void SetSurfaceWave_WithNegativeShapeValueThrows(
            float widthRatio,
            float height,
            float cycleCount,
            string parameterName)
        {
            ArgumentOutOfRangeException exception =
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    graphic.SetSurfaceWave(
                        0.5f,
                        widthRatio,
                        height,
                        cycleCount,
                        0f));

            Assert.That(exception.ParamName, Is.EqualTo(parameterName));
        }

        [Test]
        public void SetSurfaceWave_WithFullBottlePreservesVolumeAndWave()
        {
            graphic.SetLayers(
                new[] { Color.cyan, Color.cyan, Color.cyan, Color.cyan },
                new[] { 1f, 1f, 1f, 1f },
                new[] { false, false, false, false });
            graphic.SetSurfaceWave(
                0.5f,
                0.5f,
                10f,
                1f,
                0.25f);

            Mesh mesh = RenderGraphic();

            try
            {
                Assert.That(
                    CalculateMeshArea(mesh),
                    Is.EqualTo(Width * Height).Within(0.1f));
                Assert.That(
                    mesh.bounds.max.y,
                    Is.GreaterThan(Height * 0.5f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetSurfaceWave_WithHiddenSurfaceKeepsOverlayAligned()
        {
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear },
                new[] { 1f, 0f },
                new[] { true, false });
            graphic.SetSurfaceWave(
                0.5f,
                0.5f,
                10f,
                1f,
                0.25f);

            Mesh liquidMesh = RenderGraphic();
            hiddenGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh hiddenMesh = UnityEngine.Object.Instantiate(
                hiddenGraphic.canvasRenderer.GetMesh());

            try
            {
                Assert.That(
                    hiddenMesh.vertices,
                    Has.Length.EqualTo(liquidMesh.vertices.Length));

                for (int vertexIndex = 0;
                     vertexIndex < liquidMesh.vertexCount;
                     vertexIndex++)
                {
                    Assert.That(
                        hiddenMesh.vertices[vertexIndex].x,
                        Is.EqualTo(liquidMesh.vertices[vertexIndex].x)
                            .Within(0.001f));
                    Assert.That(
                        hiddenMesh.vertices[vertexIndex].y,
                        Is.EqualTo(liquidMesh.vertices[vertexIndex].y)
                            .Within(0.001f));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(liquidMesh);
                UnityEngine.Object.DestroyImmediate(hiddenMesh);
            }
        }

        [Test]
        public void SetSurfaceWave_WithShallowLiquidPreservesVolume()
        {
            const float fillAmount = 0.05f;
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear, Color.clear, Color.clear },
                new[] { fillAmount, 0f, 0f, 0f },
                new[] { false, false, false, false });
            graphic.SetSurfaceWave(
                0.5f,
                0.5f,
                10f,
                1f,
                0.25f);

            Mesh mesh = RenderGraphic();

            try
            {
                float expectedArea =
                    Width * Height * fillAmount / 4f;
                Assert.That(
                    CalculateMeshArea(mesh),
                    Is.EqualTo(expectedArea).Within(0.1f));
                Assert.That(
                    CalculateUpperSurfaceHeightRange(mesh),
                    Is.GreaterThan(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ClearSurfaceWave_RestoresFlatSurface()
        {
            graphic.SetLayers(
                new[] { Color.cyan, Color.clear },
                new[] { 1f, 0f },
                new[] { false, false });
            graphic.SetSurfaceWave(
                0.5f,
                0.5f,
                10f,
                1f,
                0.25f);

            graphic.ClearSurfaceWave();
            Mesh mesh = RenderGraphic();

            try
            {
                List<Vector3> surfaceVertices = FindHighestWorldVertices(
                    graphic.rectTransform,
                    mesh.vertices);
                Assert.That(surfaceVertices, Has.Count.EqualTo(2));
                Assert.That(
                    surfaceVertices[0].y,
                    Is.EqualTo(surfaceVertices[1].y).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
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
                area += Mathf.Abs(Vector3.Cross(second - first, third - first).z) *
                    0.5f;
            }

            return area;
        }

        private static Vector2 CalculateMeshCentroid(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector2 weightedCenter = Vector2.zero;
            float totalArea = 0f;

            for (int triangleIndex = 0;
                 triangleIndex < triangles.Length;
                 triangleIndex += 3)
            {
                Vector3 first = vertices[triangles[triangleIndex]];
                Vector3 second = vertices[triangles[triangleIndex + 1]];
                Vector3 third = vertices[triangles[triangleIndex + 2]];
                float triangleArea = Mathf.Abs(
                    Vector3.Cross(second - first, third - first).z) *
                    0.5f;
                Vector3 triangleCenter =
                    (first + second + third) / 3f;
                weightedCenter +=
                    (Vector2)triangleCenter * triangleArea;
                totalArea += triangleArea;
            }

            return weightedCenter / totalArea;
        }

        private static Vector2 CalculateSpriteCenter(
            Mesh mesh,
            float bottleAngle,
            float referenceLayerHeight)
        {
            Vector2 vertex = Quaternion.Euler(0f, 0f, bottleAngle) *
                mesh.vertices[0];
            Vector2 textureCoordinate = mesh.uv[0];
            return vertex -
                (textureCoordinate - Vector2.one * 0.5f) *
                referenceLayerHeight;
        }

        private static float CalculateUpperSurfaceHeightRange(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            float minimumHeight = float.PositiveInfinity;
            float maximumHeight = float.NegativeInfinity;

            for (int vertexIndex = 1;
                 vertexIndex < vertices.Length;
                 vertexIndex += 2)
            {
                minimumHeight = Mathf.Min(
                    minimumHeight,
                    vertices[vertexIndex].y);
                maximumHeight = Mathf.Max(
                    maximumHeight,
                    vertices[vertexIndex].y);
            }

            return maximumHeight - minimumHeight;
        }

        private static List<Vector3> FindHighestWorldVertices(
            RectTransform rectTransform,
            IReadOnlyList<Vector3> localVertices)
        {
            const float PositionTolerance = 0.001f;
            List<Vector3> highestVertices = new List<Vector3>();
            float highestPosition = float.NegativeInfinity;

            foreach (Vector3 localVertex in localVertices)
            {
                Vector3 worldVertex = rectTransform.TransformPoint(localVertex);

                if (worldVertex.y > highestPosition + PositionTolerance)
                {
                    highestPosition = worldVertex.y;
                    highestVertices.Clear();
                    highestVertices.Add(worldVertex);
                }
                else if (Mathf.Abs(worldVertex.y - highestPosition) <=
                    PositionTolerance)
                {
                    highestVertices.Add(worldVertex);
                }
            }

            return highestVertices;
        }
    }
}
