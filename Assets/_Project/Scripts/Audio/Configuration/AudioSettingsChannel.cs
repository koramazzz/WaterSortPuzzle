using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [CreateAssetMenu(
        fileName = "AudioSettingsChannel",
        menuName = "Water Sort Puzzle/Audio/Audio Settings Channel")]
    public sealed class AudioSettingsChannel : ScriptableObject
    {
        private PlayerAudioSettings current;

        public event Action<PlayerAudioSettings> Changed;

        public PlayerAudioSettings Current =>
            current ?? new PlayerAudioSettings(
                PlayerAudioSettings.DefaultVolume,
                PlayerAudioSettings.DefaultVolume);

        public void Apply(PlayerAudioSettings settings)
        {
            current = settings ?? throw new ArgumentNullException(nameof(settings));
            Changed?.Invoke(current);
        }
    }
}
