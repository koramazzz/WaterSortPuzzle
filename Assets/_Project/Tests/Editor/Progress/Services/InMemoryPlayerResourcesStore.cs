using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class InMemoryPlayerResourcesStore :
        IPlayerResourcesStore
    {
        public InMemoryPlayerResourcesStore(
            PlayerResourcesSaveData saveData)
        {
            SaveData = saveData;
        }

        public PlayerResourcesSaveData SaveData { get; private set; }

        public int SaveCount { get; private set; }

        public PlayerResourcesSaveData Load()
        {
            return SaveData;
        }

        public void Save(PlayerResourcesSaveData saveData)
        {
            SaveData = saveData;
            SaveCount++;
        }
    }
}
