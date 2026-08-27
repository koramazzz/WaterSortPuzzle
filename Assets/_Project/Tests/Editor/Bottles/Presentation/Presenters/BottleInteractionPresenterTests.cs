using System;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Audio;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Gameplay.Bottles.Presentation.Layout;
using WaterSortPuzzle.Gameplay.Levels;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class BottleInteractionPresenterTests
    {
        private const float SelectionLiftRatio = 0.25f;

        private GameObject collectionObject;
        private BottleCollectionView collectionView;
        private LevelState levelState;
        private BottleInteractionPresenter presenter;
        private List<SoundEffectId> requestedSoundEffects;

        [SetUp]
        public void SetUp()
        {
            collectionObject = new GameObject(
                "Bottle Collection",
                typeof(RectTransform),
                typeof(BottleCollectionView),
                typeof(BottleGridLayout));
            collectionView = collectionObject.GetComponent<BottleCollectionView>();

            BottleGridLayout bottleGridLayout =
                collectionObject.GetComponent<BottleGridLayout>();
            SerializedObject serializedLayout =
                new SerializedObject(bottleGridLayout);
            serializedLayout.FindProperty("maximumColumnCount").intValue = 2;
            serializedLayout.FindProperty("bottleAspectRatio").floatValue = 0.5f;
            serializedLayout.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedView = new SerializedObject(collectionView);
            serializedView.FindProperty("bottlePrefab").objectReferenceValue =
                FindSinglePrefabComponent<BottleView>();
            serializedView.FindProperty("colorPalette").objectReferenceValue =
                FindSingleAsset<LiquidColorPalette>();
            serializedView.FindProperty("bottleGridLayout").objectReferenceValue =
                bottleGridLayout;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            LevelData levelData = JsonUtility.FromJson<LevelData>(LevelJson);
            levelState = new LevelState(levelData);
            collectionView.Initialize(levelState.Bottles);

            presenter = new BottleInteractionPresenter();
            requestedSoundEffects = new List<SoundEffectId>();
            presenter.SoundEffectRequested += requestedSoundEffects.Add;
            presenter.Initialize(collectionView);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(collectionObject);
        }

        [Test]
        public void BottleClicks_WithValidDestination_PourAndRefreshViews()
        {
            BottleView sourceView = GetBottleView(0);
            BottleView destinationView = GetBottleView(1);

            sourceView.OnPointerClick(new PointerEventData(null));
            destinationView.OnPointerClick(new PointerEventData(null));

            Assert.That(
                levelState.Bottles[0].LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red" }));
            Assert.That(
                levelState.Bottles[1].LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "blue" }));
            Assert.That(GetSlotFromBottom(sourceView, 1).enabled, Is.False);
            Assert.That(GetSlotFromBottom(destinationView, 0).enabled, Is.True);
        }

        [Test]
        public void BottleClicks_WithValidDestination_RaisePourCompleted()
        {
            int completedPourCount = 0;
            presenter.PourCompleted += () => completedPourCount++;

            GetBottleView(0).OnPointerClick(new PointerEventData(null));
            GetBottleView(1).OnPointerClick(new PointerEventData(null));

            Assert.That(completedPourCount, Is.EqualTo(1));
        }

        [Test]
        public void BottleClicks_WhenSelectionIsCancelled_DoNotRaisePourCompleted()
        {
            int completedPourCount = 0;
            presenter.PourCompleted += () => completedPourCount++;
            BottleView sourceView = GetBottleView(0);

            sourceView.OnPointerClick(new PointerEventData(null));
            sourceView.OnPointerClick(new PointerEventData(null));

            Assert.That(completedPourCount, Is.Zero);
        }

        [Test]
        public void BottleClick_WithSelectableSource_RequestsSelectionSound()
        {
            GetBottleView(0).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[] { SoundEffectId.BottleSelected }));
        }

        [Test]
        public void BottleClick_WithEmptySource_RequestsInvalidMoveSound()
        {
            GetBottleView(1).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[] { SoundEffectId.InvalidMove }));
        }

        [Test]
        public void BottleClicks_WhenSelectionIsCancelled_RequestReleasedSound()
        {
            BottleView sourceView = GetBottleView(0);

            sourceView.OnPointerClick(new PointerEventData(null));
            sourceView.OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.BottleReleased
                }));
        }

        [Test]
        public void BottleClicks_WithInvalidDestination_RequestInvalidMoveSound()
        {
            GetBottleView(0).OnPointerClick(new PointerEventData(null));
            GetBottleView(2).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.InvalidMove
                }));
        }

        [Test]
        public void Initialize_WithNullCollection_ThrowsArgumentNullException()
        {
            BottleInteractionPresenter presenter = new BottleInteractionPresenter();

            Assert.Throws<ArgumentNullException>(() => presenter.Initialize(null));
        }

        [Test]
        public void BottleClicks_SelectAndDeselectSourceView()
        {
            BottleView sourceView = GetBottleView(0);
            RectTransform sourceTransform =
                sourceView.GetComponent<RectTransform>();
            float restingPositionY = sourceTransform.anchoredPosition.y;
            float expectedSelectedPositionY = restingPositionY +
                sourceTransform.rect.height * SelectionLiftRatio;
            ConfigureSelectionAnimation(sourceView);

            sourceView.OnPointerClick(new PointerEventData(null));
            DOTween.Complete(sourceTransform);

            Assert.That(
                sourceTransform.anchoredPosition.y,
                Is.EqualTo(expectedSelectedPositionY).Within(0.001f));

            sourceView.OnPointerClick(new PointerEventData(null));
            DOTween.Complete(sourceTransform);

            Assert.That(
                sourceTransform.anchoredPosition.y,
                Is.EqualTo(restingPositionY).Within(0.001f));
        }

        private BottleView GetBottleView(int index)
        {
            return collectionObject
                .transform
                .GetChild(index)
                .GetComponent<BottleView>();
        }

        private static Image GetSlotFromBottom(
            BottleView bottleView,
            int liquidIndex)
        {
            Transform liquidContainer = bottleView
                .GetComponentInChildren<VerticalLayoutGroup>()
                .transform;
            int childIndex = liquidContainer.childCount - liquidIndex - 1;
            return liquidContainer.GetChild(childIndex).GetComponent<Image>();
        }

        private static void ConfigureSelectionAnimation(BottleView bottleView)
        {
            BottleSelectionAnimator animator =
                bottleView.GetComponent<BottleSelectionAnimator>();
            SerializedObject serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("selectionLiftRatio").floatValue =
                SelectionLiftRatio;
            serializedAnimator.FindProperty("selectionDuration").floatValue = 1f;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
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
          ""difficulty"": 1,
          ""bottleCapacity"": 2,
          ""bottles"": [
            {
              ""liquidIdsBottomToTop"": [""red"", ""blue""],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [""red""],
              ""hiddenLiquidIndices"": []
            },
            {
              ""liquidIdsBottomToTop"": [""blue""],
              ""hiddenLiquidIndices"": []
            }
          ]
        }";
    }
}
