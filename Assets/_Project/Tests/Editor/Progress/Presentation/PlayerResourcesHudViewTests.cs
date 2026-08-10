using System;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Progress.Presentation
{
    public sealed class PlayerResourcesHudViewTests
    {
        private GameObject viewObject;
        private GameObject goldTextObject;
        private GameObject lifeCountTextObject;
        private GameObject lifeTimeTextObject;
        private TMP_Text goldText;
        private TMP_Text lifeCountText;
        private TMP_Text lifeTimeText;
        private PlayerResourcesHudView view;

        [SetUp]
        public void SetUp()
        {
            viewObject = new GameObject("PlayerResourcesHud");
            view = viewObject.AddComponent<PlayerResourcesHudView>();
            goldTextObject = CreateTextObject("GoldText", out goldText);
            lifeCountTextObject = CreateTextObject(
                "LifeCountText",
                out lifeCountText);
            lifeTimeTextObject = CreateTextObject(
                "LifeTimeText",
                out lifeTimeText);

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("goldText").objectReferenceValue =
                goldText;
            serializedView.FindProperty("lifeCountText")
                .objectReferenceValue = lifeCountText;
            serializedView.FindProperty("lifeTimeText")
                .objectReferenceValue = lifeTimeText;
            serializedView.FindProperty("fullLivesText").stringValue = "FULL";
            serializedView.FindProperty("maximumLives").intValue = 5;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(viewObject);
            UnityEngine.Object.DestroyImmediate(goldTextObject);
            UnityEngine.Object.DestroyImmediate(lifeCountTextObject);
            UnityEngine.Object.DestroyImmediate(lifeTimeTextObject);
        }

        [Test]
        public void ShowGold_UpdatesGoldText()
        {
            view.ShowGold(150);

            Assert.That(goldText.text, Is.EqualTo("150"));
        }

        [Test]
        public void ShowLives_WithMaximumLives_ShowsCountAndFullText()
        {
            view.ShowLives(5, 0);

            Assert.That(lifeCountText.text, Is.EqualTo("5"));
            Assert.That(lifeTimeText.text, Is.EqualTo("FULL"));
        }

        [Test]
        public void ShowLives_WhileRefilling_ShowsCountAndRemainingTime()
        {
            view.ShowLives(3, 249);

            Assert.That(lifeCountText.text, Is.EqualTo("3"));
            Assert.That(lifeTimeText.text, Is.EqualTo("04:09"));
        }

        [Test]
        public void ShowLives_UsesConfiguredMaximumLives()
        {
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("maximumLives").intValue = 3;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            view.ShowLives(3, 0);

            Assert.That(lifeCountText.text, Is.EqualTo("3"));
            Assert.That(lifeTimeText.text, Is.EqualTo("FULL"));
        }

        [Test]
        public void ShowValues_WithNegativeValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => view.ShowGold(-1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => view.ShowLives(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => view.ShowLives(6, 0));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => view.ShowLives(1, -1));
        }

        private static GameObject CreateTextObject(
            string name,
            out TMP_Text text)
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer));
            text = textObject.AddComponent<TextMeshProUGUI>();
            return textObject;
        }
    }
}
