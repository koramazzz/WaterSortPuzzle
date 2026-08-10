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
        [SerializeField] private PlayerResourcesHudController resourcesHud;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private string completedTitle;
        [SerializeField] private string levelSceneName;

        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private bool isCompleted;

        private void Start()
        {
            int levelCount = levelCatalog.LevelFiles.Count;
            int completedLevelCount = progressStore.LoadCompletedLevelCount(levelCount);
            isCompleted = completedLevelCount == levelCount;

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
            if (isCompleted)
            {
                return;
            }

            PlayerResources resources = resourcesHud.Refresh();

            if (resources.Lives == 0)
            {
                return;
            }

            SceneManager.LoadScene(levelSceneName);
        }
    }
}
