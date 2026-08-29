using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class SoundEffectPlayer : MonoBehaviour
    {
        [SerializeField] private SoundEffectRequestChannel requestChannel;
        [SerializeField] private AudioSettingsChannel settingsChannel;
        [SerializeField] private SoundEffectLibrary soundEffectLibrary;
        [SerializeField] private AudioSource audioSource;

        private SoundEffectPlaybackService playbackService;

        private void Awake()
        {
            playbackService = new SoundEffectPlaybackService(soundEffectLibrary, new AudioSourceSoundEffectOutput(audioSource));
        }

        private void OnEnable()
        {
            requestChannel.Requested += Play;

            if (settingsChannel != null)
            {
                settingsChannel.Changed += ApplySettings;
                ApplySettings(settingsChannel.Current);
            }
        }

        private void OnDisable()
        {
            requestChannel.Requested -= Play;

            if (settingsChannel != null)
            {
                settingsChannel.Changed -= ApplySettings;
            }
        }

        public void Play(SoundEffectId soundEffectId)
        {
            playbackService.Play(soundEffectId);
        }

        private void ApplySettings(PlayerAudioSettings settings)
        {
            SetVolume(settings.SoundEffectVolume);
        }

        public void SetVolume(float volume)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}
