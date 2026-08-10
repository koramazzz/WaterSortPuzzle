using System;
using UnityEngine;

namespace WaterSortPuzzle.Progress
{
    public sealed class PlayerPrefsGoldStore
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";
        private const int InitialGold = 0;

        public int LoadGold()
        {
            return Mathf.Max(
                InitialGold,
                PlayerPrefs.GetInt(GoldKey, InitialGold));
        }

        public int AddGold(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            int updatedGold = (int)Math.Min(
                int.MaxValue,
                (long)LoadGold() + amount);

            PlayerPrefs.SetInt(GoldKey, updatedGold);
            PlayerPrefs.Save();

            return updatedGold;
        }
    }
}
