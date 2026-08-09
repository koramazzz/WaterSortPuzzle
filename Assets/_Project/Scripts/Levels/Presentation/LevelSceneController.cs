using TMPro;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Gameplay.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Gameplay.Levels.Presentation
{
    public sealed class LevelSceneController : MonoBehaviour
    {
        [SerializeField] private LevelFileCatalog levelCatalog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private string levelTitleFormat;
        [SerializeField] private BottleCollectionView bottleCollectionView;

        private readonly LevelCatalogLoader levelCatalogLoader = new LevelCatalogLoader();
        private readonly PlayerPrefsLevelProgressStore progressStore = new PlayerPrefsLevelProgressStore();
        private readonly BottleInteractionPresenter bottleInteractionPresenter = new BottleInteractionPresenter();

        private void Start()
        {
            int levelCount = levelCatalog.LevelFiles.Count;
            int completedLevelCount = progressStore.LoadCompletedLevelCount(levelCount);

            if (!levelCatalogLoader.TryLoad(
                    levelCatalog,
                    completedLevelCount,
                    out LevelState levelState))
            {
                levelText.gameObject.SetActive(false);
                return;
            }

            levelText.SetText(levelTitleFormat, levelState.LevelNumber);
            bottleCollectionView.Initialize(levelState.Bottles);
            bottleInteractionPresenter.Initialize(bottleCollectionView);
        }
    }
}
