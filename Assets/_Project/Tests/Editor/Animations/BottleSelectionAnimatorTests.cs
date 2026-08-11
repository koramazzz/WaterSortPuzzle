using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class BottleSelectionAnimatorTests
    {
        private GameObject bottleObject;
        private RectTransform bottleTransform;
        private BottleSelectionAnimator animator;

        [SetUp]
        public void SetUp()
        {
            bottleObject = new GameObject(
                "Bottle",
                typeof(RectTransform),
                typeof(BottleSelectionAnimator));
            bottleTransform = bottleObject.GetComponent<RectTransform>();
            animator = bottleObject.GetComponent<BottleSelectionAnimator>();
            bottleTransform.sizeDelta = new Vector2(100f, 200f);
            bottleTransform.anchoredPosition = new Vector2(10f, 20f);

            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("selectionLiftRatio").floatValue =
                0.25f;
            serializedAnimator.FindProperty("selectionDuration").floatValue = 1f;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(bottleObject);
        }

        [Test]
        public void PlaySelection_AndDeselection_MovesBetweenPositions()
        {
            animator.PlaySelection();
            DOTween.Complete(bottleTransform);

            Assert.That(
                bottleTransform.anchoredPosition.y,
                Is.EqualTo(70f).Within(0.001f));

            animator.PlayDeselection();
            DOTween.Complete(bottleTransform);

            Assert.That(
                bottleTransform.anchoredPosition.y,
                Is.EqualTo(20f).Within(0.001f));
        }
    }
}
