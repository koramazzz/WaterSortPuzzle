using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class BottleCollectionViewTests
    {
        private GameObject collectionObject;
        private BottleCollectionView view;

        [SetUp]
        public void SetUp()
        {
            collectionObject = new GameObject(
                "Bottle Collection",
                typeof(RectTransform),
                typeof(BottleCollectionView));
            view = collectionObject.GetComponent<BottleCollectionView>();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("bottlePrefab").objectReferenceValue =
                FindSinglePrefabComponent<BottleView>();
            serializedView.FindProperty("colorPalette").objectReferenceValue =
                FindSingleAsset<LiquidColorPalette>();
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(collectionObject);
        }

        [Test]
        public void Initialize_WithBottles_CreatesViewForEachBottle()
        {
            LevelState levelState = CreateLevelState();

            view.Initialize(levelState.Bottles);

            Assert.That(
                collectionObject.transform.childCount,
                Is.EqualTo(levelState.Bottles.Count));

            for (int index = 0; index < levelState.Bottles.Count; index++)
            {
                Transform bottleTransform =
                    collectionObject.transform.GetChild(index);

                Assert.That(
                    bottleTransform.GetComponent<BottleView>(),
                    Is.Not.Null);
                Assert.That(
                    bottleTransform.GetComponentsInChildren<LiquidSlotView>(true),
                    Has.Length.EqualTo(levelState.BottleCapacity));
            }
        }

        [Test]
        public void Initialize_WithNullBottleCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => view.Initialize(null));
        }

        private static LevelState CreateLevelState()
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(LevelJson);
            return new LevelState(levelData);
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

        private static T FindSinglePrefabComponent<T>() where T : Component
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            List<T> matchingComponents = new List<T>();

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                T component = prefab.GetComponent<T>();

                if (component != null)
                {
                    matchingComponents.Add(component);
                }
            }

            Assert.That(
                matchingComponents,
                Has.Count.EqualTo(1),
                $"The project must contain exactly one prefab with " +
                $"{typeof(T).Name}.");

            return matchingComponents[0];
        }

        private const string LevelJson = @"
        {
          ""levelNumber"": 1,
          ""bottleCapacity"": 2,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""blue""],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            }
          ]
        }";
    }
}
