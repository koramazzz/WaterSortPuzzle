using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerPrefsGoldStoreTests
    {
        private const string GoldKey =
            "WaterSortPuzzle.Progress.Gold";

        private bool hadSavedGold;
        private int savedGold;

        [SetUp]
        public void SetUp()
        {
            hadSavedGold = PlayerPrefs.HasKey(GoldKey);
            savedGold = PlayerPrefs.GetInt(GoldKey);
            PlayerPrefs.DeleteKey(GoldKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (hadSavedGold)
            {
                PlayerPrefs.SetInt(GoldKey, savedGold);
            }
            else
            {
                PlayerPrefs.DeleteKey(GoldKey);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void LoadGold_WithoutSavedValue_ReturnsZero()
        {
            int gold = CreateStore().LoadGold();

            Assert.That(gold, Is.Zero);
        }

        [Test]
        public void AddGold_PersistsUpdatedValue()
        {
            PlayerPrefsGoldStore store = CreateStore();

            store.AddGold(100);
            int gold = CreateStore().LoadGold();

            Assert.That(gold, Is.EqualTo(100));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void AddGold_WithNonPositiveAmount_Throws(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateStore().AddGold(amount));
        }

        private static PlayerPrefsGoldStore CreateStore()
        {
            return new PlayerPrefsGoldStore();
        }
    }
}
