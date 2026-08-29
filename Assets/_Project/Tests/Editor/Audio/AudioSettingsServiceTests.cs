using System;
using NUnit.Framework;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class AudioSettingsServiceTests
    {
        [Test]
        public void Constructor_WithMissingStore_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new AudioSettingsService(null));
        }

        [Test]
        public void SetMusicVolume_PersistsAndPreservesSoundEffectVolume()
        {
            InMemoryAudioSettingsStore store = new InMemoryAudioSettingsStore(
                new PlayerAudioSettings(1f, 0.4f));
            AudioSettingsService service = new AudioSettingsService(store);

            PlayerAudioSettings updated = service.SetMusicVolume(0.25f);

            Assert.That(updated.MusicVolume, Is.EqualTo(0.25f));
            Assert.That(updated.SoundEffectVolume, Is.EqualTo(0.4f));
            Assert.That(store.SavedSettings, Is.SameAs(updated));
        }

        [Test]
        public void SetSoundEffectVolume_PersistsAndPreservesMusicVolume()
        {
            InMemoryAudioSettingsStore store = new InMemoryAudioSettingsStore(
                new PlayerAudioSettings(0.6f, 1f));
            AudioSettingsService service = new AudioSettingsService(store);

            PlayerAudioSettings updated =
                service.SetSoundEffectVolume(0.15f);

            Assert.That(updated.MusicVolume, Is.EqualTo(0.6f));
            Assert.That(updated.SoundEffectVolume, Is.EqualTo(0.15f));
            Assert.That(store.SavedSettings, Is.SameAs(updated));
        }

        private sealed class InMemoryAudioSettingsStore : IAudioSettingsStore
        {
            private PlayerAudioSettings settings;

            public InMemoryAudioSettingsStore(PlayerAudioSettings settings)
            {
                this.settings = settings;
            }

            public PlayerAudioSettings SavedSettings { get; private set; }

            public PlayerAudioSettings Load()
            {
                return settings;
            }

            public void Save(PlayerAudioSettings savedSettings)
            {
                settings = savedSettings;
                SavedSettings = savedSettings;
            }
        }
    }
}
