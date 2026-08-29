using System;

namespace WaterSortPuzzle.Audio
{
    public sealed class PlayerAudioSettings
    {
        public const float MinimumVolume = 0f;
        public const float MaximumVolume = 1f;
        public const float DefaultVolume = MaximumVolume;

        public PlayerAudioSettings(float musicVolume, float soundEffectVolume)
        {
            ValidateVolume(musicVolume, nameof(musicVolume));
            ValidateVolume(soundEffectVolume, nameof(soundEffectVolume));

            MusicVolume = musicVolume;
            SoundEffectVolume = soundEffectVolume;
        }

        public float MusicVolume { get; }

        public float SoundEffectVolume { get; }

        public PlayerAudioSettings WithMusicVolume(float volume)
        {
            return new PlayerAudioSettings(volume, SoundEffectVolume);
        }

        public PlayerAudioSettings WithSoundEffectVolume(float volume)
        {
            return new PlayerAudioSettings(MusicVolume, volume);
        }

        private static void ValidateVolume(float volume, string parameterName)
        {
            if (float.IsNaN(volume) || float.IsInfinity(volume) || volume < MinimumVolume || volume > MaximumVolume)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
