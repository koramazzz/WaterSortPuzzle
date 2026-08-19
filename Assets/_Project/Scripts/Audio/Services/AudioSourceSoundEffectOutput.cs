using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public sealed class AudioSourceSoundEffectOutput : ISoundEffectOutput
    {
        private readonly AudioSource audioSource;

        public AudioSourceSoundEffectOutput(AudioSource audioSource)
        {
            this.audioSource = audioSource ?? throw new ArgumentNullException(nameof(audioSource));
        }

        public void PlayOneShot(AudioClip clip, float volumeScale)
        {
            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }

            audioSource.PlayOneShot(clip, volumeScale);
        }
    }
}
