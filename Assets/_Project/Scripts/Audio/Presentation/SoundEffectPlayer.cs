using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class SoundEffectPlayer : MonoBehaviour
    {
        [SerializeField] private SoundEffectRequestChannel requestChannel;
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
        }

        private void OnDisable()
        {
            requestChannel.Requested -= Play;
        }

        public void Play(SoundEffectId soundEffectId)
        {
            playbackService.Play(soundEffectId);
        }
    }
}
