using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [CreateAssetMenu(
        fileName = "MusicRequestChannel",
        menuName = "Water Sort Puzzle/Audio/Music Request Channel")]
    public sealed class MusicRequestChannel : ScriptableObject
    {
        public event Action<MusicTrack> Requested;
        public event Action PauseRequested;
        public event Action ResumeRequested;

        public void Request(MusicTrack musicTrack)
        {
            if (musicTrack == null)
            {
                throw new ArgumentNullException(nameof(musicTrack));
            }

            Requested?.Invoke(musicTrack);
        }

        public void RequestPause()
        {
            PauseRequested?.Invoke();
        }

        public void RequestResume()
        {
            ResumeRequested?.Invoke();
        }
    }
}
