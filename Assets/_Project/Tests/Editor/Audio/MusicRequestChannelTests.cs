using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Audio;

namespace WaterSortPuzzle.Tests.EditMode.Audio
{
    public sealed class MusicRequestChannelTests
    {
        private MusicRequestChannel requestChannel;
        private MusicTrack musicTrack;

        [SetUp]
        public void SetUp()
        {
            requestChannel = ScriptableObject.CreateInstance<MusicRequestChannel>();
            musicTrack = ScriptableObject.CreateInstance<MusicTrack>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(musicTrack);
            UnityEngine.Object.DestroyImmediate(requestChannel);
        }

        [Test]
        public void Request_WithMusicTrack_NotifiesSubscribers()
        {
            MusicTrack requestedMusicTrack = null;
            requestChannel.Requested += requestedTrack => requestedMusicTrack = requestedTrack;

            requestChannel.Request(musicTrack);

            Assert.That(requestedMusicTrack, Is.SameAs(musicTrack));
        }

        [Test]
        public void Request_WithNullMusicTrack_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => requestChannel.Request(null));
        }

        [Test]
        public void RequestPause_NotifiesSubscribers()
        {
            bool wasPauseRequested = false;
            requestChannel.PauseRequested += () => wasPauseRequested = true;

            requestChannel.RequestPause();

            Assert.That(wasPauseRequested, Is.True);
        }

        [Test]
        public void RequestResume_NotifiesSubscribers()
        {
            bool wasResumeRequested = false;
            requestChannel.ResumeRequested += () => wasResumeRequested = true;

            requestChannel.RequestResume();

            Assert.That(wasResumeRequested, Is.True);
        }
    }
}
