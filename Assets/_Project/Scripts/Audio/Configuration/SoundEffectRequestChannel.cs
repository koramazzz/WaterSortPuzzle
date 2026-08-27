using System;
using UnityEngine;

namespace WaterSortPuzzle.Audio
{
    [CreateAssetMenu(
        fileName = "SoundEffectRequestChannel",
        menuName = "Water Sort Puzzle/Audio/Sound Effect Request Channel")]
    public sealed class SoundEffectRequestChannel : ScriptableObject
    {
        public event Action<SoundEffectId> Requested;

        public void Request(SoundEffectId soundEffectId)
        {
            if (!Enum.IsDefined(typeof(SoundEffectId), soundEffectId))
            {
                throw new ArgumentOutOfRangeException(nameof(soundEffectId), soundEffectId, null);
            }

            Requested?.Invoke(soundEffectId);
        }
    }
}
