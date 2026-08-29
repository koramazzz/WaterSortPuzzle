using System;

namespace WaterSortPuzzle.Audio
{
    public sealed class AudioSettingsService
    {
        private readonly IAudioSettingsStore settingsStore;

        private PlayerAudioSettings current;

        public AudioSettingsService(IAudioSettingsStore settingsStore)
        {
            this.settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        public PlayerAudioSettings Load()
        {
            current = settingsStore.Load() ?? throw new InvalidOperationException(
                    "Audio settings store returned no settings.");

            return current;
        }

        public PlayerAudioSettings SetMusicVolume(float volume)
        {
            EnsureLoaded();
            current = current.WithMusicVolume(volume);
            settingsStore.Save(current);
            return current;
        }

        public PlayerAudioSettings SetSoundEffectVolume(float volume)
        {
            EnsureLoaded();
            current = current.WithSoundEffectVolume(volume);
            settingsStore.Save(current);
            return current;
        }

        private void EnsureLoaded()
        {
            if (current == null)
            {
                Load();
            }
        }
    }
}
