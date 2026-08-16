using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation.Layout
{
    public sealed class BottleGridLayoutTests
    {
        private GameObject containerObject;
        private RectTransform container;
        private BottleGridLayout layout;

        [SetUp]
        public void SetUp()
        {
            containerObject = new GameObject(
                "Bottle Container",
                typeof(RectTransform),
                typeof(BottleGridLayout));
            container = containerObject.GetComponent<RectTransform>();
            container.sizeDelta = new Vector2(600f, 600f);
            layout = containerObject.GetComponent<BottleGridLayout>();

            SerializedObject serializedLayout = new SerializedObject(layout);
            serializedLayout.FindProperty("maximumColumnCount").intValue = 4;
            serializedLayout.FindProperty("bottleAspectRatio").floatValue = 0.5f;
            serializedLayout.FindProperty("spacing").vector2Value =
                new Vector2(20f, 20f);
            serializedLayout.ApplyModifiedPropertiesWithoutUndo();

            for (int index = 0; index < 5; index++)
            {
                CreateBottle($"Bottle {index}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(containerObject);
        }

        [Test]
        public void Arrange_WithIncompleteFinalRow_SizesAndCentersBottles()
        {
            layout.Arrange(5);

            AssertBottle(0, new Vector2(-165f, 155f));
            AssertBottle(1, new Vector2(0f, 155f));
            AssertBottle(2, new Vector2(165f, 155f));
            AssertBottle(3, new Vector2(-82.5f, -155f));
            AssertBottle(4, new Vector2(82.5f, -155f));
        }

        private void AssertBottle(int index, Vector2 expectedPosition)
        {
            RectTransform bottle =
                (RectTransform)container.GetChild(index);

            Assert.That(
                bottle.sizeDelta.x,
                Is.EqualTo(145f).Within(0.001f));
            Assert.That(
                bottle.sizeDelta.y,
                Is.EqualTo(290f).Within(0.001f));
            Assert.That(
                bottle.anchoredPosition.x,
                Is.EqualTo(expectedPosition.x).Within(0.001f));
            Assert.That(
                bottle.anchoredPosition.y,
                Is.EqualTo(expectedPosition.y).Within(0.001f));
        }

        private RectTransform CreateBottle(string name)
        {
            GameObject bottle = new GameObject(
                name,
                typeof(RectTransform));
            bottle.transform.SetParent(container, false);
            return bottle.GetComponent<RectTransform>();
        }
    }
}
