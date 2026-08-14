using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Progress.Presentation
{
    public sealed class PlayerResourcesHudControllerTests
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";
        private const string LivesKey =
            "WaterSortPuzzle.Progress.Lives";
        private const string NextLifeTimestampKey =
            "WaterSortPuzzle.Progress.NextLifeTimestamp";

        private bool hadGold;
        private bool hadLives;
        private bool hadNextLifeTimestamp;
        private int savedGold;
        private int savedLives;
        private string savedNextLifeTimestamp;

        private GameObject controllerObject;
        private GameObject goldTextObject;
        private GameObject lifeCountTextObject;
        private GameObject lifeTimeTextObject;
        private TMP_Text goldText;
        private TMP_Text lifeCountText;
        private TMP_Text lifeTimeText;
        private RectTransform goldHudTransform;
        private RectTransform lifeHudTransform;
        private HudFeedbackAnimator hudAnimator;
        private PlayerResourcesHudController controller;

        [SetUp]
        public void SetUp()
        {
            SavePlayerPrefs();
            ClearPlayerPrefs();

            controllerObject = new GameObject("PlayerResourcesHud");
            controllerObject.SetActive(false);

            PlayerResourcesHudView view =
                controllerObject.AddComponent<PlayerResourcesHudView>();
            hudAnimator = controllerObject
                .AddComponent<HudFeedbackAnimator>();
            controller = controllerObject
                .AddComponent<PlayerResourcesHudController>();
            goldHudTransform = CreateHudObject("GoldHud");
            lifeHudTransform = CreateHudObject("LifeHud");
            ConfigureHudAnimator(
                hudAnimator,
                goldHudTransform,
                lifeHudTransform);

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
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedController =
                new SerializedObject(controller);
            serializedController.FindProperty("resourcesHudView")
                .objectReferenceValue = view;
            serializedController.FindProperty("hudAnimator")
                .objectReferenceValue = hudAnimator;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(goldTextObject);
            Object.DestroyImmediate(lifeCountTextObject);
            Object.DestroyImmediate(lifeTimeTextObject);

            RestorePlayerPrefs();
        }

        [Test]
        public void CheckLifeAvailability_WithLife_UpdatesDisplayedResources()
        {
            bool hasAvailableLife = controller.CheckLifeAvailability();

            Assert.That(hasAvailableLife, Is.True);
            Assert.That(
                goldText.text,
                Is.EqualTo(GameBalance.InitialGold.ToString()));
            Assert.That(
                lifeCountText.text,
                Is.EqualTo(GameBalance.MaximumLives.ToString()));
            Assert.That(lifeTimeText.text, Is.EqualTo("FULL"));
        }

        [Test]
        public void RewardGold_PersistsAndDisplaysUpdatedGold()
        {
            controller.RewardGold(50);

            int expectedGold = GameBalance.InitialGold + 50;

            Assert.That(goldText.text, Is.EqualTo(expectedGold.ToString()));
            Assert.That(
                LoadSavedResources().Gold,
                Is.EqualTo(expectedGold));
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.False);
        }

        [Test]
        public void TryConsumeLife_WithLife_PersistsAndDisplaysUpdatedLives()
        {
            bool consumed = controller.TryConsumeLife();

            Assert.That(consumed, Is.True);
            Assert.That(
                lifeCountText.text,
                Is.EqualTo((GameBalance.MaximumLives - 1).ToString()));
            Assert.That(
                lifeTimeText.text,
                Is.EqualTo(
                    $"{GameBalance.LifeRefillDurationSeconds / 60:00}:00"));
            Assert.That(
                LoadSavedResources().Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.False);
        }

        [Test]
        public void TryConsumeLife_WithoutLives_PlaysInsufficientFeedback()
        {
            PlayerPrefs.SetInt(LivesKey, GameBalance.MinimumLives);

            bool consumed = controller.TryConsumeLife();

            Assert.That(consumed, Is.False);
            Assert.That(
                lifeCountText.text,
                Is.EqualTo(GameBalance.MinimumLives.ToString()));
            Assert.That(
                LoadSavedResources().Lives,
                Is.EqualTo(GameBalance.MinimumLives));
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.False);
        }

        [Test]
        public void TrySpendGold_WithEnoughGold_PersistsAndDisplaysGold()
        {
            PlayerPrefs.SetInt(GoldKey, 100);

            bool spent = controller.TrySpendGold(60);

            Assert.That(spent, Is.True);
            Assert.That(goldText.text, Is.EqualTo("40"));
            Assert.That(LoadSavedResources().Gold, Is.EqualTo(40));
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.False);
        }

        [Test]
        public void TrySpendGold_WithoutEnoughGold_PlaysInsufficientFeedback()
        {
            PlayerPrefs.SetInt(GoldKey, 50);

            bool spent = controller.TrySpendGold(60);

            Assert.That(spent, Is.False);
            Assert.That(goldText.text, Is.EqualTo("50"));
            Assert.That(LoadSavedResources().Gold, Is.EqualTo(50));
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.False);
        }

        [Test]
        public void CheckLifeAvailability_WithoutLife_PlaysFeedback()
        {
            PlayerPrefs.SetInt(LivesKey, GameBalance.MinimumLives);

            bool hasAvailableLife = controller.CheckLifeAvailability();

            Assert.That(hasAvailableLife, Is.False);
            Assert.That(
                lifeCountText.text,
                Is.EqualTo(GameBalance.MinimumLives.ToString()));
            Assert.That(DOTween.IsTweening(lifeHudTransform), Is.True);
            Assert.That(DOTween.IsTweening(goldHudTransform), Is.False);
        }

        private void SavePlayerPrefs()
        {
            hadGold = PlayerPrefs.HasKey(GoldKey);
            hadLives = PlayerPrefs.HasKey(LivesKey);
            hadNextLifeTimestamp = PlayerPrefs.HasKey(
                NextLifeTimestampKey);
            savedGold = PlayerPrefs.GetInt(GoldKey);
            savedLives = PlayerPrefs.GetInt(LivesKey);
            savedNextLifeTimestamp = PlayerPrefs.GetString(
                NextLifeTimestampKey);
        }

        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(GoldKey);
            PlayerPrefs.DeleteKey(LivesKey);
            PlayerPrefs.DeleteKey(NextLifeTimestampKey);
        }

        private static PlayerResources LoadSavedResources()
        {
            return new PlayerResourcesService(
                    new PlayerPrefsPlayerResourcesStore())
                .Load();
        }

        private void RestorePlayerPrefs()
        {
            RestoreInt(GoldKey, hadGold, savedGold);
            RestoreInt(LivesKey, hadLives, savedLives);

            if (hadNextLifeTimestamp)
            {
                PlayerPrefs.SetString(
                    NextLifeTimestampKey,
                    savedNextLifeTimestamp);
            }
            else
            {
                PlayerPrefs.DeleteKey(NextLifeTimestampKey);
            }

            PlayerPrefs.Save();
        }

        private static void RestoreInt(
            string key,
            bool hadValue,
            int savedValue)
        {
            if (hadValue)
            {
                PlayerPrefs.SetInt(key, savedValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
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

        private RectTransform CreateHudObject(string name)
        {
            GameObject hudObject = new GameObject(
                name,
                typeof(RectTransform));
            RectTransform hudTransform =
                hudObject.GetComponent<RectTransform>();
            hudTransform.SetParent(controllerObject.transform, false);
            return hudTransform;
        }

        private static void ConfigureHudAnimator(
            HudFeedbackAnimator animator,
            RectTransform goldHud,
            RectTransform lifeHud)
        {
            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("goldHud").objectReferenceValue =
                goldHud;
            serializedAnimator.FindProperty("lifeHud").objectReferenceValue =
                lifeHud;
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
    }
}
