using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    public interface ISoundEffectOutput
    {
        void PlayOneShot(AudioClip clip, float volumeScale);
    }
}
