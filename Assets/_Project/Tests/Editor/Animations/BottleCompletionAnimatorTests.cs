using Coffee.UIExtensions;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class BottleCompletionAnimatorTests
    {
        private GameObject bottleObject;
        private RectTransform capTransform;
        private UIParticle completionSparkle;
        private BottleCompletionAnimator animator;

        [SetUp]
        public void SetUp()
        {
            bottleObject = new GameObject("Bottle", typeof(RectTransform));
            BottleCapAnimator capAnimator = CreateCapAnimator();
            completionSparkle = CreateCompletionSparkle();
            animator = bottleObject.AddComponent<BottleCompletionAnimator>();

            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("capAnimator")
                .objectReferenceValue = capAnimator;
            serializedAnimator.FindProperty("completionSparkle")
                .objectReferenceValue = completionSparkle;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();

            animator.Hide();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(bottleObject);
        }

        [Test]
        public void PlayCompletion_AfterCapCloses_PlaysCompletionSparkle()
        {
            animator.PlayCompletion();

            Assert.That(completionSparkle.isPaused, Is.True);

            DOTween.Complete(capTransform);

            Assert.That(completionSparkle.isPaused, Is.False);
        }

        [Test]
        public void Hide_DuringCompletion_HidesCapAndStopsCompletionSparkle()
        {
            animator.PlayCompletion();
            DOTween.Complete(capTransform);

            animator.Hide();

            Assert.That(capTransform.gameObject.activeSelf, Is.False);
            Assert.That(completionSparkle.isPaused, Is.True);
        }

        private BottleCapAnimator CreateCapAnimator()
        {
            GameObject capObject = new GameObject(
                "Cap Visual",
                typeof(RectTransform));
            capTransform = capObject.GetComponent<RectTransform>();
            capTransform.SetParent(bottleObject.transform, false);
            capTransform.sizeDelta = new Vector2(50f, 20f);

            BottleCapAnimator capAnimator =
                bottleObject.AddComponent<BottleCapAnimator>();
            SerializedObject serializedCapAnimator =
                new SerializedObject(capAnimator);
            serializedCapAnimator.FindProperty("capVisual")
                .objectReferenceValue = capTransform;
            serializedCapAnimator.FindProperty("closingStartOffsetRatio")
                .floatValue = 1f;
            serializedCapAnimator.FindProperty("closingDuration")
                .floatValue = 1f;
            serializedCapAnimator.ApplyModifiedPropertiesWithoutUndo();

            return capAnimator;
        }

        private UIParticle CreateCompletionSparkle()
        {
            GameObject effectObject = new GameObject(
                "CompletionSparkle",
                typeof(RectTransform),
                typeof(UIParticle));
            effectObject.transform.SetParent(bottleObject.transform, false);
            return effectObject.GetComponent<UIParticle>();
        }
    }
}
