using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [Serializable]
    public sealed class SoundEffectDefinition
    {
        [SerializeField] private SoundEffectId id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volumeScale;

        public SoundEffectId Id => id;
        public AudioClip Clip => clip;
        public float VolumeScale => volumeScale;
    }
}
