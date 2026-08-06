using System;
using NUnit.Framework;
using UnityEngine;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Tests.EditMode.Progress
{
    public sealed class PlayerPrefsLevelProgressStoreTests
    {
        private const string CompletedLevelCountKey =
            "WaterSortPuzzle.Progress.CompletedLevelCount";

        private bool hadSavedCompletedLevelCount;
        private int savedCompletedLevelCount;

        [SetUp]
        public void SetUp()
        {
            hadSavedCompletedLevelCount = PlayerPrefs.HasKey(
                CompletedLevelCountKey);

            if (hadSavedCompletedLevelCount)
            {
                savedCompletedLevelCount = PlayerPrefs.GetInt(
                    CompletedLevelCountKey);
            }

            PlayerPrefs.DeleteKey(CompletedLevelCountKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (hadSavedCompletedLevelCount)
            {
                PlayerPrefs.SetInt(
                    CompletedLevelCountKey,
                    savedCompletedLevelCount);
            }
            else
            {
                PlayerPrefs.DeleteKey(CompletedLevelCountKey);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void LoadCompletedLevelCount_WithoutSavedValue_ReturnsZero()
        {
            PlayerPrefsLevelProgressStore store = CreateStore();

            int completedLevelCount = store.LoadCompletedLevelCount(3);

            Assert.That(completedLevelCount, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(3)]
        public void SaveCompletedLevelCount_WithValidCount_PersistsValue(
            int completedLevelCount)
        {
            PlayerPrefsLevelProgressStore writer = CreateStore();
            PlayerPrefsLevelProgressStore reader = CreateStore();

            writer.SaveCompletedLevelCount(completedLevelCount, 3);
            int loadedCompletedLevelCount = reader.LoadCompletedLevelCount(3);

            Assert.That(
                loadedCompletedLevelCount,
                Is.EqualTo(completedLevelCount));
        }

        [Test]
        public void LoadCompletedLevelCount_WithValueBelowRange_ReturnsZero()
        {
            PlayerPrefs.SetInt(CompletedLevelCountKey, -5);
            PlayerPrefsLevelProgressStore store = CreateStore();

            int completedLevelCount = store.LoadCompletedLevelCount(3);

            Assert.That(completedLevelCount, Is.Zero);
        }

        [Test]
        public void LoadCompletedLevelCount_WithValueAboveRange_ReturnsLevelCount()
        {
            PlayerPrefs.SetInt(CompletedLevelCountKey, 5);
            PlayerPrefsLevelProgressStore store = CreateStore();

            int completedLevelCount = store.LoadCompletedLevelCount(3);

            Assert.That(completedLevelCount, Is.EqualTo(3));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void SaveCompletedLevelCount_WithOutOfRangeCount_Throws(
            int completedLevelCount)
        {
            PlayerPrefsLevelProgressStore store = CreateStore();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => store.SaveCompletedLevelCount(completedLevelCount, 3));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void LoadCompletedLevelCount_WithNonPositiveLevelCount_Throws(
            int levelCount)
        {
            PlayerPrefsLevelProgressStore store = CreateStore();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => store.LoadCompletedLevelCount(levelCount));
        }

        private static PlayerPrefsLevelProgressStore CreateStore()
        {
            return new PlayerPrefsLevelProgressStore();
        }
    }
}
