using System;
using System.Globalization;
using UnityEngine;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerPrefsPlayerResourcesStore
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";
        private const string LivesKey =
            "WaterSortPuzzle.Progress.Lives";
        private const string NextLifeTimestampKey =
            "WaterSortPuzzle.Progress.NextLifeTimestamp";

        public PlayerResources Load()
        {
            return Load(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources Load(long currentUnixTimeSeconds)
        {
            int savedGold = PlayerPrefs.GetInt(
                GoldKey,
                GameBalance.InitialGold);
            int savedLives = PlayerPrefs.GetInt(
                LivesKey,
                GameBalance.MaximumLives);
            int gold = Mathf.Max(GameBalance.InitialGold, savedGold);
            int lives = Mathf.Clamp(
                savedLives,
                0,
                GameBalance.MaximumLives);
            long nextLifeTimestamp = LoadNextLifeTimestamp();
            bool shouldSave = gold != savedGold || lives != savedLives;

            if (lives == GameBalance.MaximumLives)
            {
                shouldSave |= nextLifeTimestamp != 0;
                nextLifeTimestamp = 0;
            }
            else if (nextLifeTimestamp <= 0)
            {
                nextLifeTimestamp =
                    currentUnixTimeSeconds +
                    GameBalance.LifeRefillDurationSeconds;
                shouldSave = true;
            }
            else if (currentUnixTimeSeconds >= nextLifeTimestamp)
            {
                long elapsedRefills =
                    (currentUnixTimeSeconds - nextLifeTimestamp) /
                    GameBalance.LifeRefillDurationSeconds + 1;
                int restoredLives = (int)Math.Min(
                    GameBalance.MaximumLives - lives,
                    elapsedRefills);

                lives += restoredLives;

                nextLifeTimestamp = lives == GameBalance.MaximumLives
                    ? 0
                    : nextLifeTimestamp +
                      restoredLives *
                      GameBalance.LifeRefillDurationSeconds;
                shouldSave = true;
            }

            if (shouldSave)
            {
                Save(gold, lives, nextLifeTimestamp);
            }

            return CreateResources(
                gold,
                lives,
                nextLifeTimestamp,
                currentUnixTimeSeconds);
        }

        public PlayerResources AddGold(int amount)
        {
            return AddGold(
                amount,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources AddGold(
            int amount,
            long currentUnixTimeSeconds)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            PlayerResources current = Load(currentUnixTimeSeconds);
            int updatedGold = (int)Math.Min(
                int.MaxValue,
                (long)current.Gold + amount);
            long nextLifeTimestamp = LoadNextLifeTimestamp();

            Save(updatedGold, current.Lives, nextLifeTimestamp);

            return CreateResources(
                updatedGold,
                current.Lives,
                nextLifeTimestamp,
                currentUnixTimeSeconds);
        }

#if UNITY_EDITOR
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
            if (gold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gold));
            }

            if (lives < 0 || lives > GameBalance.MaximumLives)
            {
                throw new ArgumentOutOfRangeException(nameof(lives));
            }

            PlayerResources current = Load(currentUnixTimeSeconds);
            long nextLifeTimestamp = GetNextLifeTimestamp(
                current.Lives,
                lives,
                currentUnixTimeSeconds);

            Save(gold, lives, nextLifeTimestamp);

            return CreateResources(
                gold,
                lives,
                nextLifeTimestamp,
                currentUnixTimeSeconds);
        }
#endif

        public PlayerResources ConsumeLife()
        {
            return ConsumeLife(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources ConsumeLife(long currentUnixTimeSeconds)
        {
            PlayerResources current = Load(currentUnixTimeSeconds);

            if (current.Lives == 0)
            {
                return current;
            }

            int updatedLives = current.Lives - 1;

            long nextLifeTimestamp = current.Lives == GameBalance.MaximumLives
                ? currentUnixTimeSeconds + GameBalance.LifeRefillDurationSeconds
                : LoadNextLifeTimestamp();

            Save(current.Gold, updatedLives, nextLifeTimestamp);

            return CreateResources(
                current.Gold,
                updatedLives,
                nextLifeTimestamp,
                currentUnixTimeSeconds);
        }

#if UNITY_EDITOR
        public PlayerResources ResetLifeRefillTimer()
        {
            return ResetLifeRefillTimer(
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public PlayerResources ResetLifeRefillTimer(
            long currentUnixTimeSeconds)
        {
            PlayerResources current = Load(currentUnixTimeSeconds);

            if (current.Lives == GameBalance.MaximumLives)
            {
                return current;
            }

            long nextLifeTimestamp =
                currentUnixTimeSeconds +
                GameBalance.LifeRefillDurationSeconds;

            Save(current.Gold, current.Lives, nextLifeTimestamp);

            return CreateResources(
                current.Gold,
                current.Lives,
                nextLifeTimestamp,
                currentUnixTimeSeconds);
        }

        public PlayerResources ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(GoldKey);
            PlayerPrefs.DeleteKey(LivesKey);
            PlayerPrefs.DeleteKey(NextLifeTimestampKey);
            PlayerPrefs.Save();

            return new PlayerResources(
                GameBalance.InitialGold,
                GameBalance.MaximumLives,
                0);
        }
#endif

        private static PlayerResources CreateResources(
            int gold,
            int lives,
            long nextLifeTimestamp,
            long currentUnixTimeSeconds)
        {
            long remainingSeconds = lives == GameBalance.MaximumLives
                ? 0
                : Math.Max(0, nextLifeTimestamp - currentUnixTimeSeconds);

            return new PlayerResources(
                gold,
                lives,
                (int)Math.Min(int.MaxValue, remainingSeconds));
        }

        private static long LoadNextLifeTimestamp()
        {
            string savedValue = PlayerPrefs.GetString(
                NextLifeTimestampKey,
                string.Empty);

            return long.TryParse(
                savedValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long timestamp)
                ? timestamp
                : 0;
        }

#if UNITY_EDITOR
        private static long GetNextLifeTimestamp(
            int currentLives,
            int updatedLives,
            long currentUnixTimeSeconds)
        {
            if (updatedLives == GameBalance.MaximumLives)
            {
                return 0;
            }

            return currentLives == GameBalance.MaximumLives
                ? currentUnixTimeSeconds +
                  GameBalance.LifeRefillDurationSeconds
                : LoadNextLifeTimestamp();
        }
#endif

        private static void Save(
            int gold,
            int lives,
            long nextLifeTimestamp)
        {
            PlayerPrefs.SetInt(GoldKey, gold);
            PlayerPrefs.SetInt(LivesKey, lives);

            if (nextLifeTimestamp > 0)
            {
                PlayerPrefs.SetString(
                    NextLifeTimestampKey,
                    nextLifeTimestamp.ToString(
                        CultureInfo.InvariantCulture));
            }
            else
            {
                PlayerPrefs.DeleteKey(NextLifeTimestampKey);
            }

            PlayerPrefs.Save();
        }
    }
}
