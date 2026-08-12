using System;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Editor.Progress
{
    public sealed class PlayerResourcesEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Player Resources";
        private const string MenuPath = DeveloperToolsMenu.RootPath + "/" + WindowTitle;
        private const int MinimumGold = 0;
        private const int MinimumLives = 0;

        private readonly PlayerPrefsPlayerResourcesStore resourcesStore = new PlayerPrefsPlayerResourcesStore();

        private int selectedGold;
        private int selectedLives;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            PlayerResourcesEditorWindow window = GetWindow<PlayerResourcesEditorWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            SynchronizeSelection(resourcesStore.Load());
        }

        private void OnGUI()
        {
            PlayerResources savedResources = resourcesStore.Load();

            DrawSavedState(savedResources);
            EditorGUILayout.Space();
            DrawSelectedState(savedResources);
            EditorGUILayout.Space();
            DrawLifeRefillActions(savedResources);
            EditorGUILayout.Space();
            DrawResetAction();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void DrawSavedState(PlayerResources savedResources)
        {
            EditorGUILayout.LabelField("Saved State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Gold",
                savedResources.Gold.ToString());
            EditorGUILayout.LabelField(
                "Lives",
                savedResources.Lives.ToString());
            EditorGUILayout.LabelField(
                "Next Life",
                GetLifeRefillLabel(savedResources));
        }

        private void DrawSelectedState(PlayerResources savedResources)
        {
            EditorGUILayout.LabelField("Selected State", EditorStyles.boldLabel);

            selectedGold = Mathf.Max(MinimumGold, EditorGUILayout.IntField("Gold", selectedGold));

            selectedLives = EditorGUILayout.IntSlider("Lives", selectedLives, MinimumLives, GameBalance.MaximumLives);

            bool hasChanges =
                selectedGold != savedResources.Gold ||
                selectedLives != savedResources.Lives;

            using (new EditorGUI.DisabledScope(!hasChanges))
            {
                if (GUILayout.Button("Save Resources"))
                {
                    PlayerResources updatedResources =
                        resourcesStore.SetResources(
                            selectedGold,
                            selectedLives);

                    SynchronizeSelection(updatedResources);
                    ShowNotification(new GUIContent("Saved resources"));
                }
            }
        }

        private void DrawLifeRefillActions(PlayerResources savedResources)
        {
            EditorGUILayout.LabelField(
                "Life Refill",
                EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(
                       savedResources.Lives == GameBalance.MaximumLives))
            {
                if (GUILayout.Button("Reset Life Refill Timer"))
                {
                    PlayerResources updatedResources =
                        resourcesStore.ResetLifeRefillTimer();

                    SynchronizeSelection(updatedResources);
                    ShowNotification(
                        new GUIContent("Reset life refill timer"));
                }
            }
        }

        private void DrawResetAction()
        {
            EditorGUILayout.LabelField("Defaults", EditorStyles.boldLabel);

            if (!GUILayout.Button("Reset Resources to Defaults"))
            {
                return;
            }

            bool shouldReset = EditorUtility.DisplayDialog(
                "Reset Player Resources",
                "Reset saved gold, lives, and the life refill timer?",
                "Reset",
                "Cancel");

            if (!shouldReset)
            {
                return;
            }

            PlayerResources defaultResources =
                resourcesStore.ResetToDefaults();

            SynchronizeSelection(defaultResources);
            ShowNotification(new GUIContent("Reset resources"));
        }

        private void SynchronizeSelection(PlayerResources resources)
        {
            selectedGold = resources.Gold;
            selectedLives = resources.Lives;
        }

        private static string GetLifeRefillLabel(
            PlayerResources resources)
        {
            return resources.Lives == GameBalance.MaximumLives
                ? "Stopped"
                : TimeSpan
                    .FromSeconds(resources.SecondsUntilNextLife)
                    .ToString("c");
        }
    }
}
