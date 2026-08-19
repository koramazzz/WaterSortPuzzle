using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class SoundEffectPlaybackServiceTests
    {
        private const float ConfiguredVolumeScale = 0.6f;

        private AudioClip clip;
        private SoundEffectLibrary soundEffectLibrary;
        private RecordingSoundEffectOutput soundEffectOutput;

        [SetUp]
        public void SetUp()
        {
            clip = AudioClip.Create("BottleSelected", 1, 1, 44100, false);
            soundEffectLibrary = SoundEffectLibraryTestFactory.Create(
                SoundEffectId.BottleSelected,
                clip,
                ConfiguredVolumeScale);
            soundEffectOutput = new RecordingSoundEffectOutput();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(soundEffectLibrary);
            UnityEngine.Object.DestroyImmediate(clip);
        }

        [Test]
        public void Constructor_WithMissingLibrary_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SoundEffectPlaybackService(null, soundEffectOutput));
        }

        [Test]
        public void Constructor_WithMissingOutput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SoundEffectPlaybackService(soundEffectLibrary, null));
        }

        [Test]
        public void Play_WithConfiguredSound_PlaysClipUsingConfiguredVolume()
        {
            SoundEffectPlaybackService service = new SoundEffectPlaybackService(soundEffectLibrary, soundEffectOutput);

            service.Play(SoundEffectId.BottleSelected);

            Assert.That(soundEffectOutput.PlayCount, Is.EqualTo(1));
            Assert.That(soundEffectOutput.LastClip, Is.SameAs(clip));
            Assert.That(soundEffectOutput.LastVolumeScale, Is.EqualTo(ConfiguredVolumeScale));
        }

        [Test]
        public void AudioSourceOutput_WithMissingAudioSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioSourceSoundEffectOutput(null));
        }

        private sealed class RecordingSoundEffectOutput : ISoundEffectOutput
        {
            public int PlayCount { get; private set; }
            public AudioClip LastClip { get; private set; }
            public float LastVolumeScale { get; private set; }

            public void PlayOneShot(AudioClip playedClip, float volumeScale)
            {
                PlayCount++;
                LastClip = playedClip;
                LastVolumeScale = volumeScale;
            }
        }
    }
}
