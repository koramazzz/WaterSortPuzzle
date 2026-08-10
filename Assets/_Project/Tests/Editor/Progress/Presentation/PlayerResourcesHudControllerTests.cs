using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
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
            controller = controllerObject
                .AddComponent<PlayerResourcesHudController>();

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
        public void Refresh_UpdatesEveryDisplayedResource()
        {
            PlayerResources resources = controller.Refresh();

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(
                goldText.text,
                Is.EqualTo(GameBalance.InitialGold.ToString()));
            Assert.That(
                lifeCountText.text,
                Is.EqualTo(GameBalance.MaximumLives.ToString()));
            Assert.That(lifeTimeText.text, Is.EqualTo("FULL"));
        }

        [Test]
        public void AddGold_PersistsAndDisplaysUpdatedGold()
        {
            PlayerResources resources = controller.AddGold(50);

            int expectedGold = GameBalance.InitialGold + 50;

            Assert.That(resources.Gold, Is.EqualTo(expectedGold));
            Assert.That(goldText.text, Is.EqualTo(expectedGold.ToString()));
            Assert.That(
                new PlayerPrefsPlayerResourcesStore().Load().Gold,
                Is.EqualTo(expectedGold));
        }

        [Test]
        public void ConsumeLife_PersistsAndDisplaysUpdatedLives()
        {
            PlayerResources resources = controller.ConsumeLife();

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                lifeCountText.text,
                Is.EqualTo((GameBalance.MaximumLives - 1).ToString()));
            Assert.That(
                lifeTimeText.text,
                Is.EqualTo(
                    $"{GameBalance.LifeRefillDurationSeconds / 60:00}:00"));
            Assert.That(
                new PlayerPrefsPlayerResourcesStore().Load().Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
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
    }
}
