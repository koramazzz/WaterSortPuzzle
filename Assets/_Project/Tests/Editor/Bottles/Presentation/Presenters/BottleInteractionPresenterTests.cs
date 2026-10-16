using System;
using System.Collections.Generic;
using System.Reflection;
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
        private const float PourTravelDuration = 1f;
        private const float StreamEnterDuration = 0.2f;
        private const float TransferDurationPerLiquid = 0.25f;
        private const float StreamExitDuration = 0.15f;

        private GameObject rootObject;
        private GameObject collectionObject;
        private BottleCollectionView collectionView;
        private LevelState levelState;
        private BottleInteractionPresenter presenter;
        private List<SoundEffectId> requestedSoundEffects;

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject(
                "Gameplay Root",
                typeof(RectTransform));
            collectionObject = new GameObject(
                "Bottle Collection",
                typeof(RectTransform),
                typeof(BottleCollectionView),
                typeof(BottleGridLayout),
                typeof(LiquidTransferAnimator));
            collectionObject.transform.SetParent(rootObject.transform, false);
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
                CreateBottlePrefabForTest();
            serializedView.FindProperty("colorPalette").objectReferenceValue =
                FindSingleAsset<LiquidColorPalette>();
            serializedView.FindProperty("bottleGridLayout").objectReferenceValue =
                bottleGridLayout;
            serializedView.FindProperty("liquidTransferAnimator")
                .objectReferenceValue = ConfigureLiquidTransferAnimator();
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
            DOTween.Kill(collectionObject);
            UnityEngine.Object.DestroyImmediate(rootObject);
        }

        [Test]
        public void BottleClicks_WithValidDestination_PourAndRefreshViews()
        {
            BottleView sourceView = GetBottleView(0);
            BottleView destinationView = GetBottleView(1);

            sourceView.OnPointerClick(new PointerEventData(null));
            destinationView.OnPointerClick(new PointerEventData(null));

            DOTween.Complete(collectionObject, true);

            Assert.That(
                levelState.Bottles[0].LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "red" }));
            Assert.That(
                levelState.Bottles[1].LiquidIdsBottomToTop,
                Is.EqualTo(new[] { "blue" }));
            Assert.That(
                GetRenderedFillRatio(sourceView),
                Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(
                GetRenderedFillRatio(destinationView),
                Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void BottleClicks_WithValidDestination_RaisePourCompleted()
        {
            int completedPourCount = 0;
            presenter.PourCompleted += () => completedPourCount++;

            GetBottleView(0).OnPointerClick(new PointerEventData(null));
            GetBottleView(1).OnPointerClick(new PointerEventData(null));

            Assert.That(completedPourCount, Is.Zero);

            DOTween.Complete(collectionObject, true);

            Assert.That(completedPourCount, Is.EqualTo(1));
        }

        [Test]
        public void BottleClicks_DuringPour_AnimateLiquidVolumes()
        {
            BottleView sourceView = GetBottleView(0);
            BottleView destinationView = GetBottleView(1);

            sourceView.OnPointerClick(new PointerEventData(null));
            destinationView.OnPointerClick(new PointerEventData(null));
            float elapsedPourTime = StreamEnterDuration +
                TransferDurationPerLiquid * 0.5f;
            DOTween.Goto(
                collectionObject,
                PourTravelDuration + elapsedPourTime);
            float sourceTransferProgress = elapsedPourTime /
                (StreamEnterDuration + TransferDurationPerLiquid);
            float destinationTransferProgress =
                TransferDurationPerLiquid * 0.5f /
                (TransferDurationPerLiquid + StreamExitDuration);
            float expectedSourceFillRatio =
                (2f - sourceTransferProgress) / 2f;
            float expectedDestinationFillRatio =
                destinationTransferProgress / 2f;

            Assert.That(
                GetRenderedFillRatio(sourceView),
                Is.EqualTo(expectedSourceFillRatio).Within(0.001f));
            Assert.That(
                GetRenderedFillRatio(destinationView),
                Is.EqualTo(expectedDestinationFillRatio).Within(0.001f));
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
        public void BottleClicks_WithValidDestination_RequestValidPourSound()
        {
            GetBottleView(0).OnPointerClick(new PointerEventData(null));
            GetBottleView(1).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[] { SoundEffectId.BottleSelected }));

            DOTween.Complete(collectionObject, true);

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.ValidPour
                }));
        }

        [Test]
        public void BottleClick_DuringPourAnimationIsIgnored()
        {
            GetBottleView(0).OnPointerClick(new PointerEventData(null));
            GetBottleView(1).OnPointerClick(new PointerEventData(null));

            GetBottleView(2).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[] { SoundEffectId.BottleSelected }));

            DOTween.Complete(collectionObject, true);
            GetBottleView(2).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.ValidPour,
                    SoundEffectId.BottleSelected
                }));
        }

        [Test]
        public void BottleClicks_WhenPourIsCanceled_ReleasesInputAndRefreshesViews()
        {
            BottleView sourceView = GetBottleView(0);
            BottleView destinationView = GetBottleView(1);

            sourceView.OnPointerClick(new PointerEventData(null));
            destinationView.OnPointerClick(new PointerEventData(null));
            DOTween.Goto(collectionObject, PourTravelDuration * 0.5f);

            InvokeOnDisable(
                collectionObject.GetComponent<LiquidTransferAnimator>());

            Assert.That(
                GetRenderedFillRatio(sourceView),
                Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(
                GetRenderedFillRatio(destinationView),
                Is.EqualTo(0.5f).Within(0.001f));

            GetBottleView(2).OnPointerClick(new PointerEventData(null));

            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.BottleSelected
                }));
        }

        [Test]
        public void BottleClicks_WhenAnimationSetupFails_ReleasesInputAndRestoresSource()
        {
            BottleView sourceView = GetBottleView(0);
            BottleView destinationView = GetBottleView(1);
            RectTransform sourceTransform =
                (RectTransform)sourceView.transform;
            float restingPositionY = sourceTransform.anchoredPosition.y;
            SerializedObject serializedView =
                new SerializedObject(collectionView);
            serializedView.FindProperty("colorPalette").objectReferenceValue =
                null;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            sourceView.OnPointerClick(new PointerEventData(null));

            Assert.Throws<NullReferenceException>(() =>
                destinationView.OnPointerClick(new PointerEventData(null)));

            sourceView
                .GetComponent<BottleSelectionAnimator>()
                .CompleteSelection();
            Assert.That(
                sourceTransform.anchoredPosition.y,
                Is.EqualTo(restingPositionY).Within(0.001f));

            GetBottleView(2).OnPointerClick(new PointerEventData(null));
            Assert.That(
                requestedSoundEffects,
                Is.EqualTo(new[]
                {
                    SoundEffectId.BottleSelected,
                    SoundEffectId.BottleSelected
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

        private BottleView CreateBottlePrefabForTest()
        {
            BottleView projectPrefab = FindSinglePrefabComponent<BottleView>();
            BottleView testPrefab = UnityEngine.Object.Instantiate(
                projectPrefab,
                rootObject.transform,
                false);
            testPrefab.name = "Bottle Test Prefab";

            BottleLiquidGraphic liquidGraphic =
                testPrefab.GetComponentInChildren<BottleLiquidGraphic>();
            Assert.That(liquidGraphic, Is.Not.Null);

            BottlePourAnimator pourAnimator =
                testPrefab.GetComponent<BottlePourAnimator>();

            if (pourAnimator == null)
            {
                pourAnimator = testPrefab.gameObject.AddComponent<BottlePourAnimator>();
            }

            ConfigureBottlePourAnimator(pourAnimator, liquidGraphic);

            SerializedObject serializedView = new SerializedObject(testPrefab);
            serializedView.FindProperty("liquidGraphic").objectReferenceValue =
                liquidGraphic;
            serializedView.FindProperty("pourAnimator").objectReferenceValue =
                pourAnimator;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            return testPrefab;
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

        private LiquidTransferAnimator ConfigureLiquidTransferAnimator()
        {
            GameObject streamObject = new GameObject(
                "Pour Stream",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PourStreamGraphic),
                typeof(PourStreamView));
            streamObject.transform.SetParent(rootObject.transform, false);
            RectTransform streamTransform =
                streamObject.GetComponent<RectTransform>();
            streamTransform.sizeDelta = new Vector2(1000f, 1000f);
            PourStreamGraphic streamGraphic =
                streamObject.GetComponent<PourStreamGraphic>();
            SerializedObject serializedStreamGraphic =
                new SerializedObject(streamGraphic);
            serializedStreamGraphic.FindProperty("segmentCount").intValue = 8;
            serializedStreamGraphic.FindProperty("destinationWidthRatio")
                .floatValue = 0.5f;
            serializedStreamGraphic.FindProperty("sourceTangentLengthRatio")
                .floatValue = 0.3f;
            serializedStreamGraphic.FindProperty(
                    "destinationTangentLengthRatio")
                .floatValue = 0.2f;
            serializedStreamGraphic.FindProperty(
                    "lateralWaveAmplitudeRatio")
                .floatValue = 0.65f;
            serializedStreamGraphic.FindProperty("lateralWaveCycleCount")
                .floatValue = 1.25f;
            serializedStreamGraphic.FindProperty("widthVariationRatio")
                .floatValue = 0.16f;
            serializedStreamGraphic.ApplyModifiedPropertiesWithoutUndo();

            PourStreamView streamView =
                streamObject.GetComponent<PourStreamView>();
            SerializedObject serializedStreamView =
                new SerializedObject(streamView);
            serializedStreamView.FindProperty("streamGraphic")
                .objectReferenceValue = streamGraphic;
            serializedStreamView.ApplyModifiedPropertiesWithoutUndo();
            streamObject.SetActive(false);

            LiquidTransferAnimator transferAnimator =
                collectionObject.GetComponent<LiquidTransferAnimator>();
            SerializedObject serializedAnimator =
                new SerializedObject(transferAnimator);
            serializedAnimator.FindProperty("mouthVerticalGapRatio")
                .floatValue = 0.2f;
            serializedAnimator.FindProperty("mouthHorizontalGapRatio")
                .floatValue = 0.2f;
            serializedAnimator.FindProperty("streamEnterDuration").floatValue =
                StreamEnterDuration;
            serializedAnimator.FindProperty("transferDurationPerLiquid")
                .floatValue = TransferDurationPerLiquid;
            serializedAnimator.FindProperty("streamExitDuration").floatValue =
                StreamExitDuration;
            serializedAnimator.FindProperty("streamWidthRatio").floatValue = 0.1f;
            serializedAnimator.FindProperty("streamFlowCyclesPerSecond")
                .floatValue = 2.2f;
            serializedAnimator.FindProperty("pourEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("streamView").objectReferenceValue =
                streamView;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            return transferAnimator;
        }

        private static void ConfigureBottlePourAnimator(
            BottlePourAnimator pourAnimator,
            BottleLiquidGraphic liquidGraphic)
        {
            SerializedObject serializedAnimator =
                new SerializedObject(pourAnimator);
            serializedAnimator.FindProperty("liquidGraphic")
                .objectReferenceValue = liquidGraphic;
            serializedAnimator.FindProperty("travelDuration").floatValue =
                PourTravelDuration;
            serializedAnimator.FindProperty("travelArcHeightRatio").floatValue =
                0.25f;
            serializedAnimator.FindProperty("travelEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.FindProperty("returnDuration").floatValue = 1f;
            serializedAnimator.FindProperty("returnArcHeightRatio").floatValue =
                0.1f;
            serializedAnimator.FindProperty("returnEase").enumValueIndex =
                (int)Ease.Linear;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        private static float GetRenderedFillRatio(BottleView bottleView)
        {
            BottleLiquidGraphic liquidGraphic =
                bottleView.GetComponentInChildren<BottleLiquidGraphic>();
            liquidGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                liquidGraphic.canvasRenderer.GetMesh());

            try
            {
                Rect bounds = liquidGraphic.rectTransform.rect;
                return CalculateMeshArea(mesh) / (bounds.width * bounds.height);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        private static void InvokeOnDisable(LiquidTransferAnimator target)
        {
            MethodInfo onDisable = typeof(LiquidTransferAnimator).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(onDisable, Is.Not.Null);
            onDisable.Invoke(target, null);
        }

        private static float CalculateMeshArea(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float area = 0f;

            for (int triangleIndex = 0;
                 triangleIndex < triangles.Length;
                 triangleIndex += 3)
            {
                Vector3 first = vertices[triangles[triangleIndex]];
                Vector3 second = vertices[triangles[triangleIndex + 1]];
                Vector3 third = vertices[triangles[triangleIndex + 2]];
                area += Mathf.Abs(Vector3.Cross(second - first, third - first).z) *
                    0.5f;
            }

            return area;
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
