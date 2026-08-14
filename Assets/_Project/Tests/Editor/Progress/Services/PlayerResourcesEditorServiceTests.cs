using System;
using NUnit.Framework;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Editor.Progress;
using WaterSortPuzzle.Progress;
using static WaterSortPuzzle.Tests.EditMode.Progress.PlayerResourcesTestFactory;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerResourcesEditorServiceTests
    {
        private const long CurrentTime = 1000;

        [Test]
        public void Constructor_WithoutStore_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PlayerResourcesEditorService(null));
        }

        [Test]
        public void SetResources_WithValidValues_PersistsResources()
        {
            InMemoryPlayerResourcesStore store = CreateDefaultStore();

            PlayerResources resources = CreateService(store).SetResources(
                150,
                3,
                CurrentTime);

            Assert.That(resources.Gold, Is.EqualTo(150));
            Assert.That(resources.Lives, Is.EqualTo(3));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
            Assert.That(store.SaveData.Gold, Is.EqualTo(150));
            Assert.That(store.SaveData.Lives, Is.EqualTo(3));
        }

        [Test]
        public void SetResources_WhileRefilling_PreservesRefillTimer()
        {
            long nextLifeTimestamp =
                CurrentTime + GameBalance.LifeRefillDurationSeconds;
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 1,
                nextLifeTimestamp);

            PlayerResources resources = CreateService(store).SetResources(
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
            Assert.That(
                store.SaveData.NextLifeTimestamp,
                Is.EqualTo(nextLifeTimestamp));
        }

        [Test]
        public void SetResources_ToMaximumLives_ClearsRefillTimer()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 1,
                CurrentTime + GameBalance.LifeRefillDurationSeconds);

            PlayerResources resources = CreateService(store).SetResources(
                150,
                GameBalance.MaximumLives,
                CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(store.SaveData.NextLifeTimestamp, Is.Zero);
        }

        [Test]
        public void SetResources_WithNegativeGold_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateService().SetResources(
                    -1,
                    GameBalance.MaximumLives,
                    CurrentTime));
        }

        [TestCase(-1)]
        [TestCase(GameBalance.MaximumLives + 1)]
        public void SetResources_WithOutOfRangeLives_Throws(int lives)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateService().SetResources(
                    GameBalance.InitialGold,
                    lives,
                    CurrentTime));
        }

        [Test]
        public void ResetLifeRefillTimer_WhileRefilling_RestartsTimer()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 1,
                CurrentTime + 60);

            PlayerResources resources = CreateService(store)
                .ResetLifeRefillTimer(CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
            Assert.That(
                store.SaveData.NextLifeTimestamp,
                Is.EqualTo(
                    CurrentTime +
                    GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void ResetLifeRefillTimer_WithMaximumLives_KeepsTimerStopped()
        {
            InMemoryPlayerResourcesStore store = CreateDefaultStore();

            PlayerResources resources = CreateService(store)
                .ResetLifeRefillTimer(CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(store.SaveCount, Is.Zero);
        }

        [Test]
        public void ResetToDefaults_PersistsDefaultResources()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                150,
                2,
                CurrentTime + 60);

            PlayerResources resources =
                CreateService(store).ResetToDefaults();

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(
                store.SaveData.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                store.SaveData.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(
                store.SaveData.NextLifeTimestamp,
                Is.Zero);
        }

        private static PlayerResourcesEditorService CreateService()
        {
            return CreateService(CreateDefaultStore());
        }

        private static PlayerResourcesEditorService CreateService(
            IPlayerResourcesStore store)
        {
            return new PlayerResourcesEditorService(store);
        }
    }
}
