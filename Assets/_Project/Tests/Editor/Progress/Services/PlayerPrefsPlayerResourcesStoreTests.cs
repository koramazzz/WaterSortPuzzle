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
        private const long CurrentTime = 1000;

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
            hadNextLifeTimestamp = PlayerPrefs.HasKey(NextLifeTimestampKey);
            savedGold = PlayerPrefs.GetInt(GoldKey);
            savedLives = PlayerPrefs.GetInt(LivesKey);
            savedNextLifeTimestamp = PlayerPrefs.GetString(
                NextLifeTimestampKey);

            PlayerPrefs.DeleteKey(GoldKey);
            PlayerPrefs.DeleteKey(LivesKey);
            PlayerPrefs.DeleteKey(NextLifeTimestampKey);
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
        public void Load_WithoutSavedValues_ReturnsInitialResources()
        {
            PlayerResources resources = CreateStore().Load(CurrentTime);

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
        }

        [Test]
        public void AddGold_PersistsUpdatedGold()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();

            store.AddGold(50, CurrentTime);
            PlayerResources resources = CreateStore().Load(CurrentTime);

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold + 50));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void AddGold_WithNonPositiveAmount_Throws(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateStore().AddGold(amount, CurrentTime));
        }

        [Test]
        public void SetResources_WithValidValues_PersistsResources()
        {
            PlayerPrefsPlayerResourcesStore writer = CreateStore();

            writer.SetResources(150, 3, CurrentTime);
            PlayerResources resources = CreateStore().Load(CurrentTime);

            Assert.That(resources.Gold, Is.EqualTo(150));
            Assert.That(resources.Lives, Is.EqualTo(3));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void SetResources_WhileRefilling_PreservesRefillTimer()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.ConsumeLife(CurrentTime);

            PlayerResources resources = store.SetResources(
                150,
                GameBalance.MaximumLives - 2,
                CurrentTime + 60);

            Assert.That(resources.Gold, Is.EqualTo(150));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 2));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(
                    GameBalance.LifeRefillDurationSeconds - 60));
        }

        [Test]
        public void SetResources_ToMaximumLives_ClearsRefillTimer()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.ConsumeLife(CurrentTime);

            PlayerResources resources = store.SetResources(
                150,
                GameBalance.MaximumLives,
                CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(PlayerPrefs.HasKey(NextLifeTimestampKey), Is.False);
        }

        [Test]
        public void SetResources_WithNegativeGold_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateStore().SetResources(
                    -1,
                    GameBalance.MaximumLives,
                    CurrentTime));
        }

        [TestCase(-1)]
        [TestCase(GameBalance.MaximumLives + 1)]
        public void SetResources_WithOutOfRangeLives_Throws(int lives)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateStore().SetResources(
                    GameBalance.InitialGold,
                    lives,
                    CurrentTime));
        }

        [Test]
        public void ConsumeLife_FromMaximum_StartsRefillTimer()
        {
            PlayerResources resources = CreateStore().ConsumeLife(CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(
                    GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(
                    GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void ConsumeLife_WhileRefilling_DoesNotRestartTimer()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.ConsumeLife(CurrentTime);

            PlayerResources resources = store.ConsumeLife(CurrentTime + 60);

            Assert.That(
                resources.Lives,
                Is.EqualTo(
                    GameBalance.MaximumLives - 2));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(
                    GameBalance.LifeRefillDurationSeconds -
                    60));
        }

        [Test]
        public void Load_AfterRefillDuration_RestoresOneLife()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.ConsumeLife(CurrentTime);
            store.ConsumeLife(CurrentTime);

            PlayerResources resources = store.Load(
                CurrentTime +
                GameBalance.LifeRefillDurationSeconds);

            Assert.That(
                resources.Lives,
                Is.EqualTo(
                    GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(
                    GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void Load_AfterEnoughTime_RestoresLivesToMaximum()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();

            for (int index = 0;
                 index < GameBalance.MaximumLives;
                 index++)
            {
                store.ConsumeLife(CurrentTime);
            }

            PlayerResources resources = store.Load(
                CurrentTime +
                GameBalance.MaximumLives *
                GameBalance.LifeRefillDurationSeconds);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
        }

        [Test]
        public void ConsumeLife_WithoutLives_DoesNotGoBelowZero()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();

            for (int index = 0;
                 index <= GameBalance.MaximumLives;
                 index++)
            {
                store.ConsumeLife(CurrentTime);
            }

            PlayerResources resources = store.Load(CurrentTime);

            Assert.That(resources.Lives, Is.Zero);
        }

        [Test]
        public void ResetLifeRefillTimer_WhileRefilling_RestartsTimer()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.ConsumeLife(CurrentTime);

            PlayerResources resources = store.ResetLifeRefillTimer(
                CurrentTime + 60);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void ResetLifeRefillTimer_WithMaximumLives_KeepsTimerStopped()
        {
            PlayerResources resources = CreateStore().ResetLifeRefillTimer(
                CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(PlayerPrefs.HasKey(NextLifeTimestampKey), Is.False);
        }

        [Test]
        public void ResetToDefaults_RemovesSavedResources()
        {
            PlayerPrefsPlayerResourcesStore store = CreateStore();
            store.SetResources(150, 2, CurrentTime);

            PlayerResources resources = store.ResetToDefaults();

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(PlayerPrefs.HasKey(GoldKey), Is.False);
            Assert.That(PlayerPrefs.HasKey(LivesKey), Is.False);
            Assert.That(PlayerPrefs.HasKey(NextLifeTimestampKey), Is.False);
        }

        private static PlayerPrefsPlayerResourcesStore CreateStore()
        {
            return new PlayerPrefsPlayerResourcesStore();
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
