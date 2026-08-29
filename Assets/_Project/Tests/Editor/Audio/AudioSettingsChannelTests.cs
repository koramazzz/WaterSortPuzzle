using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class AudioSettingsChannelTests
    {
        private AudioSettingsChannel channel;

        [SetUp]
        public void SetUp()
        {
            channel = ScriptableObject.CreateInstance<AudioSettingsChannel>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(channel);
        }

        [Test]
        public void Current_BeforeApply_ReturnsFullVolumeDefaults()
        {
            Assert.That(
                channel.Current.MusicVolume,
                Is.EqualTo(PlayerAudioSettings.DefaultVolume));
            Assert.That(
                channel.Current.SoundEffectVolume,
                Is.EqualTo(PlayerAudioSettings.DefaultVolume));
        }

        [Test]
        public void Apply_PublishesAndRetainsSettings()
        {
            PlayerAudioSettings published = null;
            PlayerAudioSettings settings =
                new PlayerAudioSettings(0.35f, 0.65f);
            channel.Changed += value => published = value;

            channel.Apply(settings);

            Assert.That(published, Is.SameAs(settings));
            Assert.That(channel.Current, Is.SameAs(settings));
        }

        [Test]
        public void Apply_WithMissingSettings_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => channel.Apply(null));
        }
    }
}
