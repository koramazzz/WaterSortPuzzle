using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Gameplay.Levels.Loading;
using WaterSortPuzzle.Levels.Rewards;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;
using WaterSortPuzzle.Progress.Presentation;

namespace WaterSortPuzzle.Gameplay.Levels.Presentation
{
    public sealed class LevelSceneController : MonoBehaviour
    {
        private const int CompletedLevelCountIncrement = 1;

        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private PlayerResourcesHudController resourcesHud;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private BottleCollectionView bottleCollectionView;
        [SerializeField] private string mainSceneName;

        private readonly LevelCatalogLoader levelCatalogLoader = new LevelCatalogLoader();
        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private readonly BottleInteractionPresenter bottleInteractionPresenter = new BottleInteractionPresenter();
        private readonly LevelOutcomeEvaluator levelOutcomeEvaluator = new LevelOutcomeEvaluator();
        private readonly LevelRewardCalculator levelRewardCalculator = new LevelRewardCalculator();

        private int completedLevelCount;
        private int levelCount;
        private LevelState levelState;
        private bool hasEnded;
        private int winGoldReward = GameBalance.BaseGoldReward;

        public event Action<LevelOutcome> LevelEnded;

        public int WinGoldReward => winGoldReward;

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
            winGoldReward = levelRewardCalculator.CalculateGoldReward(levelState.Difficulty);
            bottleCollectionView.Initialize(levelState.Bottles);
            bottleInteractionPresenter.PourCompleted += HandlePourCompleted;
            bottleInteractionPresenter.Initialize(bottleCollectionView);
        }

        private void HandlePourCompleted()
        {
            if (hasEnded)
            {
                return;
            }

            LevelOutcome outcome = levelOutcomeEvaluator.Evaluate(levelState);

            if (outcome == LevelOutcome.InProgress)
            {
                return;
            }

            hasEnded = true;

            if (outcome == LevelOutcome.Completed)
            {
                completedLevelCount += CompletedLevelCountIncrement;
                progressStore.SaveCompletedLevelCount(completedLevelCount, levelCount);
                resourcesHud.AddGold(WinGoldReward);
            }
            else if (outcome == LevelOutcome.Failed)
            {
                resourcesHud.ConsumeLife();
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
            PlayerResources resources = resourcesHud.Refresh();

            if (resources.Lives == 0)
            {
                resourcesHud.PlayInsufficientFeedback(PlayerResourceType.Life);
                return;
            }

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
