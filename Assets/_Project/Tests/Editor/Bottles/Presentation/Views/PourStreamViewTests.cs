using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class PourStreamViewTests
    {
        private const int SegmentCount = 48;
        private const float FlowAmplitudeRatio = 0.65f;
        private const float FlowCycleCount = 1.25f;
        private const float WidthVariationRatio = 0.16f;
        private const float MaximumAnimatedSegmentLengthRatio = 1.3f;

        private GameObject parentObject;
        private GameObject streamObject;
        private PourStreamGraphic streamGraphic;
        private PourStreamView view;

        [SetUp]
        public void SetUp()
        {
            parentObject = new GameObject("Parent", typeof(RectTransform));
            RectTransform parent = parentObject.GetComponent<RectTransform>();
            parent.sizeDelta = new Vector2(1000f, 1000f);

            streamObject = new GameObject(
                "Pour Stream",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PourStreamGraphic),
                typeof(PourStreamView));
            RectTransform streamTransform =
                streamObject.GetComponent<RectTransform>();
            streamTransform.SetParent(parent, false);
            streamTransform.sizeDelta = parent.sizeDelta;
            streamGraphic = streamObject.GetComponent<PourStreamGraphic>();
            view = streamObject.GetComponent<PourStreamView>();

            SerializedObject serializedGraphic =
                new SerializedObject(streamGraphic);
            serializedGraphic.FindProperty("segmentCount").intValue =
                SegmentCount;
            serializedGraphic.FindProperty("destinationWidthRatio")
                .floatValue = 0.5f;
            serializedGraphic.FindProperty("sourceTangentLengthRatio")
                .floatValue = 0.3f;
            serializedGraphic.FindProperty("destinationTangentLengthRatio")
                .floatValue = 0.2f;
            serializedGraphic.FindProperty("lateralWaveAmplitudeRatio")
                .floatValue = FlowAmplitudeRatio;
            serializedGraphic.FindProperty("lateralWaveCycleCount")
                .floatValue = FlowCycleCount;
            serializedGraphic.FindProperty("widthVariationRatio")
                .floatValue = WidthVariationRatio;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("streamGraphic").objectReferenceValue =
                streamGraphic;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            streamObject.SetActive(false);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(parentObject);
        }

        [Test]
        public void Show_CreatesSmoothTaperedStreamBetweenPoints()
        {
            ConfigureFlowMotion(0f, 0f, 0f);
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 sourceInterior = parent.TransformPoint(
                new Vector3(-100f, 80f));
            Vector3 sourceMouth = parent.TransformPoint(
                new Vector3(-100f, 100f));
            Vector3 destinationMouth = parent.TransformPoint(
                new Vector3(100f, -100f));
            Vector3 destinationInterior = parent.TransformPoint(
                new Vector3(100f, -120f));

            view.Show(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationInterior,
                Color.magenta);
            view.SetWidth(20f);
            Mesh mesh = RenderStream();

            try
            {
                Vector3[] vertices = mesh.vertices;
                int middleLeftIndex = SegmentCount;
                Vector3 middle =
                    (vertices[middleLeftIndex] + vertices[middleLeftIndex + 1]) *
                    0.5f;
                float sourceWidth = Vector3.Distance(vertices[0], vertices[1]);
                int destinationLeftIndex = SegmentCount * 2;
                float destinationWidth = Vector3.Distance(
                    vertices[destinationLeftIndex],
                    vertices[destinationLeftIndex + 1]);

                Assert.That(streamObject.activeSelf, Is.True);
                Assert.That(streamGraphic.color, Is.EqualTo(Color.magenta));
                Assert.That(vertices, Has.Length.EqualTo((SegmentCount + 1) * 2));
                Assert.That(mesh.triangles, Has.Length.EqualTo(SegmentCount * 6));
                Assert.That(
                    middle.x,
                    Is.InRange(sourceInterior.x, destinationInterior.x));
                Assert.That(middle.y, Is.GreaterThan(0f));
                Assert.That(sourceWidth, Is.EqualTo(20f).Within(0.001f));
                Assert.That(destinationWidth, Is.EqualTo(10f).Within(0.001f));
                AssertPathDoesNotContainSharpCorners(vertices);
                AssertSegmentsHaveSimilarLengths(vertices);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Hide_DisablesStream()
        {
            streamObject.SetActive(true);

            view.Hide();

            Assert.That(streamObject.activeSelf, Is.False);
        }

        [Test]
        public void Show_WithVerticalPath_KeepsConfiguredSourceWidth()
        {
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 sourceInterior = parent.TransformPoint(
                new Vector3(0f, 80f));
            Vector3 sourceMouth = parent.TransformPoint(
                new Vector3(0f, 100f));
            Vector3 destinationMouth = parent.TransformPoint(
                new Vector3(0f, -100f));
            Vector3 destinationInterior = parent.TransformPoint(
                new Vector3(0f, -120f));

            view.Show(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationInterior,
                Color.cyan);
            view.SetWidth(20f);
            Mesh mesh = RenderStream();

            try
            {
                Assert.That(
                    Vector3.Distance(mesh.vertices[0], mesh.vertices[1]),
                    Is.EqualTo(20f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Show_WithAlignedMouths_CurvesAlongSourceExitDirection()
        {
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 sourceInterior = parent.TransformPoint(
                new Vector3(-20f, 80f));
            Vector3 sourceMouth = parent.TransformPoint(
                new Vector3(0f, 100f));
            Vector3 destinationMouth = parent.TransformPoint(
                new Vector3(0f, -100f));
            Vector3 destinationInterior = parent.TransformPoint(
                new Vector3(0f, -120f));

            view.Show(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationInterior,
                Color.cyan);
            view.SetWidth(20f);
            Mesh mesh = RenderStream();

            try
            {
                float maximumCenterX = float.NegativeInfinity;

                for (int segmentIndex = 0;
                     segmentIndex <= SegmentCount;
                     segmentIndex++)
                {
                    int leftVertexIndex = segmentIndex * 2;
                    float centerX = (mesh.vertices[leftVertexIndex].x +
                        mesh.vertices[leftVertexIndex + 1].x) * 0.5f;
                    maximumCenterX = Mathf.Max(maximumCenterX, centerX);
                }

                Assert.That(maximumCenterX, Is.GreaterThan(sourceMouth.x));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [TestCase(-40f, 0.125f)]
        [TestCase(-40f, 0.375f)]
        [TestCase(-40f, 0.5625f)]
        [TestCase(40f, 0.0625f)]
        [TestCase(40f, 0.25f)]
        [TestCase(40f, 0.875f)]
        public void SetFlowPhase_WithTiltedSourceDeformsOnlyFreeStreamAndKeepsMeshValid(
            float sourceInteriorX,
            float flowPhase)
        {
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 sourceInterior = parent.TransformPoint(
                new Vector3(sourceInteriorX, 80f));
            Vector3 sourceMouth = parent.TransformPoint(
                new Vector3(0f, 100f));
            Vector3 destinationMouth = parent.TransformPoint(
                new Vector3(0f, -100f));
            Vector3 destinationInterior = parent.TransformPoint(
                new Vector3(0f, -120f));

            view.Show(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationInterior,
                Color.cyan);
            view.SetWidth(20f);
            Mesh initialMesh = RenderStream();
            view.SetFlowPhase(flowPhase);
            Mesh movedMesh = RenderStream();

            try
            {
                Assert.That(
                    movedMesh.vertexCount,
                    Is.EqualTo(initialMesh.vertexCount));
                Assert.That(
                    movedMesh.triangles,
                    Has.Length.EqualTo(initialMesh.triangles.Length));
                AssertCentersAreEqual(initialMesh, movedMesh, 0);
                AssertCentersAreEqual(initialMesh, movedMesh, 1);
                AssertCentersAreEqual(
                    initialMesh,
                    movedMesh,
                    SegmentCount - 1);
                AssertCentersAreEqual(
                    initialMesh,
                    movedMesh,
                    SegmentCount);
                Assert.That(
                    CalculateMaximumCenterDisplacement(
                        initialMesh,
                        movedMesh),
                    Is.GreaterThan(1f));
                AssertMeshIsFiniteAndWellFormed(movedMesh);
                AssertCrossSectionsFollowCenterline(movedMesh);
                AssertPathDoesNotContainSharpCorners(movedMesh.vertices);
                AssertSegmentsHaveSimilarLengths(
                    movedMesh.vertices,
                    MaximumAnimatedSegmentLengthRatio);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(initialMesh);
                UnityEngine.Object.DestroyImmediate(movedMesh);
            }
        }

        [Test]
        public void SetFlowPhase_AfterCompleteCycleRecreatesInitialMesh()
        {
            ShowVerticalStream();
            Mesh initialMesh = RenderStream();
            view.SetFlowPhase(1f);
            Mesh cycledMesh = RenderStream();

            try
            {
                AssertVerticesAreEqual(initialMesh, cycledMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(initialMesh);
                UnityEngine.Object.DestroyImmediate(cycledMesh);
            }
        }

        [Test]
        public void Show_AfterFlowPhaseChangesResetsFlowMotion()
        {
            ShowVerticalStream();
            Mesh initialMesh = RenderStream();
            view.SetFlowPhase(0.25f);
            Mesh movedMesh = RenderStream();
            ShowVerticalStream();
            Mesh resetMesh = RenderStream();

            try
            {
                Assert.That(
                    CalculateMaximumCenterDisplacement(
                        initialMesh,
                        movedMesh),
                    Is.GreaterThan(1f));
                AssertVerticesAreEqual(initialMesh, resetMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(initialMesh);
                UnityEngine.Object.DestroyImmediate(movedMesh);
                UnityEngine.Object.DestroyImmediate(resetMesh);
            }
        }

        [Test]
        public void SetVisibleRange_RestrictsStreamToRequestedSection()
        {
            ConfigureFlowMotion(0f, 0f, 0f);
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 source = parent.TransformPoint(new Vector3(0f, 100f));
            Vector3 destination = parent.TransformPoint(new Vector3(0f, -100f));

            view.Show(
                source,
                source,
                destination,
                destination,
                Color.cyan);
            view.SetWidth(20f);
            view.SetVisibleRange(0.25f, 0.75f);
            Mesh mesh = RenderStream();

            try
            {
                Assert.That(mesh.bounds.size.y, Is.EqualTo(100f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void UpdatePath_MovesStreamEndInsideDestination()
        {
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            Vector3 sourceInterior = parent.TransformPoint(
                new Vector3(-100f, 80f));
            Vector3 sourceMouth = parent.TransformPoint(
                new Vector3(-100f, 100f));
            Vector3 destinationMouth = parent.TransformPoint(
                new Vector3(100f, -100f));
            Vector3 destinationInterior = parent.TransformPoint(
                new Vector3(100f, -150f));

            view.Show(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationMouth,
                Color.cyan);
            view.UpdatePath(
                sourceInterior,
                sourceMouth,
                destinationMouth,
                destinationInterior);
            view.SetWidth(20f);
            Mesh mesh = RenderStream();

            try
            {
                int destinationLeftIndex = SegmentCount * 2;
                Vector3 destinationCenter =
                    (mesh.vertices[destinationLeftIndex] +
                        mesh.vertices[destinationLeftIndex + 1]) * 0.5f;
                Assert.That(
                    destinationCenter.x,
                    Is.EqualTo(100f).Within(0.001f));
                Assert.That(
                    destinationCenter.y,
                    Is.EqualTo(-150f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void SetVisibleRange_WithReversedProgressThrows()
        {
            Assert.Throws<ArgumentException>(() =>
                view.SetVisibleRange(0.75f, 0.25f));
        }

        [Test]
        public void SetWidth_WithNegativeWidthThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                view.SetWidth(-1f));
        }

        [Test]
        public void SetFlowPhase_WithNegativePhaseThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                view.SetFlowPhase(-1f));
        }

        private void ShowVerticalStream()
        {
            RectTransform parent =
                parentObject.GetComponent<RectTransform>();
            view.Show(
                parent.TransformPoint(new Vector3(0f, 80f)),
                parent.TransformPoint(new Vector3(0f, 100f)),
                parent.TransformPoint(new Vector3(0f, -100f)),
                parent.TransformPoint(new Vector3(0f, -120f)),
                Color.cyan);
            view.SetWidth(20f);
        }

        private void ConfigureFlowMotion(
            float amplitudeRatio,
            float cycleCount,
            float widthVariationRatio)
        {
            SerializedObject serializedGraphic =
                new SerializedObject(streamGraphic);
            serializedGraphic.FindProperty("lateralWaveAmplitudeRatio")
                .floatValue = amplitudeRatio;
            serializedGraphic.FindProperty("lateralWaveCycleCount")
                .floatValue = cycleCount;
            serializedGraphic.FindProperty("widthVariationRatio")
                .floatValue = widthVariationRatio;
            serializedGraphic.ApplyModifiedPropertiesWithoutUndo();
        }

        private Mesh RenderStream()
        {
            streamGraphic.Rebuild(CanvasUpdate.PreRender);
            return UnityEngine.Object.Instantiate(
                streamGraphic.canvasRenderer.GetMesh());
        }

        private static void AssertCentersAreEqual(
            Mesh expectedMesh,
            Mesh actualMesh,
            int segmentIndex)
        {
            Assert.That(
                GetCenter(actualMesh.vertices, segmentIndex),
                Is.EqualTo(GetCenter(expectedMesh.vertices, segmentIndex))
                    .Using(Vector3EqualityComparer.Instance));
        }

        private static float CalculateMaximumCenterDisplacement(
            Mesh firstMesh,
            Mesh secondMesh)
        {
            float maximumDisplacement = 0f;

            for (int segmentIndex = 0;
                 segmentIndex <= SegmentCount;
                 segmentIndex++)
            {
                float displacement = Vector3.Distance(
                    GetCenter(firstMesh.vertices, segmentIndex),
                    GetCenter(secondMesh.vertices, segmentIndex));
                maximumDisplacement = Mathf.Max(
                    maximumDisplacement,
                    displacement);
            }

            return maximumDisplacement;
        }

        private static void AssertMeshIsFiniteAndWellFormed(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;

            for (int segmentIndex = 0;
                 segmentIndex <= SegmentCount;
                 segmentIndex++)
            {
                int leftVertexIndex = segmentIndex * 2;
                Vector3 left = vertices[leftVertexIndex];
                Vector3 right = vertices[leftVertexIndex + 1];
                Assert.That(float.IsNaN(left.x), Is.False);
                Assert.That(float.IsNaN(left.y), Is.False);
                Assert.That(float.IsInfinity(left.x), Is.False);
                Assert.That(float.IsInfinity(left.y), Is.False);
                Assert.That(
                    Vector3.Distance(left, right),
                    Is.GreaterThan(0f));
            }
        }

        private static void AssertCrossSectionsFollowCenterline(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;

            for (int segmentIndex = 1;
                 segmentIndex < SegmentCount;
                 segmentIndex++)
            {
                Vector3 tangent =
                    GetCenter(vertices, segmentIndex + 1) -
                    GetCenter(vertices, segmentIndex - 1);
                int leftVertexIndex = segmentIndex * 2;
                Vector3 crossSection =
                    vertices[leftVertexIndex + 1] -
                    vertices[leftVertexIndex];
                float normalizedDot = Mathf.Abs(Vector3.Dot(
                    tangent.normalized,
                    crossSection.normalized));
                Assert.That(normalizedDot, Is.LessThan(0.001f));
            }
        }

        private static void AssertVerticesAreEqual(
            Mesh expectedMesh,
            Mesh actualMesh)
        {
            Vector3[] expectedVertices = expectedMesh.vertices;
            Vector3[] actualVertices = actualMesh.vertices;

            Assert.That(
                actualVertices,
                Has.Length.EqualTo(expectedVertices.Length));

            for (int vertexIndex = 0;
                 vertexIndex < expectedVertices.Length;
                 vertexIndex++)
            {
                Assert.That(
                    actualVertices[vertexIndex],
                    Is.EqualTo(expectedVertices[vertexIndex])
                        .Using(Vector3EqualityComparer.Instance));
            }
        }

        private static Vector3 GetCenter(
            Vector3[] vertices,
            int segmentIndex)
        {
            int leftVertexIndex = segmentIndex * 2;
            return (vertices[leftVertexIndex] +
                vertices[leftVertexIndex + 1]) * 0.5f;
        }

        private static void AssertPathDoesNotContainSharpCorners(
            Vector3[] vertices)
        {
            Vector2 previousCenter = (vertices[0] + vertices[1]) * 0.5f;
            Vector2 currentCenter = (vertices[2] + vertices[3]) * 0.5f;

            for (int segmentIndex = 2;
                 segmentIndex <= SegmentCount;
                 segmentIndex++)
            {
                int currentLeftIndex = segmentIndex * 2;
                Vector2 nextCenter =
                    (vertices[currentLeftIndex] +
                        vertices[currentLeftIndex + 1]) * 0.5f;
                float turnAngle = Vector2.Angle(
                    currentCenter - previousCenter,
                    nextCenter - currentCenter);
                Assert.That(turnAngle, Is.LessThan(45f));
                previousCenter = currentCenter;
                currentCenter = nextCenter;
            }
        }

        private static void AssertSegmentsHaveSimilarLengths(
            Vector3[] vertices,
            float maximumLengthRatio = 1.1f)
        {
            float shortestSegmentLength = float.PositiveInfinity;
            float longestSegmentLength = 0f;
            Vector2 previousCenter =
                (vertices[0] + vertices[1]) * 0.5f;

            for (int segmentIndex = 1;
                 segmentIndex <= SegmentCount;
                 segmentIndex++)
            {
                int currentLeftIndex = segmentIndex * 2;
                Vector2 currentCenter =
                    (vertices[currentLeftIndex] +
                        vertices[currentLeftIndex + 1]) * 0.5f;
                float segmentLength = Vector2.Distance(
                    previousCenter,
                    currentCenter);
                shortestSegmentLength = Mathf.Min(
                    shortestSegmentLength,
                    segmentLength);
                longestSegmentLength = Mathf.Max(
                    longestSegmentLength,
                    segmentLength);
                previousCenter = currentCenter;
            }

            Assert.That(
                longestSegmentLength / shortestSegmentLength,
                Is.LessThan(maximumLengthRatio));
        }
    }
}
