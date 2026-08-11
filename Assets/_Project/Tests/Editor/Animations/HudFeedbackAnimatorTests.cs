using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class HudFeedbackAnimatorTests
    {
        private GameObject hudObject;
        private RectTransform goldHudTransform;
        private RectTransform lifeHudTransform;
        private HudFeedbackAnimator animator;

        [SetUp]
        public void SetUp()
        {
            CreateAnimator(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(hudObject);
        }

        [Test]
        public void PlayChanged_AfterCompletion_RestoresOriginalScale()
        {
            animator.PlayChanged(PlayerResourceType.Gold);
            DOTween.Complete(goldHudTransform);

            Assert.That(
                goldHudTransform.localScale,
                Is.EqualTo(new Vector3(2f, 2f, 1f)));
            Assert.That(
                lifeHudTransform.localScale,
                Is.EqualTo(new Vector3(3f, 3f, 1f)));
        }

        [Test]
        public void PlayInsufficient_AfterCompletion_RestoresOriginalScale()
        {
            animator.PlayInsufficient(PlayerResourceType.Life);
            DOTween.Complete(lifeHudTransform);

            Assert.That(
                lifeHudTransform.localScale,
                Is.EqualTo(new Vector3(3f, 3f, 1f)));
            Assert.That(
                goldHudTransform.localScale,
                Is.EqualTo(new Vector3(2f, 2f, 1f)));
        }

        [Test]
        public void PlayFeedback_WhenRepeated_RestartsFromOriginalScale()
        {
            animator.PlayChanged(PlayerResourceType.Gold);
            goldHudTransform.localScale = Vector3.one;

            animator.PlayInsufficient(PlayerResourceType.Life);

            Assert.That(
                goldHudTransform.localScale,
                Is.EqualTo(new Vector3(2f, 2f, 1f)));
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.True);
        }

        [Test]
        public void PlayChanged_WhenHudStartsInactive_RestoresOriginalScale()
        {
            Object.DestroyImmediate(hudObject);
            CreateAnimator(false);

            animator.PlayChanged(PlayerResourceType.Gold);
            DOTween.Complete(goldHudTransform);

            Assert.That(
                goldHudTransform.localScale,
                Is.EqualTo(new Vector3(2f, 2f, 1f)));
        }

        private void CreateAnimator(bool isActive)
        {
            hudObject = new GameObject(
                "Hud",
                typeof(RectTransform));
            hudObject.SetActive(isActive);
            goldHudTransform = CreateHud(
                "GoldHud",
                new Vector3(2f, 2f, 1f));
            lifeHudTransform = CreateHud(
                "LifeHud",
                new Vector3(3f, 3f, 1f));
            animator = hudObject.AddComponent<HudFeedbackAnimator>();

            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("goldHud").objectReferenceValue =
                goldHudTransform;
            serializedAnimator.FindProperty("lifeHud").objectReferenceValue =
                lifeHudTransform;
            serializedAnimator.FindProperty("changedScaleMultiplier").floatValue =
                1.25f;
            serializedAnimator.FindProperty("changedPhaseDuration").floatValue =
                1f;
            serializedAnimator.FindProperty("insufficientScaleMultiplier")
                .floatValue = 1.5f;
            serializedAnimator.FindProperty("insufficientPhaseDuration")
                .floatValue = 1f;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        private RectTransform CreateHud(
            string name,
            Vector3 scale)
        {
            GameObject targetObject = new GameObject(
                name,
                typeof(RectTransform));
            RectTransform targetTransform =
                targetObject.GetComponent<RectTransform>();
            targetTransform.SetParent(hudObject.transform, false);
            targetTransform.localScale = scale;
            return targetTransform;
        }
    }
}
