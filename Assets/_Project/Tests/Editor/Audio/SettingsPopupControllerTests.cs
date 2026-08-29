using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class SettingsPopupControllerTests
    {
        private const string MusicVolumeKey =
            "WaterSortPuzzle.Audio.MusicVolume";
        private const string SoundEffectVolumeKey =
            "WaterSortPuzzle.Audio.SoundEffectVolume";

        private bool hadMusicVolume;
        private bool hadSoundEffectVolume;
        private float savedMusicVolume;
        private float savedSoundEffectVolume;

        private GameObject controllerObject;
        private GameObject popupObject;
        private GameObject settingsButtonObject;
        private GameObject closeButtonObject;
        private GameObject musicToggleObject;
        private GameObject soundEffectToggleObject;
        private GameObject musicSliderObject;
        private GameObject soundEffectSliderObject;
        private Button settingsButton;
        private Button closeButton;
        private Button musicToggleButton;
        private Button soundEffectToggleButton;
        private Slider musicSlider;
        private Slider soundEffectSlider;
        private AudioSettingsChannel settingsChannel;
        private SettingsPopupController controller;

        [SetUp]
        public void SetUp()
        {
            SavePlayerPrefs();
            ClearPlayerPrefs();

            popupObject = new GameObject("SettingsPopup");
            settingsButton = CreateButton(
                "SettingsButton",
                out settingsButtonObject);
            closeButton = CreateButton(
                "CloseButton",
                out closeButtonObject);
            musicToggleButton = CreateButton(
                "MusicToggleButton",
                out musicToggleObject);
            soundEffectToggleButton = CreateButton(
                "SoundEffectToggleButton",
                out soundEffectToggleObject);
            musicSlider = CreateSlider(
                "MusicSlider",
                out musicSliderObject);
            soundEffectSlider = CreateSlider(
                "SoundEffectSlider",
                out soundEffectSliderObject);
            settingsChannel =
                ScriptableObject.CreateInstance<AudioSettingsChannel>();

            controllerObject = new GameObject("SettingsController");
            controllerObject.SetActive(false);
            controller =
                controllerObject.AddComponent<SettingsPopupController>();
            Configure(controller);
            InvokeLifecycleMethod("Awake");
            InvokeLifecycleMethod("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            InvokeLifecycleMethod("OnDisable");
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(popupObject);
            Object.DestroyImmediate(settingsButtonObject);
            Object.DestroyImmediate(closeButtonObject);
            Object.DestroyImmediate(musicToggleObject);
            Object.DestroyImmediate(soundEffectToggleObject);
            Object.DestroyImmediate(musicSliderObject);
            Object.DestroyImmediate(soundEffectSliderObject);
            Object.DestroyImmediate(settingsChannel);

            RestorePlayerPrefs();
        }

        [Test]
        public void Awake_LoadsDefaultsAndClosesPopup()
        {
            Assert.That(popupObject.activeSelf, Is.False);
            Assert.That(musicSlider.value, Is.EqualTo(1f));
            Assert.That(soundEffectSlider.value, Is.EqualTo(1f));
            Assert.That(settingsChannel.Current.MusicVolume, Is.EqualTo(1f));
            Assert.That(
                settingsChannel.Current.SoundEffectVolume,
                Is.EqualTo(1f));
        }

        [Test]
        public void SettingsAndCloseButtons_TogglePopupVisibility()
        {
            settingsButton.onClick.Invoke();

            Assert.That(popupObject.activeSelf, Is.True);

            closeButton.onClick.Invoke();

            Assert.That(popupObject.activeSelf, Is.False);
        }

        [Test]
        public void MusicSlider_UpdatesChannelAndPersistsVolume()
        {
            musicSlider.value = 0.35f;

            Assert.That(
                settingsChannel.Current.MusicVolume,
                Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(
                PlayerPrefs.GetFloat(MusicVolumeKey),
                Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void MusicToggle_MutesAndRestoresPreviousVolume()
        {
            musicSlider.value = 0.4f;

            musicToggleButton.onClick.Invoke();

            Assert.That(musicSlider.value, Is.Zero);
            Assert.That(settingsChannel.Current.MusicVolume, Is.Zero);

            musicToggleButton.onClick.Invoke();

            Assert.That(musicSlider.value, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(
                settingsChannel.Current.MusicVolume,
                Is.EqualTo(0.4f).Within(0.001f));
        }

        private void Configure(SettingsPopupController controller)
        {
            SerializedObject serializedController =
                new SerializedObject(controller);
            serializedController.FindProperty("settingsPopup")
                .objectReferenceValue = popupObject;
            serializedController.FindProperty("settingsButton")
                .objectReferenceValue = settingsButton;
            serializedController.FindProperty("closeButton")
                .objectReferenceValue = closeButton;
            serializedController.FindProperty("musicToggleButton")
                .objectReferenceValue = musicToggleButton;
            serializedController.FindProperty("musicSlider")
                .objectReferenceValue = musicSlider;
            serializedController.FindProperty("soundEffectToggleButton")
                .objectReferenceValue = soundEffectToggleButton;
            serializedController.FindProperty("soundEffectSlider")
                .objectReferenceValue = soundEffectSlider;
            serializedController.FindProperty("settingsChannel")
                .objectReferenceValue = settingsChannel;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private void InvokeLifecycleMethod(string methodName)
        {
            MethodInfo method = typeof(SettingsPopupController).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method.Invoke(controller, null);
        }

        private static Button CreateButton(
            string name,
            out GameObject buttonObject)
        {
            buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            return buttonObject.GetComponent<Button>();
        }

        private static Slider CreateSlider(
            string name,
            out GameObject sliderObject)
        {
            sliderObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Slider));
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private void SavePlayerPrefs()
        {
            hadMusicVolume = PlayerPrefs.HasKey(MusicVolumeKey);
            hadSoundEffectVolume = PlayerPrefs.HasKey(SoundEffectVolumeKey);
            savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
            savedSoundEffectVolume = PlayerPrefs.GetFloat(
                SoundEffectVolumeKey);
        }

        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(SoundEffectVolumeKey);
        }

        private void RestorePlayerPrefs()
        {
            Restore(MusicVolumeKey, hadMusicVolume, savedMusicVolume);
            Restore(
                SoundEffectVolumeKey,
                hadSoundEffectVolume,
                savedSoundEffectVolume);
            PlayerPrefs.Save();
        }

        private static void Restore(
            string key,
            bool hadValue,
            float savedValue)
        {
            if (hadValue)
            {
                PlayerPrefs.SetFloat(key, savedValue);
                return;
            }

            PlayerPrefs.DeleteKey(key);
        }
    }
}
