using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Editor.Levels
{
    public sealed class LevelProgressEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Level Progress";
        private const string MenuPath = DeveloperToolsMenu.RootPath + "/" + WindowTitle;
        private const string CatalogSearchFilter = "t:LevelFileCatalog";
        private const int FirstLevelIndex = 0;
        private const int FirstLevelNumber = 1;

        private readonly PlayerPrefsLevelProgressStore progressStore =
            new PlayerPrefsLevelProgressStore();

        private LevelFileCatalog levelCatalog;
        private int selectedCompletedLevelCount;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            LevelProgressEditorWindow window =
                GetWindow<LevelProgressEditorWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            levelCatalog = FindLevelCatalog();

            if (levelCatalog != null && levelCatalog.LevelFiles.Count > 0)
            {
                selectedCompletedLevelCount =
                    progressStore.LoadCompletedLevelCount(
                        levelCatalog.LevelFiles.Count);
            }
        }

        private void OnGUI()
        {
            if (levelCatalog == null)
            {
                EditorGUILayout.HelpBox(
                    "The project must contain exactly one Level File Catalog.",
                    MessageType.Error);
                return;
            }

            int levelCount = levelCatalog.LevelFiles.Count;

            if (levelCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "The Level File Catalog does not contain any levels.",
                    MessageType.Warning);
                return;
            }

            int savedCompletedLevelCount =
                progressStore.LoadCompletedLevelCount(levelCount);

            selectedCompletedLevelCount = Mathf.Clamp(
                selectedCompletedLevelCount,
                FirstLevelIndex,
                levelCount);

            EditorGUILayout.LabelField(
                "Saved State",
                GetLevelLabel(savedCompletedLevelCount, levelCount));
            EditorGUILayout.Space();

            selectedCompletedLevelCount = EditorGUILayout.IntSlider(
                "Selected State",
                selectedCompletedLevelCount,
                FirstLevelIndex,
                levelCount);

            EditorGUILayout.HelpBox(
                GetLevelLabel(selectedCompletedLevelCount, levelCount),
                MessageType.Info);

            using (new EditorGUI.DisabledScope(
                       selectedCompletedLevelCount ==
                       savedCompletedLevelCount))
            {
                if (GUILayout.Button("Save Level"))
                {
                    progressStore.SaveCompletedLevelCount(
                        selectedCompletedLevelCount,
                        levelCount);

                    string selectedLevelLabel = GetLevelLabel(
                        selectedCompletedLevelCount,
                        levelCount);
                    string notification = $"Saved {selectedLevelLabel}";

                    ShowNotification(new GUIContent(notification));
                }
            }
        }

        private static LevelFileCatalog FindLevelCatalog()
        {
            string[] catalogGuids =
                AssetDatabase.FindAssets(CatalogSearchFilter);

            if (catalogGuids.Length != 1)
            {
                return null;
            }

            string catalogPath =
                AssetDatabase.GUIDToAssetPath(catalogGuids[0]);

            return AssetDatabase.LoadAssetAtPath<LevelFileCatalog>(
                catalogPath);
        }

        private static string GetLevelLabel(
            int completedLevelCount,
            int levelCount)
        {
            return completedLevelCount >= levelCount
                ? "Finished"
                : $"Level {completedLevelCount + FirstLevelNumber}";
        }
    }
}
