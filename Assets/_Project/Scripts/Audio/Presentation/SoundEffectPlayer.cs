using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class SoundEffectPlayer : MonoBehaviour
    {
        [SerializeField] private SoundEffectLibrary soundEffectLibrary;
        [SerializeField] private AudioSource audioSource;

        private SoundEffectPlaybackService playbackService;

        private void Awake()
        {
            playbackService = new SoundEffectPlaybackService(soundEffectLibrary, new AudioSourceSoundEffectOutput(audioSource));
        }

        public void Play(SoundEffectId soundEffectId)
        {
            playbackService.Play(soundEffectId);
        }
    }
}
