using System;
using NUnit.Framework;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class PlayerAudioSettingsTests
    {
        [Test]
        public void Constructor_WithValidVolumes_StoresValues()
        {
            PlayerAudioSettings settings =
                new PlayerAudioSettings(0.25f, 0.75f);

            Assert.That(settings.MusicVolume, Is.EqualTo(0.25f));
            Assert.That(settings.SoundEffectVolume, Is.EqualTo(0.75f));
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        public void Constructor_WithInvalidMusicVolume_Throws(float volume)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerAudioSettings(volume, 1f));
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        public void Constructor_WithInvalidSoundEffectVolume_Throws(
            float volume)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerAudioSettings(1f, volume));
        }

        [Test]
        public void WithMusicVolume_PreservesSoundEffectVolume()
        {
            PlayerAudioSettings settings =
                new PlayerAudioSettings(1f, 0.4f);

            PlayerAudioSettings updated = settings.WithMusicVolume(0.2f);

            Assert.That(updated.MusicVolume, Is.EqualTo(0.2f));
            Assert.That(updated.SoundEffectVolume, Is.EqualTo(0.4f));
        }

        [Test]
        public void WithSoundEffectVolume_PreservesMusicVolume()
        {
            PlayerAudioSettings settings =
                new PlayerAudioSettings(0.6f, 1f);

            PlayerAudioSettings updated =
                settings.WithSoundEffectVolume(0.3f);

            Assert.That(updated.MusicVolume, Is.EqualTo(0.6f));
            Assert.That(updated.SoundEffectVolume, Is.EqualTo(0.3f));
        }
    }
}
