using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class SoundEffectLibraryTests
    {
        private const float ConfiguredVolumeScale = 0.4f;

        private AudioClip clip;
        private SoundEffectLibrary soundEffectLibrary;

        [SetUp]
        public void SetUp()
        {
            clip = AudioClip.Create("ButtonClick", 1, 1, 44100, false);
            soundEffectLibrary = SoundEffectLibraryTestFactory.Create(
                SoundEffectId.ButtonClick,
                clip,
                ConfiguredVolumeScale);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(soundEffectLibrary);
            UnityEngine.Object.DestroyImmediate(clip);
        }

        [Test]
        public void Get_WithConfiguredSound_ReturnsDefinition()
        {
            SoundEffectDefinition soundEffect = soundEffectLibrary.Get(SoundEffectId.ButtonClick);

            Assert.That(soundEffect.Id, Is.EqualTo(SoundEffectId.ButtonClick));
            Assert.That(soundEffect.Clip, Is.SameAs(clip));
            Assert.That(soundEffect.VolumeScale, Is.EqualTo(ConfiguredVolumeScale));
        }

        [Test]
        public void Get_WithMissingSound_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => soundEffectLibrary.Get(SoundEffectId.ValidPour));
        }

        [Test]
        public void Get_WithInvalidSoundId_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => soundEffectLibrary.Get((SoundEffectId)99));
        }

        [Test]
        public void Get_WithMissingClip_ThrowsInvalidOperationException()
        {
            UnityEngine.Object.DestroyImmediate(soundEffectLibrary);
            soundEffectLibrary = SoundEffectLibraryTestFactory.Create(
                SoundEffectId.ButtonClick,
                null,
                ConfiguredVolumeScale);

            Assert.Throws<InvalidOperationException>(() => soundEffectLibrary.Get(SoundEffectId.ButtonClick));
        }
    }
}
