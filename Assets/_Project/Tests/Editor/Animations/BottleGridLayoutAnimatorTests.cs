using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Tests.EditMode.Animations
{
    public sealed class BottleGridLayoutAnimatorTests
    {
        private static readonly Vector2 BottleSize =
            new Vector2(145f, 290f);

        private GameObject containerObject;
        private RectTransform container;
        private BottleGridLayoutAnimator animator;
        private readonly List<RectTransform> bottles =
            new List<RectTransform>();

        [SetUp]
        public void SetUp()
        {
            containerObject = new GameObject(
                "Bottle Container",
                typeof(RectTransform),
                typeof(BottleGridLayoutAnimator));
            container = containerObject.GetComponent<RectTransform>();
            animator = containerObject.GetComponent<BottleGridLayoutAnimator>();

            SerializedObject serializedAnimator =
                new SerializedObject(animator);
            serializedAnimator.FindProperty("additionDuration").floatValue = 1f;
            serializedAnimator.FindProperty("additionEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();

            CreateBottle(new Vector2(-165f, 155f));
            CreateBottle(new Vector2(0f, 155f));
            CreateBottle(new Vector2(165f, 155f));
            CreateBottle(new Vector2(-82.5f, -155f));
            CreateBottle(new Vector2(82.5f, -155f));
            CreateBottle(Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.Kill(containerObject);
            UnityEngine.Object.DestroyImmediate(containerObject);
        }

        [Test]
        public void Play_AnimatesOnlyBottlesWhoseLayoutChanged()
        {
            RectTransform addedBottle = bottles[5];

            animator.Play(
                container,
                BottleSize,
                CreateTargetPositions(),
                addedBottle);

            Assert.That(addedBottle.localScale, Is.EqualTo(Vector3.zero));
            Assert.That(DOTween.IsTweening(containerObject), Is.True);

            DOTween.Goto(containerObject, 0.5f);

            AssertBottle(0, new Vector2(-165f, 155f));
            AssertBottle(3, new Vector2(-123.75f, -155f));

            DOTween.Complete(containerObject);

            AssertBottle(0, new Vector2(-165f, 155f));
            AssertBottle(1, new Vector2(0f, 155f));
            AssertBottle(2, new Vector2(165f, 155f));
            AssertBottle(3, new Vector2(-165f, -155f));
            AssertBottle(4, new Vector2(0f, -155f));
            AssertBottle(5, new Vector2(165f, -155f));
            Assert.That(addedBottle.localScale, Is.EqualTo(Vector3.one));
        }

        private RectTransform CreateBottle(Vector2 anchoredPosition)
        {
            GameObject bottleObject = new GameObject(
                $"Bottle {bottles.Count}",
                typeof(RectTransform));
            RectTransform bottle =
                bottleObject.GetComponent<RectTransform>();
            bottle.SetParent(container, false);
            bottle.sizeDelta = BottleSize;
            bottle.anchoredPosition = anchoredPosition;
            bottles.Add(bottle);
            return bottle;
        }

        private static IReadOnlyList<Vector2> CreateTargetPositions()
        {
            return new[]
            {
                new Vector2(-165f, 155f),
                new Vector2(0f, 155f),
                new Vector2(165f, 155f),
                new Vector2(-165f, -155f),
                new Vector2(0f, -155f),
                new Vector2(165f, -155f)
            };
        }

        private void AssertBottle(int index, Vector2 expectedPosition)
        {
            RectTransform bottle = bottles[index];

            Assert.That(bottle.sizeDelta, Is.EqualTo(BottleSize));
            Assert.That(
                bottle.anchoredPosition.x,
                Is.EqualTo(expectedPosition.x).Within(0.001f));
            Assert.That(
                bottle.anchoredPosition.y,
                Is.EqualTo(expectedPosition.y).Within(0.001f));
        }
    }
}
