using System;
using TMPro;
using UnityEngine;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress.Presentation
{
    public sealed class PlayerResourcesHudView : MonoBehaviour
    {
        [Header("Gold")]
        [SerializeField] private TMP_Text goldText;

        [Header("Lives")]
        [SerializeField] private TMP_Text lifeCountText;
        [SerializeField] private TMP_Text lifeTimeText;
        [SerializeField] private string fullLivesText;

        public void Show(PlayerResources resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            ShowGold(resources.Gold);
            ShowLives(resources.Lives, resources.SecondsUntilNextLife);
        }

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
            if (lives < 0 || lives > GameBalance.MaximumLives)
            {
                throw new ArgumentOutOfRangeException(nameof(lives));
            }

            if (secondsUntilNextLife < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(secondsUntilNextLife));
            }

            lifeCountText.SetText(lives.ToString());

            if (lives == GameBalance.MaximumLives)
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
