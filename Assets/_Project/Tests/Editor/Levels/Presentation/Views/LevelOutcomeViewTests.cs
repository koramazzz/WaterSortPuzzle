using NUnit.Framework;
using UnityEditor;
using UnityEngine;
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

            SerializedObject serializedView = new SerializedObject(outcomeView);
            serializedView.FindProperty("levelSceneController")
                .objectReferenceValue = levelSceneController;
            serializedView.FindProperty("outcomeRoot").objectReferenceValue =
                outcomeRoot;
            serializedView.FindProperty("winPanel").objectReferenceValue =
                winPanel;
            serializedView.FindProperty("losePanel").objectReferenceValue =
                losePanel;
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
        }
    }
}
