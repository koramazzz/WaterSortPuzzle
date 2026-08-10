using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.MainMenu.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.MainMenu.Presentation
{
    public sealed class LevelDifficultyBadgeViewTests
    {
        private GameObject badgeObject;
        private Texture2D badgeTexture;
        private Image badgeImage;
        private Sprite easyBadge;
        private Sprite mediumBadge;
        private Sprite hardBadge;
        private LevelDifficultyBadgeView view;

        [SetUp]
        public void SetUp()
        {
            badgeObject = new GameObject(
                "DifficultyBadge",
                typeof(RectTransform),
                typeof(CanvasRenderer));
            view = badgeObject.AddComponent<LevelDifficultyBadgeView>();
            badgeImage = badgeObject.AddComponent<Image>();
            badgeTexture = new Texture2D(1, 1);
            easyBadge = CreateSprite();
            mediumBadge = CreateSprite();
            hardBadge = CreateSprite();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("badgeImage").objectReferenceValue =
                badgeImage;
            serializedView.FindProperty("easyBadge").objectReferenceValue =
                easyBadge;
            serializedView.FindProperty("mediumBadge").objectReferenceValue =
                mediumBadge;
            serializedView.FindProperty("hardBadge").objectReferenceValue =
                hardBadge;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(badgeObject);
            Object.DestroyImmediate(easyBadge);
            Object.DestroyImmediate(mediumBadge);
            Object.DestroyImmediate(hardBadge);
            Object.DestroyImmediate(badgeTexture);
        }

        [TestCase(LevelDifficulty.Easy)]
        [TestCase(LevelDifficulty.Medium)]
        [TestCase(LevelDifficulty.Hard)]
        public void Show_WithDifficulty_UpdatesBadgeSprite(
            LevelDifficulty difficulty)
        {
            badgeObject.SetActive(false);

            view.Show(difficulty);

            Assert.That(badgeObject.activeSelf, Is.True);
            Assert.That(badgeImage.sprite, Is.SameAs(GetBadge(difficulty)));
        }

        [Test]
        public void Hide_DeactivatesBadge()
        {
            view.Hide();

            Assert.That(badgeObject.activeSelf, Is.False);
        }

        private Sprite CreateSprite()
        {
            return Sprite.Create(
                badgeTexture,
                new Rect(0, 0, 1, 1),
                Vector2.one * 0.5f);
        }

        private Sprite GetBadge(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => easyBadge,
                LevelDifficulty.Medium => mediumBadge,
                LevelDifficulty.Hard => hardBadge,
                _ => throw new AssertionException("Unexpected difficulty.")
            };
        }
    }
}
