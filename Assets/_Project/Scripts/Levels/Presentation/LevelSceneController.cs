using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaterSortPuzzle.Audio;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Gameplay.Bottles;
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

        [Header("Level")]
        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private string levelTitleFormat;

        [Header("Gameplay")]
        [SerializeField] private BottleCollectionView bottleCollectionView;
        [SerializeField] private PlayerResourcesHudController resourcesHud;

        [Header("Audio")]
        [SerializeField] private SoundEffectRequestChannel soundEffectRequests;
        [SerializeField] private MusicRequestChannel musicRequests;

        [Header("Navigation")]
        [SerializeField] private string mainSceneName;

        private readonly LevelCatalogLoader levelCatalogLoader = new LevelCatalogLoader();
        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private readonly BottleInteractionPresenter bottleInteractionPresenter = new BottleInteractionPresenter();
        private readonly LevelOutcomeEvaluator levelOutcomeEvaluator = new LevelOutcomeEvaluator();
        private readonly LevelRewardCalculator levelRewardCalculator = new LevelRewardCalculator();
        private readonly BottleAdditionProgress bottleAdditionProgress = new BottleAdditionProgress();

        private int completedLevelCount;
        private int levelCount;
        private LevelState levelState;
        private LevelOutcome currentOutcome;
        private int winGoldReward = GameBalance.BaseGoldReward;

        public event Action<LevelOutcome> OutcomeChanged;

        public int WinGoldReward => winGoldReward;

        public bool HasAvailableBottleAddition => bottleAdditionProgress.CanAddBottle;

        public int AddBottleGoldCost => GameBalance.AddBottleGoldCost;

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
            bottleInteractionPresenter.SoundEffectRequested += soundEffectRequests.Request;
            bottleCollectionView.BottleCompletionAnimationFinished += HandleBottleCompletionAnimationFinished;
            bottleInteractionPresenter.Initialize(bottleCollectionView);
        }

        private void HandlePourCompleted()
        {
            soundEffectRequests.Request(SoundEffectId.ValidPour);
            LevelOutcome evaluatedOutcome = levelOutcomeEvaluator.Evaluate(levelState);

            if (evaluatedOutcome == currentOutcome)
            {
                return;
            }

            ApplyOutcome(evaluatedOutcome);
        }

        private void HandleBottleCompletionAnimationFinished()
        {
            soundEffectRequests.Request(SoundEffectId.CapClosed);
        }

        private void ApplyOutcome(LevelOutcome outcome)
        {
            currentOutcome = outcome;

            switch (outcome)
            {
                case LevelOutcome.Completed:
                    CompleteLevel();
                    soundEffectRequests.Request(SoundEffectId.LevelCompleted);
                    break;

                case LevelOutcome.Failed:
                    FailLevel();
                    soundEffectRequests.Request(SoundEffectId.LevelFailed);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }

            musicRequests.RequestPause();
            OutcomeChanged?.Invoke(outcome);
        }

        private void CompleteLevel()
        {
            completedLevelCount += CompletedLevelCountIncrement;
            progressStore.SaveCompletedLevelCount(completedLevelCount, levelCount);
            resourcesHud.RewardGold(WinGoldReward);
        }

        private void FailLevel()
        {
            resourcesHud.TryConsumeLife();
        }

        public void AddBottle()
        {
            if (currentOutcome != LevelOutcome.Failed || !bottleAdditionProgress.CanAddBottle)
            {
                return;
            }

            if (!resourcesHud.TrySpendGold(AddBottleGoldCost))
            {
                return;
            }

            BottleState addedBottle = levelState.AddEmptyBottle();
            bottleCollectionView.AddBottle(addedBottle);
            bottleAdditionProgress.RecordBottleAdded();
            soundEffectRequests.Request(SoundEffectId.BottleAdded);
            currentOutcome = LevelOutcome.InProgress;
            musicRequests.RequestResume();
            OutcomeChanged?.Invoke(currentOutcome);
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
            if (!resourcesHud.CheckLifeAvailability())
            {
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
