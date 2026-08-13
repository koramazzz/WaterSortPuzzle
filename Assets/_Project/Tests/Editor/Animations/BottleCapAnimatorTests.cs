using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class BottleCapAnimatorTests
    {
        private GameObject bottleObject;
        private RectTransform capTransform;
        private BottleCapAnimator animator;

        [SetUp]
        public void SetUp()
        {
            bottleObject = new GameObject("Bottle", typeof(RectTransform));
            GameObject capObject = new GameObject(
                "Cap Visual",
                typeof(RectTransform));
            capTransform = capObject.GetComponent<RectTransform>();
            capTransform.SetParent(bottleObject.transform, false);
            capTransform.sizeDelta = new Vector2(50f, 20f);
            capTransform.anchoredPosition = new Vector2(5f, 10f);
            capObject.SetActive(false);

            animator = bottleObject.AddComponent<BottleCapAnimator>();
            SerializedObject serializedAnimator =
                new SerializedObject(animator);
            serializedAnimator.FindProperty("capVisual")
                .objectReferenceValue = capTransform;
            serializedAnimator.FindProperty("closingStartOffsetRatio")
                .floatValue = 1f;
            serializedAnimator.FindProperty("closingDuration")
                .floatValue = 1f;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(bottleObject);
        }

        [Test]
        public void PlayClosing_MovesCapFromOffsetToRestingPosition()
        {
            animator.PlayClosing();

            Assert.That(capTransform.gameObject.activeSelf, Is.True);
            Assert.That(
                capTransform.anchoredPosition,
                Is.EqualTo(new Vector2(5f, 30f)));

            DOTween.Complete(capTransform);

            Assert.That(
                capTransform.anchoredPosition,
                Is.EqualTo(new Vector2(5f, 10f)));
        }

        [Test]
        public void Hide_DuringClosing_HidesCapAtRestingPosition()
        {
            animator.PlayClosing();

            animator.Hide();

            Assert.That(capTransform.gameObject.activeSelf, Is.False);
            Assert.That(
                capTransform.anchoredPosition,
                Is.EqualTo(new Vector2(5f, 10f)));
            Assert.That(DOTween.IsTweening(capTransform), Is.False);
        }
    }
}
