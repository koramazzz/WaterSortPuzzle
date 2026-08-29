using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class PlayerPrefsAudioSettingsStoreTests
    {
        private const string MusicVolumeKey =
            "WaterSortPuzzle.Audio.MusicVolume";
        private const string SoundEffectVolumeKey =
            "WaterSortPuzzle.Audio.SoundEffectVolume";

        private bool hadMusicVolume;
        private bool hadSoundEffectVolume;
        private float savedMusicVolume;
        private float savedSoundEffectVolume;

        [SetUp]
        public void SetUp()
        {
            hadMusicVolume = PlayerPrefs.HasKey(MusicVolumeKey);
            hadSoundEffectVolume = PlayerPrefs.HasKey(SoundEffectVolumeKey);
            savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
            savedSoundEffectVolume = PlayerPrefs.GetFloat(
                SoundEffectVolumeKey);

            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(SoundEffectVolumeKey);
        }

        [TearDown]
        public void TearDown()
        {
            Restore(MusicVolumeKey, hadMusicVolume, savedMusicVolume);
            Restore(
                SoundEffectVolumeKey,
                hadSoundEffectVolume,
                savedSoundEffectVolume);
            PlayerPrefs.Save();
        }

        [Test]
        public void Load_WithoutSavedValues_ReturnsFullVolumeDefaults()
        {
            PlayerAudioSettings settings = CreateStore().Load();

            Assert.That(settings.MusicVolume, Is.EqualTo(1f));
            Assert.That(settings.SoundEffectVolume, Is.EqualTo(1f));
        }

        [Test]
        public void Save_ThenLoad_RestoresVolumes()
        {
            PlayerPrefsAudioSettingsStore store = CreateStore();
            store.Save(new PlayerAudioSettings(0.2f, 0.8f));

            PlayerAudioSettings settings = store.Load();

            Assert.That(settings.MusicVolume, Is.EqualTo(0.2f));
            Assert.That(settings.SoundEffectVolume, Is.EqualTo(0.8f));
        }

        [Test]
        public void Load_WithOutOfRangeValues_ClampsVolumes()
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, -2f);
            PlayerPrefs.SetFloat(SoundEffectVolumeKey, 3f);

            PlayerAudioSettings settings = CreateStore().Load();

            Assert.That(settings.MusicVolume, Is.Zero);
            Assert.That(settings.SoundEffectVolume, Is.EqualTo(1f));
        }

        private static PlayerPrefsAudioSettingsStore CreateStore()
        {
            return new PlayerPrefsAudioSettingsStore();
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
