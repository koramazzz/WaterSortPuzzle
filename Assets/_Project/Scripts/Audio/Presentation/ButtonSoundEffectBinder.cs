using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class ButtonSoundEffectBinder : MonoBehaviour
    {
        [SerializeField] private SoundEffectRequestChannel requestChannel;
        [SerializeField] private SoundEffectId soundEffectId;

        private readonly List<Button> buttons = new List<Button>();

        private void OnEnable()
        {
            buttons.AddRange(GetComponentsInChildren<Button>(true));

            foreach (Button button in buttons)
            {
                button.onClick.AddListener(RequestSoundEffect);
            }
        }

        private void OnDisable()
        {
            foreach (Button button in buttons)
            {
                button.onClick.RemoveListener(RequestSoundEffect);
            }

            buttons.Clear();
        }

        private void RequestSoundEffect()
        {
            requestChannel.Request(soundEffectId);
        }
    }
}
