namespace WaterSortPuzzle.Progress
{
    public interface IPlayerResourcesStore
    {
        PlayerResourcesSaveData Load();

        void Save(PlayerResourcesSaveData saveData);
    }
}
