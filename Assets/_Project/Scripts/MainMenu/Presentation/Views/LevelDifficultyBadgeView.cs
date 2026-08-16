using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.MainMenu.Presentation
{
    public sealed class LevelDifficultyBadgeView : MonoBehaviour
    {
        [Header("Badge")]
        [SerializeField] private Image badgeImage;

        [Header("Difficulty Sprites")]
        [SerializeField] private Sprite easyBadge;
        [SerializeField] private Sprite mediumBadge;
        [SerializeField] private Sprite hardBadge;

        public void Show(LevelDifficulty difficulty)
        {
            badgeImage.sprite = GetBadge(difficulty);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private Sprite GetBadge(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => easyBadge,
                LevelDifficulty.Medium => mediumBadge,
                LevelDifficulty.Hard => hardBadge,
                _ => null
            };
        }
    }
}
