using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Levels;
using WaterSortPuzzle.Levels.Loading;
using WaterSortPuzzle.Levels.Sources;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class LiquidColorPaletteTests
    {
        private LiquidColorPalette palette;

        [SetUp]
        public void SetUp()
        {
            palette = ScriptableObject.CreateInstance<LiquidColorPalette>();

            SerializedObject serializedPalette = new SerializedObject(palette);
            SerializedProperty entries = serializedPalette.FindProperty("entries");
            entries.arraySize = 1;

            SerializedProperty entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("liquidId").stringValue = "red";
            entry.FindPropertyRelative("color").colorValue = Color.red;
            serializedPalette.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(palette);
        }

        [Test]
        public void GetColor_WithKnownLiquidId_ReturnsConfiguredColor()
        {
            Color color = palette.GetColor("red");

            Assert.That(color, Is.EqualTo(Color.red));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetColor_WithEmptyLiquidId_ThrowsArgumentException(
            string liquidId)
        {
            Assert.Throws<ArgumentException>(() => palette.GetColor(liquidId));
        }

        [Test]
        public void GetColor_WithUnknownLiquidId_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => palette.GetColor("blue"));
        }

        [Test]
        public void GetColor_WithDifferentlyCasedLiquidId_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => palette.GetColor("Red"));
        }

        [Test]
        public void ProjectPalette_ContainsUniqueNonEmptyLiquidIds()
        {
            LiquidColorPalette projectPalette =
                FindSingleAsset<LiquidColorPalette>();
            SerializedObject serializedPalette =
                new SerializedObject(projectPalette);
            SerializedProperty entries = serializedPalette.FindProperty("entries");
            HashSet<string> liquidIds =
                new HashSet<string>(StringComparer.Ordinal);

            Assert.That(entries.arraySize, Is.GreaterThan(0));

            for (int index = 0; index < entries.arraySize; index++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(index);
                string liquidId =
                    entry.FindPropertyRelative("liquidId").stringValue;

                Assert.That(
                    string.IsNullOrWhiteSpace(liquidId),
                    Is.False,
                    $"Entry {index} has no ID.");
                Assert.That(
                    liquidIds.Add(liquidId),
                    Is.True,
                    $"Liquid ID '{liquidId}' is duplicated.");
            }
        }

        [Test]
        public void ProjectPalette_ContainsEveryLiquidIdUsedByLevels()
        {
            LiquidColorPalette projectPalette =
                FindSingleAsset<LiquidColorPalette>();
            LevelFileCatalog catalog = FindSingleAsset<LevelFileCatalog>();
            LevelDataLoader levelDataLoader = new LevelDataLoader();

            foreach (TextAsset levelFile in catalog.LevelFiles)
            {
                LevelData level = levelDataLoader.Load(levelFile);

                foreach (BottleData bottle in level.Bottles)
                {
                    foreach (string liquidId in bottle.LiquidIdsBottomToTop)
                    {
                        Assert.DoesNotThrow(
                            () => projectPalette.GetColor(liquidId),
                            $"Level file '{levelFile.name}' uses the missing " +
                            $"liquid ID '{liquidId}'.");
                    }
                }
            }
        }

        private static T FindSingleAsset<T>() where T : UnityEngine.Object
        {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            Assert.That(
                assetGuids.Length,
                Is.EqualTo(1),
                $"The project must contain exactly one {typeof(T).Name} asset.");

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            Assert.That(asset, Is.Not.Null);
            return asset;
        }
    }
}
