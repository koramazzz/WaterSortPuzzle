using System;
using TMPro;
using UnityEngine;

namespace WaterSortPuzzle.Progress.Presentation
{
    public sealed class PlayerResourcesHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text lifeCountText;
        [SerializeField] private TMP_Text lifeTimeText;
        [SerializeField] private string fullLivesText;
        [SerializeField, Min(1)] private int maximumLives;

        public void ShowGold(int gold)
        {
            if (gold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gold));
            }

            goldText.SetText(gold.ToString());
        }

        public void ShowLives(
            int lives,
            int secondsUntilNextLife)
        {
            if (lives < 0 || lives > maximumLives)
            {
                throw new ArgumentOutOfRangeException(nameof(lives));
            }

            if (secondsUntilNextLife < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(secondsUntilNextLife));
            }

            lifeCountText.SetText(lives.ToString());

            if (lives == maximumLives)
            {
                lifeTimeText.SetText(fullLivesText);
                return;
            }

            int minutes = secondsUntilNextLife / 60;
            int seconds = secondsUntilNextLife % 60;
            lifeTimeText.SetText($"{minutes:00}:{seconds:00}");
        }
    }
}
