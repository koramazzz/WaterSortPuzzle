using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class PlayerPrefsAudioSettingsStore : IAudioSettingsStore
    {
        private const string MusicVolumeKey = "WaterSortPuzzle.Audio.MusicVolume";
        private const string SoundEffectVolumeKey = "WaterSortPuzzle.Audio.SoundEffectVolume";

        public PlayerAudioSettings Load()
        {
            return new PlayerAudioSettings(LoadVolume(MusicVolumeKey), LoadVolume(SoundEffectVolumeKey));
        }

        public void Save(PlayerAudioSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            PlayerPrefs.SetFloat(MusicVolumeKey, settings.MusicVolume);
            PlayerPrefs.SetFloat(SoundEffectVolumeKey, settings.SoundEffectVolume);
            PlayerPrefs.Save();
        }

        private static float LoadVolume(string key)
        {
            float volume = PlayerPrefs.GetFloat(key, PlayerAudioSettings.DefaultVolume);

            if (float.IsNaN(volume) || float.IsInfinity(volume))
            {
                return PlayerAudioSettings.DefaultVolume;
            }

            return Mathf.Clamp01(volume);
        }
    }
}
