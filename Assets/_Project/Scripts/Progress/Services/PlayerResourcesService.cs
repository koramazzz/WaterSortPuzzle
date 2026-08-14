using System;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerResourcesService
    {
        private readonly IPlayerResourcesStore resourcesStore;

        public PlayerResourcesService(IPlayerResourcesStore resourcesStore)
        {
            this.resourcesStore = resourcesStore ?? throw new ArgumentNullException(nameof(resourcesStore));
        }

        public PlayerResources Load()
        {
            return Load(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources Load(long currentUnixTimeSeconds)
        {
            PlayerResourcesSaveData saveData = RefreshLifeRefill(currentUnixTimeSeconds);

            return CreateResources(saveData, currentUnixTimeSeconds);
        }

        public PlayerResources AddGold(int amount)
        {
            return AddGold(amount, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources AddGold(int amount, long currentUnixTimeSeconds)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            PlayerResourcesSaveData current = RefreshLifeRefill(currentUnixTimeSeconds);

            int updatedGold = (int)Math.Min(int.MaxValue, (long)current.Gold + amount);

            PlayerResourcesSaveData updated = new PlayerResourcesSaveData(
                    updatedGold,
                    current.Lives,
                    current.NextLifeTimestamp);

            resourcesStore.Save(updated);

            return CreateResources(updated, currentUnixTimeSeconds);
        }

        public bool TrySpendGold(int amount, out PlayerResources resources)
        {
            return TrySpendGold(amount, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), out resources);
        }

        public bool TrySpendGold(int amount, long currentUnixTimeSeconds, out PlayerResources resources)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            PlayerResourcesSaveData current = RefreshLifeRefill(currentUnixTimeSeconds);

            if (current.Gold < amount)
            {
                resources = CreateResources(current, currentUnixTimeSeconds);
                return false;
            }

            PlayerResourcesSaveData updated = new PlayerResourcesSaveData(
                    current.Gold - amount,
                    current.Lives,
                    current.NextLifeTimestamp);

            resourcesStore.Save(updated);
            resources = CreateResources(updated, currentUnixTimeSeconds);
            return true;
        }

        public bool TryConsumeLife(out PlayerResources resources)
        {
            return TryConsumeLife(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), out resources);
        }

        public bool TryConsumeLife(long currentUnixTimeSeconds, out PlayerResources resources)
        {
            PlayerResourcesSaveData current = RefreshLifeRefill(currentUnixTimeSeconds);

            if (current.Lives == GameBalance.MinimumLives)
            {
                resources = CreateResources(current, currentUnixTimeSeconds);
                return false;
            }

            int updatedLives = current.Lives - 1;

            long nextLifeTimestamp = current.Lives == GameBalance.MaximumLives
                    ? currentUnixTimeSeconds + GameBalance.LifeRefillDurationSeconds
                    : current.NextLifeTimestamp;

            PlayerResourcesSaveData updated = new PlayerResourcesSaveData(
                    current.Gold,
                    updatedLives,
                    nextLifeTimestamp);

            resourcesStore.Save(updated);
            resources = CreateResources(updated, currentUnixTimeSeconds);
            return true;
        }

        private PlayerResourcesSaveData RefreshLifeRefill(long currentUnixTimeSeconds)
        {
            PlayerResourcesSaveData current = resourcesStore.Load();
            int lives = current.Lives;
            long nextLifeTimestamp = current.NextLifeTimestamp;
            bool shouldSave = false;

            if (lives == GameBalance.MaximumLives)
            {
                shouldSave = nextLifeTimestamp != 0;
                nextLifeTimestamp = 0;
            }
            else if (nextLifeTimestamp <= 0)
            {
                nextLifeTimestamp = currentUnixTimeSeconds + GameBalance.LifeRefillDurationSeconds;
                shouldSave = true;
            }
            else if (currentUnixTimeSeconds >= nextLifeTimestamp)
            {
                long elapsedRefills = (currentUnixTimeSeconds - nextLifeTimestamp) /
                    GameBalance.LifeRefillDurationSeconds + 1;

                int restoredLives = (int)Math.Min(GameBalance.MaximumLives - lives, elapsedRefills);
                lives += restoredLives;

                nextLifeTimestamp = lives == GameBalance.MaximumLives
                    ? 0
                    : nextLifeTimestamp + restoredLives * GameBalance.LifeRefillDurationSeconds;

                shouldSave = true;
            }

            if (!shouldSave)
            {
                return current;
            }

            PlayerResourcesSaveData updated = new PlayerResourcesSaveData(
                    current.Gold,
                    lives,
                    nextLifeTimestamp);

            resourcesStore.Save(updated);
            return updated;
        }

        private static PlayerResources CreateResources(
            PlayerResourcesSaveData saveData,
            long currentUnixTimeSeconds)
        {
            long remainingSeconds = saveData.Lives == GameBalance.MaximumLives
                    ? 0
                    : Math.Max(0, saveData.NextLifeTimestamp - currentUnixTimeSeconds);

            return new PlayerResources(
                saveData.Gold,
                saveData.Lives,
                (int)Math.Min(int.MaxValue, remainingSeconds));
        }
    }
}
