using TMPro;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Levels.Presentation
{
    public sealed class LevelOutcomeView : MonoBehaviour
    {
        [SerializeField] private LevelSceneController levelSceneController;
        [SerializeField] private GameObject outcomeRoot;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TMP_Text goldRewardText;

        private void OnEnable()
        {
            levelSceneController.LevelEnded += Show;
        }

        private void OnDisable()
        {
            levelSceneController.LevelEnded -= Show;
        }

        public void Show(LevelOutcome outcome)
        {
            bool isCompleted = outcome == LevelOutcome.Completed;
            bool isFailed = outcome == LevelOutcome.Failed;

            if (isCompleted)
            {
                goldRewardText.SetText(
                    $"+{levelSceneController.WinGoldReward}");
            }

            winPanel.SetActive(isCompleted);
            losePanel.SetActive(isFailed);
            outcomeRoot.SetActive(isCompleted || isFailed);
        }
    }
}
