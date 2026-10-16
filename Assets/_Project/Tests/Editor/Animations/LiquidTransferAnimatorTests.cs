using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class LiquidTransferAnimatorTests
    {
        private const float TravelDuration = 1f;
        private const float StreamEnterDuration = 0.2f;
        private const float TransferDurationPerLiquid = 0.25f;
        private const float StreamExitDuration = 0.15f;
        private const float ReturnDuration = 1f;
        private const float TimelineBoundaryOffset = 0.001f;
        private static readonly Rect LiquidBounds =
            Rect.MinMaxRect(-50f, -80f, 50f, 60f);

        private GameObject rootObject;
        private GameObject bottleParentObject;
        private RectTransform source;
        private RectTransform destination;
        private GameObject streamObject;
        private LiquidTransferAnimator animator;
        private BottlePourAnimator sourceAnimator;
        private BottlePourAnimator destinationAnimator;

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject("Animation Root", typeof(RectTransform));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(1000f, 1000f);

            bottleParentObject = new GameObject(
                "Bottle Parent",
                typeof(RectTransform),
                typeof(LiquidTransferAnimator));
            RectTransform bottleParent =
                bottleParentObject.GetComponent<RectTransform>();
            bottleParent.SetParent(root, false);
            bottleParent.sizeDelta = root.sizeDelta;

            source = CreateBottle("Source", bottleParent, -200f);
            destination = CreateBottle("Destination", bottleParent, 200f);
            sourceAnimator = source.GetComponent<BottlePourAnimator>();
            destinationAnimator =
                destination.GetComponent<BottlePourAnimator>();
            ConfigureBottleAnimator(
                sourceAnimator,
                source.GetComponent<BottleLiquidGraphic>());
            ConfigureBottleAnimator(
                destinationAnimator,
                destination.GetComponent<BottleLiquidGraphic>());

            streamObject = new GameObject(
                "Pour Stream",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PourStreamGraphic),
                typeof(PourStreamView));
            streamObject.transform.SetParent(root, false);
            PourStreamView streamView =
                streamObject.GetComponent<PourStreamView>();
            SerializedObject serializedStreamView =
                new SerializedObject(streamView);
            PourStreamGraphic streamGraphic =
                streamObject.GetComponent<PourStreamGraphic>();
            SerializedObject serializedStreamGraphic =
                new SerializedObject(streamGraphic);
            serializedStreamGraphic.FindProperty("segmentCount").intValue = 8;
            serializedStreamGraphic.FindProperty("destinationWidthRatio")
                .floatValue = 0.5f;
            serializedStreamGraphic.FindProperty(
                    "sourceTangentLengthRatio")
                .floatValue = 0.3f;
            serializedStreamGraphic.FindProperty(
                    "destinationTangentLengthRatio")
                .floatValue = 0.2f;
            serializedStreamGraphic.FindProperty(
                    "lateralWaveAmplitudeRatio")
                .floatValue = 0.65f;
            serializedStreamGraphic.FindProperty("lateralWaveCycleCount")
                .floatValue = 1.25f;
            serializedStreamGraphic.FindProperty("widthVariationRatio")
                .floatValue = 0.16f;
            serializedStreamGraphic.ApplyModifiedPropertiesWithoutUndo();
            serializedStreamView.FindProperty("streamGraphic")
                .objectReferenceValue = streamGraphic;
            serializedStreamView.ApplyModifiedPropertiesWithoutUndo();
            streamObject.SetActive(false);

            animator = bottleParentObject.GetComponent<LiquidTransferAnimator>();
            ConfigureAnimator(animator, streamView);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.Kill(bottleParentObject);
            UnityEngine.Object.DestroyImmediate(rootObject);
        }

        [Test]
        public void Play_CompletesCallbacksAndRestoresSource()
        {
            Vector2 restingPosition = source.anchoredPosition;
            int siblingIndex = source.GetSiblingIndex();
            bool pourStarted = false;
            bool liquidTransferred = false;
            bool completed = false;
            float lastSourceProgress = 0f;
            float lastDestinationProgress = 0f;

            animator.Play(
                CreateRequest(restingPosition, 4, 2),
                () => pourStarted = true,
                (sourceProgress, destinationProgress) =>
                {
                    lastSourceProgress = sourceProgress;
                    lastDestinationProgress = destinationProgress;
                },
                null,
                () => liquidTransferred = true,
                () => completed = true);

            DOTween.Complete(bottleParentObject, true);

            Assert.That(pourStarted, Is.True);
            Assert.That(lastSourceProgress, Is.EqualTo(1f));
            Assert.That(lastDestinationProgress, Is.EqualTo(1f));
            Assert.That(liquidTransferred, Is.True);
            Assert.That(completed, Is.True);
            AssertPositionsAreEqual(source.anchoredPosition, restingPosition);
            Assert.That(
                Mathf.DeltaAngle(0f, source.localEulerAngles.z),
                Is.Zero.Within(0.001f));
            Assert.That(source.GetSiblingIndex(), Is.EqualTo(siblingIndex));
            Assert.That(streamObject.activeSelf, Is.False);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void Play_UsesPouredLiquidCountForDuration(int pouredLiquidCount)
        {
            animator.Play(
                CreateRequest(
                    source.anchoredPosition,
                    4,
                    pouredLiquidCount),
                null,
                null,
                null,
                null,
                null);

            List<Tween> tweens = DOTween.TweensByTarget(bottleParentObject);
            float expectedDuration = TravelDuration +
                StreamEnterDuration +
                TransferDurationPerLiquid * pouredLiquidCount +
                StreamExitDuration +
                ReturnDuration;

            Assert.That(tweens, Has.Count.EqualTo(1));
            Assert.That(
                tweens[0].Duration(),
                Is.EqualTo(expectedDuration).Within(0.001f));
        }

        [Test]
        public void Play_UsesLiquidVolumeAndBottleGeometryForAngles()
        {
            float angleWhenPourStarted = 0f;
            float angleWhenTransferFinished = 0f;

            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 3),
                () => angleWhenPourStarted =
                    Mathf.DeltaAngle(0f, source.localEulerAngles.z),
                null,
                null,
                () => angleWhenTransferFinished =
                    Mathf.DeltaAngle(0f, source.localEulerAngles.z),
                null);

            DOTween.Complete(bottleParentObject, true);

            float expectedStartAngle = Mathf.Atan(0.4f) * Mathf.Rad2Deg;
            float expectedEndAngle = Mathf.Atan(4.4f) * Mathf.Rad2Deg;
            Assert.That(
                angleWhenPourStarted,
                Is.EqualTo(-expectedStartAngle).Within(0.001f));
            Assert.That(
                angleWhenTransferFinished,
                Is.EqualTo(-expectedEndAngle).Within(0.001f));
        }

        [Test]
        public void Play_UpdatesAngleWhileLiquidVolumeChanges()
        {
            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 2),
                null,
                null,
                null,
                null,
                null);

            float pourDuration = StreamEnterDuration +
                TransferDurationPerLiquid * 2 +
                StreamExitDuration;
            DOTween.Goto(bottleParentObject, TravelDuration);
            float startAngle = Mathf.Abs(Mathf.DeltaAngle(
                0f,
                source.localEulerAngles.z));

            DOTween.Goto(
                bottleParentObject,
                TravelDuration + pourDuration * 0.5f);
            float middleAngle = Mathf.Abs(Mathf.DeltaAngle(
                0f,
                source.localEulerAngles.z));

            DOTween.Goto(
                bottleParentObject,
                TravelDuration + pourDuration - TimelineBoundaryOffset);
            float endAngle = Mathf.Abs(Mathf.DeltaAngle(
                0f,
                source.localEulerAngles.z));

            Assert.That(middleAngle, Is.GreaterThan(startAngle));
            Assert.That(middleAngle, Is.LessThan(endAngle));
        }

        [Test]
        public void Play_DelaysDestinationUntilStreamArrives()
        {
            float sourceProgress = 0f;
            float destinationProgress = 0f;

            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 1),
                null,
                (currentSourceProgress, currentDestinationProgress) =>
                {
                    sourceProgress = currentSourceProgress;
                    destinationProgress = currentDestinationProgress;
                },
                null,
                null,
                null);

            DOTween.Goto(
                bottleParentObject,
                TravelDuration + StreamEnterDuration * 0.5f);

            Assert.That(sourceProgress, Is.GreaterThan(0f));
            Assert.That(destinationProgress, Is.Zero);
        }

        [Test]
        public void Play_WhenStreamStarts_UsesExtendedSourceLiquidBounds()
        {
            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 1),
                null,
                null,
                null,
                null,
                null);
            DOTween.Goto(
                bottleParentObject,
                TravelDuration + TimelineBoundaryOffset);

            PourStreamGraphic streamGraphic =
                streamObject.GetComponent<PourStreamGraphic>();
            streamGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                streamGraphic.canvasRenderer.GetMesh());

            try
            {
                Vector3[] vertices = mesh.vertices;
                Vector3 streamStart =
                    (vertices[0] + vertices[1]) * 0.5f;
                Vector3 streamStartWorld =
                    streamGraphic.rectTransform.TransformPoint(streamStart);
                Vector2 sourceLocalStreamStart =
                    source.InverseTransformPoint(streamStartWorld);

                Assert.That(
                    sourceLocalStreamStart.y,
                    Is.GreaterThan(LiquidBounds.yMax));
                Assert.That(
                    sourceLocalStreamStart.y,
                    Is.LessThan(source.rect.yMax));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Play_WhenStreamReachesDestination_NotifiesImpactOnce()
        {
            int impactCount = 0;

            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 1),
                null,
                null,
                () => impactCount++,
                null,
                null);

            DOTween.Complete(bottleParentObject, true);

            Assert.That(impactCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_ExtendsStreamInsideSourceAndDestination()
        {
            animator.Play(
                CreateRequest(source.anchoredPosition, 2, 1),
                null,
                null,
                null,
                null,
                null);
            float transferDuration = TransferDurationPerLiquid;
            float elapsedPourTime = StreamEnterDuration +
                transferDuration * 0.5f;
            DOTween.Goto(
                bottleParentObject,
                TravelDuration + elapsedPourTime);

            PourStreamGraphic streamGraphic =
                streamObject.GetComponent<PourStreamGraphic>();
            streamGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                streamGraphic.canvasRenderer.GetMesh());

            try
            {
                Vector3[] vertices = mesh.vertices;
                Vector3 streamStart = (vertices[0] + vertices[1]) * 0.5f;
                int streamEndIndex = vertices.Length - 2;
                Vector3 streamEnd =
                    (vertices[streamEndIndex] + vertices[streamEndIndex + 1]) *
                    0.5f;
                RectTransform streamSpace = streamGraphic.rectTransform;
                Vector3 streamStartWorld =
                    streamSpace.TransformPoint(streamStart);
                Vector2 sourceLocalStreamStart =
                    source.InverseTransformPoint(streamStartWorld);
                float destinationProgress =
                    transferDuration * 0.5f /
                    (transferDuration + StreamExitDuration);
                float destinationFillRatio = destinationProgress / 4f;
                Vector3 expectedDestinationWorld = destination.TransformPoint(
                    new Vector2(
                        LiquidBounds.center.x,
                        Mathf.Lerp(
                            LiquidBounds.yMin,
                            LiquidBounds.yMax,
                            destinationFillRatio)));
                Vector3 expectedStreamEnd =
                    streamSpace.InverseTransformPoint(expectedDestinationWorld);

                Assert.That(
                    sourceLocalStreamStart.x,
                    Is.EqualTo(20f).Within(0.001f));
                Assert.That(
                    sourceLocalStreamStart.y,
                    Is.InRange(LiquidBounds.yMin, LiquidBounds.yMax));
                Assert.That(
                    sourceLocalStreamStart.y,
                    Is.Not.EqualTo(LiquidBounds.yMax).Within(0.001f));
                Assert.That(streamEnd.x, Is.EqualTo(expectedStreamEnd.x).Within(0.001f));
                Assert.That(streamEnd.y, Is.EqualTo(expectedStreamEnd.y).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [TestCase(-200f, 0f, 200f, 1f)]
        [TestCase(-200f, 200f, 0f, 1f)]
        [TestCase(200f, 0f, 200f, -1f)]
        [TestCase(200f, 200f, 0f, -1f)]
        public void Play_WhenBottleBoundsOverlap_KeepsBodyTowardParentCenter(
            float columnPosition,
            float sourcePositionY,
            float destinationPositionY,
            float expectedAngleSign)
        {
            source.anchoredPosition = new Vector2(
                columnPosition,
                sourcePositionY);
            destination.anchoredPosition = new Vector2(
                columnPosition,
                destinationPositionY);
            float angleWhenPourStarted = 0f;

            animator.Play(
                CreateRequest(source.anchoredPosition, 4, 1),
                () => angleWhenPourStarted = Mathf.DeltaAngle(
                    0f,
                    source.localEulerAngles.z),
                null,
                null,
                null,
                null);

            DOTween.Complete(bottleParentObject, true);

            Assert.That(
                Mathf.Sign(angleWhenPourStarted),
                Is.EqualTo(expectedAngleSign));
        }

        [Test]
        public void Disable_DuringAnimationRestoresSourceAndNotifiesCancellation()
        {
            Vector2 restingPosition = source.anchoredPosition;
            int siblingIndex = source.GetSiblingIndex();
            int cancellationCount = 0;

            animator.Play(
                CreateRequest(restingPosition, 4, 2),
                null,
                null,
                null,
                null,
                null,
                () => cancellationCount++);
            DOTween.Goto(bottleParentObject, TravelDuration * 0.5f);

            InvokeOnDisable(animator);

            AssertPositionsAreEqual(source.anchoredPosition, restingPosition);
            Assert.That(
                Mathf.DeltaAngle(0f, source.localEulerAngles.z),
                Is.Zero.Within(0.001f));
            Assert.That(source.GetSiblingIndex(), Is.EqualTo(siblingIndex));
            Assert.That(streamObject.activeSelf, Is.False);
            Assert.That(cancellationCount, Is.EqualTo(1));

            bottleParentObject.SetActive(true);
            Assert.DoesNotThrow(() => animator.Play(
                CreateRequest(restingPosition, 4, 1),
                null,
                null,
                null,
                null,
                null));
        }

        [Test]
        public void Play_WithNonRectTransformParentThrowsWithoutMovingSource()
        {
            GameObject invalidParent = new GameObject("Invalid Parent");
            invalidParent.transform.SetParent(rootObject.transform, false);
            source.SetParent(invalidParent.transform, false);
            destination.SetParent(invalidParent.transform, false);
            Vector2 restingPosition = source.anchoredPosition;
            int siblingIndex = source.GetSiblingIndex();

            Assert.Throws<ArgumentException>(() => animator.Play(
                CreateRequest(restingPosition, 4, 1),
                null,
                null,
                null,
                null,
                null));

            AssertPositionsAreEqual(source.anchoredPosition, restingPosition);
            Assert.That(source.GetSiblingIndex(), Is.EqualTo(siblingIndex));
            Assert.That(streamObject.activeSelf, Is.False);
        }

        [Test]
        public void Play_WhenSourceIsAlreadyPouringDoesNotCancelExistingPour()
        {
            Vector2 restingPosition = source.anchoredPosition;
            sourceAnimator.Begin(restingPosition);

            Assert.Throws<InvalidOperationException>(() => animator.Play(
                CreateRequest(restingPosition, 4, 1),
                null,
                null,
                null,
                null,
                null));
            Assert.Throws<InvalidOperationException>(() =>
                sourceAnimator.Begin(restingPosition));

            sourceAnimator.Cancel();
        }

        [Test]
        public void Play_WithInvalidPouredLiquidCountThrows()
        {
            LiquidTransferAnimationRequest request = CreateRequest(
                source.anchoredPosition,
                2,
                3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                animator.Play(request, null, null, null, null, null));
        }

        private static void AssertPositionsAreEqual(
            Vector2 actual,
            Vector2 expected)
        {
            Assert.That(
                actual.x,
                Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(
                actual.y,
                Is.EqualTo(expected.y).Within(0.001f));
        }

        private static void InvokeOnDisable(LiquidTransferAnimator target)
        {
            MethodInfo onDisable = typeof(LiquidTransferAnimator).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(onDisable, Is.Not.Null);
            onDisable.Invoke(target, null);
        }

        private LiquidTransferAnimationRequest CreateRequest(
            Vector2 restingPosition,
            int sourceLiquidCount,
            int pouredLiquidCount)
        {
            return new LiquidTransferAnimationRequest(
                sourceAnimator,
                destinationAnimator,
                restingPosition,
                LiquidBounds,
                LiquidBounds,
                4,
                sourceLiquidCount,
                0,
                pouredLiquidCount,
                Color.cyan);
        }

        private static RectTransform CreateBottle(
            string name,
            RectTransform parent,
            float anchoredPositionX)
        {
            GameObject bottleObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(BottleLiquidGraphic),
                typeof(BottlePourAnimator));
            RectTransform bottle = bottleObject.GetComponent<RectTransform>();
            bottle.SetParent(parent, false);
            bottle.sizeDelta = new Vector2(100f, 200f);
            bottle.anchoredPosition = new Vector2(anchoredPositionX, 0f);

            GameObject mouthObject = new GameObject(
                "Mouth Opening",
                typeof(RectTransform));
            RectTransform mouthOpening =
                mouthObject.GetComponent<RectTransform>();
            mouthOpening.SetParent(bottle, false);
            mouthOpening.anchorMin = new Vector2(0.5f, 1f);
            mouthOpening.anchorMax = new Vector2(0.5f, 1f);
            mouthOpening.sizeDelta = new Vector2(40f, 0f);
            mouthOpening.anchoredPosition = Vector2.zero;
            return bottle;
        }

        private static void ConfigureAnimator(
            LiquidTransferAnimator transferAnimator,
            PourStreamView streamView)
        {
            SerializedObject serializedAnimator =
                new SerializedObject(transferAnimator);
            serializedAnimator.FindProperty("mouthVerticalGapRatio")
                .floatValue = 0.2f;
            serializedAnimator.FindProperty("mouthHorizontalGapRatio")
                .floatValue = 0.2f;
            serializedAnimator.FindProperty("streamEnterDuration").floatValue =
                StreamEnterDuration;
            serializedAnimator.FindProperty("transferDurationPerLiquid")
                .floatValue = TransferDurationPerLiquid;
            serializedAnimator.FindProperty("streamExitDuration").floatValue =
                StreamExitDuration;
            serializedAnimator.FindProperty("streamWidthRatio").floatValue = 0.1f;
            serializedAnimator.FindProperty("streamFlowCyclesPerSecond")
                .floatValue = 2.2f;
            serializedAnimator.FindProperty("pourEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("streamView").objectReferenceValue =
                streamView;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBottleAnimator(
            BottlePourAnimator pourAnimator,
            BottleLiquidGraphic liquidGraphic)
        {
            SerializedObject serializedLiquidGraphic =
                new SerializedObject(liquidGraphic);
            serializedLiquidGraphic.FindProperty("surfaceAngleMultiplier")
                .floatValue = 1f;
            serializedLiquidGraphic.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedAnimator =
                new SerializedObject(pourAnimator);
            serializedAnimator.FindProperty("liquidGraphic")
                .objectReferenceValue = liquidGraphic;
            serializedAnimator.FindProperty("mouthOpening")
                .objectReferenceValue =
                    pourAnimator.transform
                        .Find("Mouth Opening")
                        .GetComponent<RectTransform>();
            serializedAnimator.FindProperty("travelDuration").floatValue =
                TravelDuration;
            serializedAnimator.FindProperty("travelArcHeightRatio").floatValue =
                0.25f;
            serializedAnimator.FindProperty("travelEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("returnDuration").floatValue =
                ReturnDuration;
            serializedAnimator.FindProperty("returnArcHeightRatio").floatValue =
                0.1f;
            serializedAnimator.FindProperty("returnEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
