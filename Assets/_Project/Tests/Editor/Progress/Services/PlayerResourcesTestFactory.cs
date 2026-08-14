using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    internal static class PlayerResourcesTestFactory
    {
        internal static InMemoryPlayerResourcesStore CreateDefaultStore()
        {
            return CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives,
                0);
        }

        internal static InMemoryPlayerResourcesStore CreateStore(
            int gold,
            int lives,
            long nextLifeTimestamp)
        {
            return new InMemoryPlayerResourcesStore(
                new PlayerResourcesSaveData(
                    gold,
                    lives,
                    nextLifeTimestamp));
        }
    }
}
