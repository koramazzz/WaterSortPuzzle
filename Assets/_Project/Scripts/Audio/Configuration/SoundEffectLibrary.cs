using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [CreateAssetMenu(
        fileName = "SoundEffectLibrary",
        menuName = "Water Sort Puzzle/Audio/Sound Effect Library")]
    public sealed class SoundEffectLibrary : ScriptableObject
    {
        [SerializeField] private SoundEffectDefinition[] soundEffects = Array.Empty<SoundEffectDefinition>();

        public SoundEffectDefinition Get(SoundEffectId soundEffectId)
        {
            if (!Enum.IsDefined(typeof(SoundEffectId), soundEffectId))
            {
                throw new ArgumentOutOfRangeException(nameof(soundEffectId), soundEffectId, null);
            }

            foreach (SoundEffectDefinition soundEffect in soundEffects)
            {
                if (soundEffect == null || soundEffect.Id != soundEffectId)
                {
                    continue;
                }

                if (soundEffect.Clip == null)
                {
                    throw new InvalidOperationException($"Sound effect '{soundEffectId}' has no audio clip.");
                }

                return soundEffect;
            }

            throw new KeyNotFoundException($"Sound effect library does not contain '{soundEffectId}'.");
        }
    }
}
