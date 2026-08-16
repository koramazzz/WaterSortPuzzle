using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Gameplay.Levels.Presentation;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Levels.Presentation
{
    public sealed class LevelOutcomeViewTests
    {
        private GameObject controllerObject;
        private GameObject outcomeRoot;
        private GameObject winPanel;
        private GameObject losePanel;
        private GameObject addBottleButton;
        private GameObject goldRewardTextObject;
        private TMP_Text goldRewardText;
        private GameObject addBottleGoldCostTextObject;
        private TMP_Text addBottleGoldCostText;
        private LevelOutcomeView outcomeView;

        [SetUp]
        public void SetUp()
        {
            controllerObject = new GameObject("LevelController");
            controllerObject.SetActive(false);
            LevelSceneController levelSceneController =
                controllerObject.AddComponent<LevelSceneController>();
            outcomeView = controllerObject.AddComponent<LevelOutcomeView>();
            outcomeRoot = new GameObject("Outcome");
            winPanel = new GameObject("WinPanel");
            losePanel = new GameObject("LosePanel");
            addBottleButton = new GameObject("AddBottleButton");
            goldRewardTextObject = new GameObject(
                "GoldRewardText",
                typeof(RectTransform),
                typeof(CanvasRenderer));
            goldRewardText =
                goldRewardTextObject.AddComponent<TextMeshProUGUI>();
            addBottleGoldCostTextObject = new GameObject(
                "AddBottleGoldCostText",
                typeof(RectTransform),
                typeof(CanvasRenderer));
            addBottleGoldCostText =
                addBottleGoldCostTextObject.AddComponent<TextMeshProUGUI>();

            SerializedObject serializedView = new SerializedObject(outcomeView);
            serializedView.FindProperty("levelSceneController")
                .objectReferenceValue = levelSceneController;
            serializedView.FindProperty("outcomeRoot").objectReferenceValue =
                outcomeRoot;
            serializedView.FindProperty("winPanel").objectReferenceValue =
                winPanel;
            serializedView.FindProperty("losePanel").objectReferenceValue =
                losePanel;
            serializedView.FindProperty("goldRewardText")
                .objectReferenceValue = goldRewardText;
            serializedView.FindProperty("addBottleButton")
                .objectReferenceValue = addBottleButton;
            serializedView.FindProperty("addBottleGoldCostText")
                .objectReferenceValue = addBottleGoldCostText;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            controllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(outcomeRoot);
            Object.DestroyImmediate(winPanel);
            Object.DestroyImmediate(losePanel);
            Object.DestroyImmediate(addBottleButton);
            Object.DestroyImmediate(goldRewardTextObject);
            Object.DestroyImmediate(addBottleGoldCostTextObject);
        }

        [TestCase(LevelOutcome.InProgress, false, false, false)]
        [TestCase(LevelOutcome.Completed, true, true, false)]
        [TestCase(LevelOutcome.Failed, true, false, true)]
        public void Show_WithOutcome_UpdatesPanelVisibility(
            LevelOutcome outcome,
            bool expectedRootVisibility,
            bool expectedWinVisibility,
            bool expectedLoseVisibility)
        {
            outcomeRoot.SetActive(true);
            winPanel.SetActive(true);
            losePanel.SetActive(true);

            outcomeView.Show(outcome);

            Assert.That(
                outcomeRoot.activeSelf,
                Is.EqualTo(expectedRootVisibility));
            Assert.That(
                winPanel.activeSelf,
                Is.EqualTo(expectedWinVisibility));
            Assert.That(
                losePanel.activeSelf,
                Is.EqualTo(expectedLoseVisibility));
            Assert.That(
                addBottleButton.activeSelf,
                Is.EqualTo(expectedLoseVisibility));

            if (outcome == LevelOutcome.Completed)
            {
                Assert.That(
                    goldRewardText.text,
                    Is.EqualTo(
                        $"+{GameBalance.BaseGoldReward}"));
            }

            if (outcome == LevelOutcome.Failed)
            {
                Assert.That(
                    addBottleGoldCostText.text,
                    Is.EqualTo(
                        GameBalance
                            .AddBottleGoldCost
                            .ToString()));
            }
        }
    }
}
