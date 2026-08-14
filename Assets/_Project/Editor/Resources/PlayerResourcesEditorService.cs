using System;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Editor.Progress
{
    public sealed class PlayerResourcesEditorService
    {
        private readonly IPlayerResourcesStore resourcesStore;
        private readonly PlayerResourcesService resourcesService;

        public PlayerResourcesEditorService(IPlayerResourcesStore resourcesStore)
        {
            this.resourcesStore = resourcesStore ?? throw new ArgumentNullException(nameof(resourcesStore));
            resourcesService = new PlayerResourcesService(resourcesStore);
        }

        public PlayerResources Load()
        {
            return resourcesService.Load();
        }

        public PlayerResources SetResources(int gold, int lives)
        {
            return SetResources(
                gold,
                lives,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources SetResources(
            int gold,
            int lives,
            long currentUnixTimeSeconds)
        {
            PlayerResourcesSaveData selectedSaveData = new PlayerResourcesSaveData(gold, lives, 0);

            PlayerResources current = resourcesService.Load(currentUnixTimeSeconds);

            long currentNextLifeTimestamp = current.Lives == GameBalance.MaximumLives
                    ? 0
                    : currentUnixTimeSeconds + current.SecondsUntilNextLife;

            long nextLifeTimestamp = GetNextLifeTimestamp(
                current.Lives,
                selectedSaveData.Lives,
                currentNextLifeTimestamp,
                currentUnixTimeSeconds);

            PlayerResourcesSaveData updatedSaveData = nextLifeTimestamp == selectedSaveData.NextLifeTimestamp
                    ? selectedSaveData
                    : new PlayerResourcesSaveData(
                        selectedSaveData.Gold,
                        selectedSaveData.Lives,
                        nextLifeTimestamp);

            resourcesStore.Save(updatedSaveData);

            return resourcesService.Load(currentUnixTimeSeconds);
        }

        public PlayerResources ResetLifeRefillTimer()
        {
            return ResetLifeRefillTimer(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources ResetLifeRefillTimer(long currentUnixTimeSeconds)
        {
            PlayerResources current = resourcesService.Load(currentUnixTimeSeconds);

            if (current.Lives == GameBalance.MaximumLives)
            {
                return current;
            }

            resourcesStore.Save(
                new PlayerResourcesSaveData(
                    current.Gold,
                    current.Lives,
                    currentUnixTimeSeconds +
                    GameBalance.LifeRefillDurationSeconds));

            return resourcesService.Load(currentUnixTimeSeconds);
        }

        public PlayerResources ResetToDefaults()
        {
            resourcesStore.Save(
                new PlayerResourcesSaveData(
                    GameBalance.InitialGold,
                    GameBalance.MaximumLives,
                    0));

            return resourcesService.Load();
        }

        private static long GetNextLifeTimestamp(
            int currentLives,
            int updatedLives,
            long currentNextLifeTimestamp,
            long currentUnixTimeSeconds)
        {
            if (updatedLives == GameBalance.MaximumLives)
            {
                return 0;
            }

            return currentLives == GameBalance.MaximumLives
                ? currentUnixTimeSeconds + GameBalance.LifeRefillDurationSeconds
                : currentNextLifeTimestamp;
        }
    }
}
