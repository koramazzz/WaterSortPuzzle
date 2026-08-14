using System;
using NUnit.Framework;
using WaterSortPuzzle.Configuration;
using WaterSortPuzzle.Progress;
using static WaterSortPuzzle.Tests.EditMode.Progress.PlayerResourcesTestFactory;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerResourcesServiceTests
    {
        private const long CurrentTime = 1000;

        [Test]
        public void Constructor_WithoutStore_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PlayerResourcesService(null));
        }

        [Test]
        public void Load_WithDefaultSaveData_ReturnsInitialResources()
        {
            PlayerResources resources = CreateService().Load(CurrentTime);

            Assert.That(
                resources.Gold,
                Is.EqualTo(GameBalance.InitialGold));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
        }

        [Test]
        public void Load_WithoutRefillTimer_StartsRefillTimer()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 1,
                0);

            PlayerResources resources =
                CreateService(store).Load(CurrentTime);

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
        public void Load_WithMaximumLives_ClearsRefillTimer()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives,
                CurrentTime + 100);

            PlayerResources resources =
                CreateService(store).Load(CurrentTime);

            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
            Assert.That(store.SaveData.NextLifeTimestamp, Is.Zero);
        }

        [Test]
        public void Load_AfterRefillDuration_RestoresOneLife()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 2,
                CurrentTime);

            PlayerResources resources = CreateService(store).Load(
                CurrentTime);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void Load_AfterEnoughTime_RestoresLivesToMaximum()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                0,
                CurrentTime);

            PlayerResources resources = CreateService(store).Load(
                CurrentTime +
                GameBalance.MaximumLives *
                GameBalance.LifeRefillDurationSeconds);

            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives));
            Assert.That(resources.SecondsUntilNextLife, Is.Zero);
        }

        [Test]
        public void AddGold_PersistsUpdatedGold()
        {
            InMemoryPlayerResourcesStore store = CreateDefaultStore();

            PlayerResources resources = CreateService(store).AddGold(
                50,
                CurrentTime);

            int expectedGold = GameBalance.InitialGold + 50;
            Assert.That(resources.Gold, Is.EqualTo(expectedGold));
            Assert.That(store.SaveData.Gold, Is.EqualTo(expectedGold));
        }

        [Test]
        public void AddGold_AboveMaximumValue_ClampsGold()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                int.MaxValue,
                GameBalance.MaximumLives,
                0);

            PlayerResources resources = CreateService(store).AddGold(
                1,
                CurrentTime);

            Assert.That(resources.Gold, Is.EqualTo(int.MaxValue));
            Assert.That(store.SaveData.Gold, Is.EqualTo(int.MaxValue));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void AddGold_WithNonPositiveAmount_Throws(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateService().AddGold(amount, CurrentTime));
        }

        [Test]
        public void TrySpendGold_WithEnoughGold_PersistsReducedGold()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                100,
                GameBalance.MaximumLives,
                0);

            bool spent = CreateService(store).TrySpendGold(
                60,
                CurrentTime,
                out PlayerResources resources);

            Assert.That(spent, Is.True);
            Assert.That(resources.Gold, Is.EqualTo(40));
            Assert.That(store.SaveData.Gold, Is.EqualTo(40));
        }

        [Test]
        public void TrySpendGold_WithoutEnoughGold_DoesNotChangeGold()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                50,
                GameBalance.MaximumLives,
                0);

            bool spent = CreateService(store).TrySpendGold(
                60,
                CurrentTime,
                out PlayerResources resources);

            Assert.That(spent, Is.False);
            Assert.That(resources.Gold, Is.EqualTo(50));
            Assert.That(store.SaveData.Gold, Is.EqualTo(50));
            Assert.That(store.SaveCount, Is.Zero);
        }

        [Test]
        public void TrySpendGold_WhileRefilling_PreservesLifeTimer()
        {
            long nextLifeTimestamp =
                CurrentTime + GameBalance.LifeRefillDurationSeconds;
            InMemoryPlayerResourcesStore store = CreateStore(
                100,
                GameBalance.MaximumLives - 1,
                nextLifeTimestamp);

            bool spent = CreateService(store).TrySpendGold(
                60,
                CurrentTime + 60,
                out PlayerResources resources);

            Assert.That(spent, Is.True);
            Assert.That(resources.Gold, Is.EqualTo(40));
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(
                    GameBalance.LifeRefillDurationSeconds - 60));
            Assert.That(
                store.SaveData.NextLifeTimestamp,
                Is.EqualTo(nextLifeTimestamp));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TrySpendGold_WithNonPositiveAmount_Throws(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateService().TrySpendGold(
                    amount,
                    CurrentTime,
                    out _));
        }

        [Test]
        public void TryConsumeLife_FromMaximum_StartsRefillTimer()
        {
            InMemoryPlayerResourcesStore store = CreateDefaultStore();

            bool consumed = CreateService(store).TryConsumeLife(
                CurrentTime,
                out PlayerResources resources);

            Assert.That(consumed, Is.True);
            Assert.That(
                resources.Lives,
                Is.EqualTo(GameBalance.MaximumLives - 1));
            Assert.That(
                resources.SecondsUntilNextLife,
                Is.EqualTo(GameBalance.LifeRefillDurationSeconds));
        }

        [Test]
        public void TryConsumeLife_WhileRefilling_DoesNotRestartTimer()
        {
            long nextLifeTimestamp =
                CurrentTime + GameBalance.LifeRefillDurationSeconds;
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                GameBalance.MaximumLives - 1,
                nextLifeTimestamp);

            bool consumed = CreateService(store).TryConsumeLife(
                CurrentTime + 60,
                out PlayerResources resources);

            Assert.That(consumed, Is.True);
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
        public void TryConsumeLife_WithoutLives_ReturnsFalse()
        {
            InMemoryPlayerResourcesStore store = CreateStore(
                GameBalance.InitialGold,
                0,
                CurrentTime + GameBalance.LifeRefillDurationSeconds);

            bool consumed = CreateService(store).TryConsumeLife(
                CurrentTime,
                out PlayerResources resources);

            Assert.That(consumed, Is.False);
            Assert.That(resources.Lives, Is.Zero);
            Assert.That(store.SaveCount, Is.Zero);
        }

        private static PlayerResourcesService CreateService()
        {
            return CreateService(CreateDefaultStore());
        }

        private static PlayerResourcesService CreateService(
            IPlayerResourcesStore store)
        {
            return new PlayerResourcesService(store);
        }
    }
}
