using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Audio;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Gameplay.Levels.Presentation;
using WaterSortPuzzle.Progress;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels.Presentation
{
    public sealed class LevelSceneControllerTests
    {
        private const string GoldKey = "WaterSortPuzzle.Progress.Gold";
        private const string LivesKey = "WaterSortPuzzle.Progress.Lives";
        private const string NextLifeTimestampKey = "WaterSortPuzzle.Progress.NextLifeTimestamp";

        private bool hadGold;
        private bool hadLives;
        private bool hadNextLifeTimestamp;
        private int savedGold;
        private int savedLives;
        private string savedNextLifeTimestamp;

        private GameObject levelControllerObject;
        private GameObject resourcesControllerObject;
        private LevelSceneController levelController;
        private SoundEffectRequestChannel soundEffectRequests;
        private MusicRequestChannel musicRequests;

        [SetUp]
        public void SetUp()
        {
            SavePlayerPrefs();
            ClearPlayerPrefs();

            soundEffectRequests =
                ScriptableObject.CreateInstance<SoundEffectRequestChannel>();
            musicRequests =
                ScriptableObject.CreateInstance<MusicRequestChannel>();

            PlayerResourcesHudController resourcesHud =
                CreateResourcesHudController();

            levelControllerObject = new GameObject("LevelController");
            levelControllerObject.SetActive(false);
            levelController =
                levelControllerObject.AddComponent<LevelSceneController>();

            SerializedObject serializedController =
                new SerializedObject(levelController);
            serializedController.FindProperty("resourcesHud")
                .objectReferenceValue = resourcesHud;
            serializedController.FindProperty("soundEffectRequests")
                .objectReferenceValue = soundEffectRequests;
            serializedController.FindProperty("musicRequests")
                .objectReferenceValue = musicRequests;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(levelControllerObject);
            Object.DestroyImmediate(resourcesControllerObject);
            Object.DestroyImmediate(soundEffectRequests);
            Object.DestroyImmediate(musicRequests);

            RestorePlayerPrefs();
        }

        [Test]
        public void ApplyOutcome_WhenLevelFails_KeepsLifeAvailableForContinuation()
        {
            ApplyFailedOutcome();

            Assert.That(
                LoadSavedResources().Lives,
                Is.EqualTo(GameBalance.MaximumLives));
        }

        [Test]
        public void TryForfeitUnfinishedAttempt_AfterFailure_ConsumesLife()
        {
            ApplyFailedOutcome();

            bool forfeited = InvokePrivate<bool>(
                "TryForfeitUnfinishedAttempt");

            Assert.That(forfeited, Is.True);
            Assert.That(
                LoadSavedResources().Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
        }

        private void ApplyFailedOutcome()
        {
            InvokePrivate<object>(
                "ApplyOutcome",
                LevelOutcome.Failed);
        }

        private T InvokePrivate<T>(
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = typeof(LevelSceneController).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            return (T)method.Invoke(levelController, arguments);
        }

        private PlayerResourcesHudController CreateResourcesHudController()
        {
            resourcesControllerObject = new GameObject("PlayerResourcesHud");
            resourcesControllerObject.SetActive(false);

            PlayerResourcesHudView view =
                resourcesControllerObject.AddComponent<PlayerResourcesHudView>();
            HudFeedbackAnimator hudAnimator =
                resourcesControllerObject.AddComponent<HudFeedbackAnimator>();
            PlayerResourcesHudController controller =
                resourcesControllerObject
                    .AddComponent<PlayerResourcesHudController>();

            TMP_Text goldText = CreateText("GoldText");
            TMP_Text lifeCountText = CreateText("LifeCountText");
            TMP_Text lifeTimeText = CreateText("LifeTimeText");
            RectTransform goldHud = CreateRectTransform("GoldHud");
            RectTransform lifeHud = CreateRectTransform("LifeHud");

            ConfigureResourcesView(
                view,
                goldText,
                lifeCountText,
                lifeTimeText);
            ConfigureHudAnimator(hudAnimator, goldHud, lifeHud);

            SerializedObject serializedController =
                new SerializedObject(controller);
            serializedController.FindProperty("resourcesHudView")
                .objectReferenceValue = view;
            serializedController.FindProperty("hudAnimator")
                .objectReferenceValue = hudAnimator;
            serializedController.FindProperty("soundEffectRequests")
                .objectReferenceValue = soundEffectRequests;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            resourcesControllerObject.SetActive(true);
            return controller;
        }

        private TMP_Text CreateText(string name)
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer));
            textObject.transform.SetParent(
                resourcesControllerObject.transform,
                false);
            return textObject.AddComponent<TextMeshProUGUI>();
        }

        private RectTransform CreateRectTransform(string name)
        {
            GameObject rectObject = new GameObject(
                name,
                typeof(RectTransform));
            rectObject.transform.SetParent(
                resourcesControllerObject.transform,
                false);
            return rectObject.GetComponent<RectTransform>();
        }

        private static void ConfigureResourcesView(
            PlayerResourcesHudView view,
            TMP_Text goldText,
            TMP_Text lifeCountText,
            TMP_Text lifeTimeText)
        {
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("goldText").objectReferenceValue =
                goldText;
            serializedView.FindProperty("lifeCountText")
                .objectReferenceValue = lifeCountText;
            serializedView.FindProperty("lifeTimeText")
                .objectReferenceValue = lifeTimeText;
            serializedView.FindProperty("fullLivesText").stringValue = "FULL";
            serializedView.ApplyModifiedPropertiesWithoutUndo();
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
    }
}
