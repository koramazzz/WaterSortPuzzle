using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Gameplay.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Gameplay.Levels.Presentation
{
    public sealed class LevelSceneController : MonoBehaviour
    {
        private const int CompletedLevelCountIncrement = 1;

        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private BottleCollectionView bottleCollectionView;
        [SerializeField] private string mainSceneName;

        private readonly LevelCatalogLoader levelCatalogLoader = new LevelCatalogLoader();
        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private readonly BottleInteractionPresenter bottleInteractionPresenter = new BottleInteractionPresenter();
        private readonly LevelOutcomeEvaluator levelOutcomeEvaluator = new LevelOutcomeEvaluator();

        private int completedLevelCount;
        private int levelCount;
        private LevelState levelState;

        public event Action<LevelOutcome> LevelEnded;

        private void Start()
        {
            levelCount = levelCatalog.LevelFiles.Count;
            completedLevelCount = progressStore.LoadCompletedLevelCount(levelCount);

            if (!levelCatalogLoader.TryLoad(
                    levelCatalog,
                    completedLevelCount,
                    out levelState))
            {
                levelText.gameObject.SetActive(false);
                return;
            }

            levelText.SetText(levelTitleFormat, levelState.LevelNumber);
            bottleCollectionView.Initialize(levelState.Bottles);
            bottleInteractionPresenter.PourCompleted += HandlePourCompleted;
            bottleInteractionPresenter.Initialize(bottleCollectionView);
        }

        private void HandlePourCompleted()
        {
            LevelOutcome outcome = levelOutcomeEvaluator.Evaluate(levelState);

            if (outcome == LevelOutcome.InProgress)
            {
                return;
            }

            if (outcome == LevelOutcome.Completed)
            {
                completedLevelCount += CompletedLevelCountIncrement;
                progressStore.SaveCompletedLevelCount(completedLevelCount, levelCount);
            }

            LevelEnded?.Invoke(outcome);
        }

        public void LoadNextLevel()
        {
            if (completedLevelCount == levelCount)
            {
                ReturnToMainMenu();
                return;
            }

            ReloadLevelScene();
        }

        public void RetryLevel()
        {
            ReloadLevelScene();
        }

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainSceneName);
        }

        private void ReloadLevelScene()
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
