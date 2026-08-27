using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class SoundEffectRequestChannelTests
    {
        private SoundEffectRequestChannel requestChannel;

        [SetUp]
        public void SetUp()
        {
            requestChannel =
                ScriptableObject.CreateInstance<SoundEffectRequestChannel>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(requestChannel);
        }

        [Test]
        public void Request_WithValidSoundEffect_NotifiesSubscribers()
        {
            SoundEffectId? requestedSoundEffect = null;
            requestChannel.Requested += soundEffectId =>
                requestedSoundEffect = soundEffectId;

            requestChannel.Request(SoundEffectId.ValidPour);

            Assert.That(
                requestedSoundEffect,
                Is.EqualTo(SoundEffectId.ValidPour));
        }

        [Test]
        public void Request_WithInvalidSoundEffect_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                requestChannel.Request((SoundEffectId)99));
        }
    }
}
