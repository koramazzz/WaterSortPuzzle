using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class LiquidSlotViewTests
    {
        private GameObject root;
        private Image liquidImage;
        private GameObject hiddenVisual;
        private LiquidSlotView view;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Liquid Slot");
            GameObject liquidImageObject = new GameObject(
                "Liquid Image",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            liquidImage = liquidImageObject.GetComponent<Image>();
            hiddenVisual = new GameObject("Hidden Visual");
            liquidImage.transform.SetParent(root.transform);
            hiddenVisual.transform.SetParent(root.transform);
            view = root.AddComponent<LiquidSlotView>();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("liquidImage").objectReferenceValue =
                liquidImage;
            serializedView.FindProperty("hiddenVisual").objectReferenceValue =
                hiddenVisual;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ShowLiquid_WithVisibleLiquid_DisplaysConfiguredColor()
        {
            view.ShowLiquid(Color.cyan, false);

            Assert.That(liquidImage.color, Is.EqualTo(Color.cyan));
            Assert.That(liquidImage.enabled, Is.True);
            Assert.That(hiddenVisual.activeSelf, Is.False);
        }

        [Test]
        public void ShowLiquid_WithHiddenLiquid_DisplaysHiddenVisual()
        {
            view.ShowLiquid(Color.cyan, true);

            Assert.That(liquidImage.enabled, Is.False);
            Assert.That(hiddenVisual.activeSelf, Is.True);
        }

        [Test]
        public void ShowEmpty_HidesLiquidAndHiddenVisual()
        {
            view.ShowLiquid(Color.cyan, true);

            view.ShowEmpty();

            Assert.That(liquidImage.enabled, Is.False);
            Assert.That(hiddenVisual.activeSelf, Is.False);
        }
    }
}
