using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.MainMenu.Presentation
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private const int FirstLevelNumber = 1;

        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text playButtonText;
        [SerializeField] private PlayerResourcesHudView resourcesHud;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private string completedTitle;
        [SerializeField] private string levelSceneName;

        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private readonly PlayerPrefsGoldStore goldStore = new PlayerPrefsGoldStore();

        private void Start()
        {
            int levelCount = levelCatalog.LevelFiles.Count;
            int completedLevelCount = progressStore.LoadCompletedLevelCount(levelCount);
            bool isCompleted = completedLevelCount == levelCount;
            resourcesHud.ShowGold(goldStore.LoadGold());

            playButton.interactable = !isCompleted;

            if (isCompleted)
            {
                playButtonText.SetText(completedTitle);
                return;
            }

            int levelNumber = completedLevelCount + FirstLevelNumber;
            playButtonText.SetText(levelTitleFormat, levelNumber);
        }

        public void Play()
        {
            SceneManager.LoadScene(levelSceneName);
        }
    }
}
