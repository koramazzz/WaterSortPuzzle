using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.MainMenu.Presentation
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private LevelDifficultyBadgeView difficultyBadge;

        [Header("Play Button")]
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text playButtonText;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private string completedTitle;

        [Header("Player Resources")]
        [SerializeField] private PlayerResourcesHudController resourcesHud;

        [Header("Navigation")]
        [SerializeField] private string levelSceneName;

        private readonly LevelDataLoader levelDataLoader = new LevelDataLoader();
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
                difficultyBadge.Hide();
                return;
            }

            TextAsset levelFile = levelCatalog.LevelFiles[completedLevelCount];
            LevelData levelData = levelDataLoader.Load(levelFile);

            playButtonText.SetText(levelTitleFormat, levelData.LevelNumber);
            difficultyBadge.Show(levelData.Difficulty);
        }

        public void Play()
        {
            if (isCompleted)
            {
                return;
            }

            if (!resourcesHud.CheckLifeAvailability())
            {
                return;
            }

            SceneManager.LoadScene(levelSceneName);
        }
    }
}
