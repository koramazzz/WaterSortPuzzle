using TMPro;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Levels.Presentation
{
    public sealed class LevelOutcomeView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private LevelSceneController levelSceneController;

        [Header("Outcome Panels")]
        [SerializeField] private GameObject outcomeRoot;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Reward")]
        [SerializeField] private TMP_Text goldRewardText;

        [Header("Add Bottle")]
        [SerializeField] private GameObject addBottleButton;
        [SerializeField] private TMP_Text addBottleGoldCostText;

        private void OnEnable()
        {
            levelSceneController.OutcomeChanged += Show;
        }

        private void OnDisable()
        {
            levelSceneController.OutcomeChanged -= Show;
        }

        public void Show(LevelOutcome outcome)
        {
            bool isCompleted = outcome == LevelOutcome.Completed;
            bool isFailed = outcome == LevelOutcome.Failed;

            if (isCompleted)
            {
                goldRewardText.SetText($"+{levelSceneController.WinGoldReward}");
            }

            UpdateAddBottleOffer(isFailed);

            winPanel.SetActive(isCompleted);
            losePanel.SetActive(isFailed);
            outcomeRoot.SetActive(isCompleted || isFailed);
        }

        private void UpdateAddBottleOffer(bool isFailed)
        {
            bool isAvailable = isFailed && levelSceneController.HasAvailableBottleAddition;

            addBottleButton.SetActive(isAvailable);

            if (isAvailable)
            {
                addBottleGoldCostText.SetText(levelSceneController.AddBottleGoldCost.ToString());
            }
        }
    }
}
