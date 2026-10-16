using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class BottlePourAnimatorTests
    {
        private GameObject rootObject;
        private RectTransform bottleTransform;
        private BottleLiquidGraphic liquidGraphic;
        private BottlePourAnimator animator;

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject("Root", typeof(RectTransform));
            GameObject bottleObject = new GameObject(
                "Bottle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(BottleLiquidGraphic),
                typeof(BottlePourAnimator));
            bottleTransform = bottleObject.GetComponent<RectTransform>();
            bottleTransform.SetParent(rootObject.transform, false);
            bottleTransform.sizeDelta = new Vector2(100f, 200f);
            bottleTransform.anchoredPosition = new Vector2(-100f, 25f);
            liquidGraphic = bottleObject.GetComponent<BottleLiquidGraphic>();
            GameObject hiddenGraphicObject = new GameObject(
                "Hidden Liquid",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(HiddenLiquidGraphic));
            hiddenGraphicObject.transform.SetParent(
                bottleObject.transform,
                false);
            SerializedObject serializedLiquidGraphic =
                new SerializedObject(liquidGraphic);
            serializedLiquidGraphic.FindProperty("surfaceSegmentCount")
                .intValue = 16;
            serializedLiquidGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 1f;
            serializedLiquidGraphic.FindProperty("hiddenLiquidGraphic")
                .objectReferenceValue =
                    hiddenGraphicObject.GetComponent<HiddenLiquidGraphic>();
            serializedLiquidGraphic.ApplyModifiedPropertiesWithoutUndo();
            liquidGraphic.SetLayers(
                new[] { Color.cyan, Color.clear },
                new[] { 1f, 0f },
                new[] { false, false });
            GameObject mouthObject = new GameObject(
                "Mouth Opening",
                typeof(RectTransform));
            RectTransform mouthOpening =
                mouthObject.GetComponent<RectTransform>();
            mouthOpening.SetParent(bottleObject.transform, false);
            mouthOpening.anchorMin = new Vector2(0.5f, 1f);
            mouthOpening.anchorMax = new Vector2(0.5f, 1f);
            mouthOpening.sizeDelta = new Vector2(40f, 0f);
            animator = bottleObject.GetComponent<BottlePourAnimator>();
            ConfigureAnimator(
                animator,
                liquidGraphic,
                mouthOpening);

            GameObject siblingObject = new GameObject(
                "Sibling",
                typeof(RectTransform));
            siblingObject.transform.SetParent(rootObject.transform, false);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.Kill(animator);
            UnityEngine.Object.DestroyImmediate(rootObject);
        }

        [Test]
        public void PourMovement_PinsMouthAndRestoresBottleTransform()
        {
            Vector2 restingPosition = bottleTransform.anchoredPosition;
            int restingSiblingIndex = bottleTransform.GetSiblingIndex();
            Vector2 pouringMouth = new Vector2(150f, 100f);
            Vector2 localMouth = new Vector2(0f, 100f);
            const float pourAngle = -45f;

            animator.Begin(restingPosition);
            Tween travelTween = animator.CreateTravelTween(
                pouringMouth,
                localMouth,
                pourAngle);
            travelTween.Complete();

            Vector2 animatedMouth = bottleTransform.anchoredPosition +
                (Vector2)(Quaternion.Euler(0f, 0f, pourAngle) * localMouth);
            Assert.That(animatedMouth.x, Is.EqualTo(pouringMouth.x).Within(0.001f));
            Assert.That(animatedMouth.y, Is.EqualTo(pouringMouth.y).Within(0.001f));
            Assert.That(bottleTransform.GetSiblingIndex(), Is.EqualTo(1));

            animator.CaptureReturnPose();
            Tween returnTween = animator.CreateReturnTween();
            returnTween.Goto(0.5f);
            Mesh returningMesh = RenderLiquid();

            try
            {
                Vector3[] returningSurface = GetSurfaceVertices(returningMesh);
                Vector3 firstWorldVertex = bottleTransform.TransformPoint(
                    returningSurface[0]);
                Vector3 secondWorldVertex = bottleTransform.TransformPoint(
                    returningSurface[1]);
                Assert.That(
                    firstWorldVertex.y,
                    Is.EqualTo(secondWorldVertex.y).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(returningMesh);
            }

            returnTween.Complete();
            animator.Complete();

            Assert.That(
                bottleTransform.anchoredPosition.x,
                Is.EqualTo(restingPosition.x).Within(0.001f));
            Assert.That(
                bottleTransform.anchoredPosition.y,
                Is.EqualTo(restingPosition.y).Within(0.001f));
            Assert.That(
                Mathf.DeltaAngle(0f, bottleTransform.localEulerAngles.z),
                Is.Zero.Within(0.001f));
            Assert.That(
                bottleTransform.GetSiblingIndex(),
                Is.EqualTo(restingSiblingIndex));
        }

        [Test]
        public void PlayLiquidImpact_DampensSurfaceWobbleToRest()
        {
            animator.PlayLiquidImpact();
            DOTween.Goto(animator, 0.5f);
            Mesh movingMesh = RenderLiquid();

            try
            {
                Assert.That(
                    CalculateSurfaceHeightRange(movingMesh),
                    Is.GreaterThan(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(movingMesh);
            }

            DOTween.Complete(animator);
            Mesh settledMesh = RenderLiquid();

            try
            {
                Assert.That(
                    CalculateSurfaceHeightRange(settledMesh),
                    Is.Zero.Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(settledMesh);
            }
        }

        [Test]
        public void PlayLiquidInteraction_DampensSurfaceWobbleToRest()
        {
            animator.PlayLiquidInteraction();
            DOTween.Goto(animator, 0.25f);
            Mesh movingMesh = RenderLiquid();

            try
            {
                Assert.That(
                    CalculateSurfaceHeightRange(movingMesh),
                    Is.GreaterThan(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(movingMesh);
            }

            DOTween.Complete(animator);
            Mesh settledMesh = RenderLiquid();

            try
            {
                Assert.That(
                    CalculateSurfaceHeightRange(settledMesh),
                    Is.Zero.Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(settledMesh);
            }
        }

        private Mesh RenderLiquid()
        {
            liquidGraphic.Rebuild(CanvasUpdate.PreRender);
            return UnityEngine.Object.Instantiate(
                liquidGraphic.canvasRenderer.GetMesh());
        }

        private static Vector3[] GetSurfaceVertices(Mesh mesh)
        {
            Vector3[] surfaceVertices = new Vector3[2];
            int surfaceVertexCount = 0;

            foreach (Vector3 vertex in mesh.vertices)
            {
                if (vertex.y <= -99.999f)
                {
                    continue;
                }

                surfaceVertices[surfaceVertexCount] = vertex;
                surfaceVertexCount++;
            }

            Assert.That(surfaceVertexCount, Is.EqualTo(2));
            return surfaceVertices;
        }

        private static float CalculateSurfaceHeightRange(Mesh mesh)
        {
            float minimumHeight = float.PositiveInfinity;
            float maximumHeight = float.NegativeInfinity;

            foreach (Vector3 vertex in mesh.vertices)
            {
                if (vertex.y <= -99.999f)
                {
                    continue;
                }

                minimumHeight = Mathf.Min(minimumHeight, vertex.y);
                maximumHeight = Mathf.Max(maximumHeight, vertex.y);
            }

            return maximumHeight - minimumHeight;
        }

        private static void ConfigureAnimator(
            BottlePourAnimator pourAnimator,
            BottleLiquidGraphic bottleLiquidGraphic,
            RectTransform mouthOpening)
        {
            SerializedObject serializedAnimator =
                new SerializedObject(pourAnimator);
            serializedAnimator.FindProperty("liquidGraphic")
                .objectReferenceValue = bottleLiquidGraphic;
            serializedAnimator.FindProperty("mouthOpening")
                .objectReferenceValue = mouthOpening;
            serializedAnimator.FindProperty("travelDuration").floatValue = 1f;
            serializedAnimator.FindProperty("travelArcHeightRatio").floatValue =
                0.25f;
            serializedAnimator.FindProperty("travelEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("returnDuration").floatValue = 1f;
            serializedAnimator.FindProperty("returnArcHeightRatio").floatValue =
                0.1f;
            serializedAnimator.FindProperty("returnEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("impactWaveHeightRatio")
                .floatValue = 0.1f;
            serializedAnimator.FindProperty("impactWaveWidthRatio")
                .floatValue = 0.5f;
            serializedAnimator.FindProperty("impactWaveDuration")
                .floatValue = 1f;
            serializedAnimator.FindProperty("impactWaveCycleCount")
                .floatValue = 1f;
            serializedAnimator.FindProperty("impactWaveEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("interactionWaveHeightRatio")
                .floatValue = 0.05f;
            serializedAnimator.FindProperty("interactionWaveWidthRatio")
                .floatValue = 0.5f;
            serializedAnimator.FindProperty("interactionWaveDuration")
                .floatValue = 0.5f;
            serializedAnimator.FindProperty("interactionWaveCycleCount")
                .floatValue = 1f;
            serializedAnimator.FindProperty("interactionWaveEase")
                .enumValueIndex = (int)Ease.Linear;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
