using System;
using System.Globalization;
using UnityEngine;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerPrefsPlayerResourcesStore :
        IPlayerResourcesStore
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";
        private const string LivesKey =
            "WaterSortPuzzle.Progress.Lives";
        private const string NextLifeTimestampKey =
            "WaterSortPuzzle.Progress.NextLifeTimestamp";

        public PlayerResourcesSaveData Load()
        {
            int gold = Mathf.Max(
                GameBalance.MinimumGold,
                PlayerPrefs.GetInt(
                    GoldKey,
                    GameBalance.InitialGold));

            int lives = Mathf.Clamp(
                PlayerPrefs.GetInt(
                    LivesKey,
                    GameBalance.MaximumLives),
                GameBalance.MinimumLives,
                GameBalance.MaximumLives);
            long nextLifeTimestamp = LoadNextLifeTimestamp();

            return new PlayerResourcesSaveData(
                gold,
                lives,
                nextLifeTimestamp);
        }

        public void Save(PlayerResourcesSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            PlayerPrefs.SetInt(GoldKey, saveData.Gold);
            PlayerPrefs.SetInt(LivesKey, saveData.Lives);

            if (saveData.NextLifeTimestamp > 0)
            {
                PlayerPrefs.SetString(
                    NextLifeTimestampKey,
                    saveData.NextLifeTimestamp.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                PlayerPrefs.DeleteKey(NextLifeTimestampKey);
            }

            PlayerPrefs.Save();
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
                   && timestamp > 0
                ? timestamp
                : 0;
        }
    }
}
