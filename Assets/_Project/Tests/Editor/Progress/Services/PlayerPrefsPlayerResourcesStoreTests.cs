using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerPrefsPlayerResourcesStoreTests
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";
        private const string LivesKey =
            "WaterSortPuzzle.Progress.Lives";
        private const string NextLifeTimestampKey =
            "WaterSortPuzzle.Progress.NextLifeTimestamp";

        private bool hadGold;
        private bool hadLives;
        private bool hadNextLifeTimestamp;
        private int savedGold;
        private int savedLives;
        private string savedNextLifeTimestamp;

        [SetUp]
        public void SetUp()
        {
            hadGold = PlayerPrefs.HasKey(GoldKey);
            hadLives = PlayerPrefs.HasKey(LivesKey);
            hadNextLifeTimestamp = PlayerPrefs.HasKey(
                NextLifeTimestampKey);
            savedGold = PlayerPrefs.GetInt(GoldKey);
            savedLives = PlayerPrefs.GetInt(LivesKey);
            savedNextLifeTimestamp = PlayerPrefs.GetString(
                NextLifeTimestampKey);

            ClearPlayerPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            RestoreInt(GoldKey, hadGold, savedGold);
            RestoreInt(LivesKey, hadLives, savedLives);

            if (hadNextLifeTimestamp)
            {
                PlayerPrefs.SetString(
                    NextLifeTimestampKey,
                    savedNextLifeTimestamp);
            }
            else
            {
                PlayerPrefs.DeleteKey(NextLifeTimestampKey);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void Load_WithoutSavedValues_ReturnsDefaultSaveData()
        {
            PlayerResourcesSaveData saveData = CreateStore().Load();

            Assert.That(
                saveData.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                saveData.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(saveData.NextLifeTimestamp, Is.Zero);
        }

        [Test]
        public void Save_PersistsEveryResourceValue()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();

            store.Save(new PlayerResourcesSaveData(150, 3, 2500));
            PlayerResourcesSaveData saveData = CreateStore().Load();

            Assert.That(saveData.Gold, Is.EqualTo(150));
            Assert.That(saveData.Lives, Is.EqualTo(3));
            Assert.That(saveData.NextLifeTimestamp, Is.EqualTo(2500));
        }

        [Test]
        public void Save_WithoutSaveData_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => CreateStore().Save(null));
        }

        [Test]
        public void Load_WithInvalidSavedValues_ReturnsSafeValues()
        {
            PlayerPrefs.SetInt(GoldKey, -1);
            PlayerPrefs.SetInt(
                LivesKey,
                GameBalance.MaximumLives + 1);
            PlayerPrefs.SetString(NextLifeTimestampKey, "invalid");

            PlayerResourcesSaveData saveData = CreateStore().Load();

            Assert.That(saveData.Gold, Is.Zero);
            Assert.That(
                saveData.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(saveData.NextLifeTimestamp, Is.Zero);
        }

        private static PlayerPrefsPlayerResourcesStore CreateStore()
        {
            return new PlayerPrefsPlayerResourcesStore();
        }

        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(GoldKey);
            PlayerPrefs.DeleteKey(LivesKey);
            PlayerPrefs.DeleteKey(NextLifeTimestampKey);
        }

        private static void RestoreInt(
            string key,
            bool hadValue,
            int savedValue)
        {
            if (hadValue)
            {
                PlayerPrefs.SetInt(key, savedValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
