using System;
using Coffee.UIExtensions;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class BottleViewTests
    {
        private GameObject bottleObject;
        private RectTransform liquidContainer;
        private BottleLiquidGraphic liquidGraphic;
        private HiddenLiquidGraphic hiddenLiquidGraphic;
        private Texture2D hiddenLiquidTexture;
        private Sprite hiddenLiquidSprite;
        private GameObject capVisual;
        private RectTransform capTransform;
        private BottleCompletionAnimator completionAnimator;
        private UIParticle completionSparkle;
        private LiquidColorPalette colorPalette;
        private BottleView view;

        [SetUp]
        public void SetUp()
        {
            bottleObject = new GameObject("Bottle", typeof(RectTransform));

            GameObject containerObject = new GameObject(
                "Liquid Container",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(BottleLiquidGraphic));
            containerObject.transform.SetParent(bottleObject.transform);
            liquidContainer = containerObject.GetComponent<RectTransform>();
            liquidContainer.sizeDelta = new Vector2(100f, 200f);
            liquidGraphic = containerObject.GetComponent<BottleLiquidGraphic>();

            GameObject hiddenGraphicObject = new GameObject(
                "Hidden Liquid Graphic",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(HiddenLiquidGraphic));
            hiddenGraphicObject.transform.SetParent(
                bottleObject.transform,
                false);
            RectTransform hiddenGraphicTransform =
                hiddenGraphicObject.GetComponent<RectTransform>();
            hiddenGraphicTransform.sizeDelta = liquidContainer.sizeDelta;
            hiddenLiquidGraphic =
                hiddenGraphicObject.GetComponent<HiddenLiquidGraphic>();
            hiddenLiquidTexture = new Texture2D(2, 2);
            hiddenLiquidSprite = Sprite.Create(
                hiddenLiquidTexture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f));

            SerializedObject serializedHiddenGraphic =
                new SerializedObject(hiddenLiquidGraphic);
            serializedHiddenGraphic.FindProperty("hiddenLiquidSprite")
                .objectReferenceValue = hiddenLiquidSprite;
            serializedHiddenGraphic.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedLiquidGraphic =
                new SerializedObject(liquidGraphic);
            serializedLiquidGraphic.FindProperty("hiddenLiquidGraphic")
                .objectReferenceValue = hiddenLiquidGraphic;
            serializedLiquidGraphic.ApplyModifiedPropertiesWithoutUndo();

            BottlePourAnimator pourAnimator =
                bottleObject.AddComponent<BottlePourAnimator>();
            SerializedObject serializedPourAnimator =
                new SerializedObject(pourAnimator);
            serializedPourAnimator.FindProperty("liquidGraphic")
                .objectReferenceValue = liquidGraphic;
            serializedPourAnimator.ApplyModifiedPropertiesWithoutUndo();

            capVisual = new GameObject(
                "Cap Visual",
                typeof(RectTransform));
            capTransform = capVisual.GetComponent<RectTransform>();
            capTransform.SetParent(bottleObject.transform, false);
            capTransform.sizeDelta = new Vector2(50f, 20f);
            capVisual.SetActive(false);

            BottleCapAnimator capAnimator =
                bottleObject.AddComponent<BottleCapAnimator>();
            SerializedObject serializedCapAnimator =
                new SerializedObject(capAnimator);
            serializedCapAnimator.FindProperty("capVisual")
                .objectReferenceValue = capTransform;
            serializedCapAnimator.FindProperty("closingStartOffsetRatio")
                .floatValue = 1f;
            serializedCapAnimator.FindProperty("closingDuration")
                .floatValue = 1f;
            serializedCapAnimator.ApplyModifiedPropertiesWithoutUndo();

            GameObject effectObject = new GameObject(
                "CompletionSparkle",
                typeof(RectTransform),
                typeof(UIParticle));
            effectObject.transform.SetParent(bottleObject.transform, false);
            completionSparkle = effectObject.GetComponent<UIParticle>();

            completionAnimator =
                bottleObject.AddComponent<BottleCompletionAnimator>();
            SerializedObject serializedCompletionAnimator =
                new SerializedObject(completionAnimator);
            serializedCompletionAnimator.FindProperty("capAnimator")
                .objectReferenceValue = capAnimator;
            serializedCompletionAnimator.FindProperty("completionSparkle")
                .objectReferenceValue = completionSparkle;
            serializedCompletionAnimator.ApplyModifiedPropertiesWithoutUndo();

            colorPalette = CreateColorPalette();
            view = bottleObject.AddComponent<BottleView>();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("liquidContainer").objectReferenceValue =
                liquidContainer;
            serializedView.FindProperty("liquidGraphic").objectReferenceValue =
                liquidGraphic;
            serializedView.FindProperty("completionAnimator").objectReferenceValue =
                completionAnimator;
            serializedView.FindProperty("pourAnimator").objectReferenceValue =
                pourAnimator;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(bottleObject);
            UnityEngine.Object.DestroyImmediate(colorPalette);
            UnityEngine.Object.DestroyImmediate(hiddenLiquidSprite);
            UnityEngine.Object.DestroyImmediate(hiddenLiquidTexture);
        }

        [Test]
        public void Initialize_WithNullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => view.Initialize(null, colorPalette));
        }

        [Test]
        public void Initialize_WithNullPalette_ThrowsArgumentNullException()
        {
            BottleState state = CreateBottleState(4, EmptyBottleJson);

            Assert.Throws<ArgumentNullException>(
                () => view.Initialize(state, null));
        }

        [Test]
        public void Initialize_WithState_ExposesState()
        {
            BottleState state = CreateBottleState(4, EmptyBottleJson);

            view.Initialize(state, colorPalette);

            Assert.That(view.State, Is.SameAs(state));
        }

        [Test]
        public void Initialize_WithLiquids_RendersVisibleAndHiddenLiquids()
        {
            BottleState state = CreateBottleState(4, FilledBottleJson);

            view.Initialize(state, colorPalette);

            Assert.That(RenderedHiddenLiquidHasGeometry(), Is.True);
            Assert.That(RenderedLiquidContainsColor(Color.blue), Is.True);
        }

        [Test]
        public void Refresh_AfterRemovingLiquid_UpdatesLiquidGraphic()
        {
            BottleState state = CreateBottleState(4, VisibleBottleJson);
            view.Initialize(state, colorPalette);

            state.RemoveTopLiquid();
            view.Refresh();

            Assert.That(RenderedLiquidContainsColor(Color.red), Is.True);
            Assert.That(RenderedLiquidContainsColor(Color.blue), Is.False);
        }

        [Test]
        public void Refresh_AfterBottleBecomesCompleted_PlaysCompletion()
        {
            BottleState state = CreateBottleState(
                4,
                PartiallyFilledSingleColorBottleJson);
            view.Initialize(state, colorPalette);

            state.AddLiquid("red");
            view.Refresh();

            Assert.That(state.IsCompleted, Is.True);
            Assert.That(capVisual.activeSelf, Is.True);
            Assert.That(DOTween.IsTweening(capTransform), Is.True);
            Assert.That(completionSparkle.isPaused, Is.True);

            DOTween.Complete(capTransform);

            Assert.That(completionSparkle.isPaused, Is.False);
        }

        [Test]
        public void Refresh_AfterCompletionAnimation_NotifiesSubscribers()
        {
            BottleState state = CreateBottleState(
                4,
                PartiallyFilledSingleColorBottleJson);
            bool completionFinished = false;
            view.Initialize(state, colorPalette);
            view.CompletionAnimationFinished += () =>
                completionFinished = true;

            state.AddLiquid("red");
            view.Refresh();

            Assert.That(completionFinished, Is.False);

            DOTween.Complete(capTransform);

            Assert.That(completionFinished, Is.True);
        }

        [TestCase(EmptyBottleJson)]
        [TestCase(VisibleBottleJson)]
        public void Initialize_WithIncompleteBottle_HidesCap(string json)
        {
            BottleState state = CreateBottleState(4, json);

            view.Initialize(state, colorPalette);

            Assert.That(capVisual.activeSelf, Is.False);
            Assert.That(DOTween.IsTweening(capTransform), Is.False);
            Assert.That(completionSparkle.isPaused, Is.True);
        }

        [Test]
        public void Initialize_WithCompletedBottle_HidesCap()
        {
            BottleState state = CreateBottleState(4, CompletedBottleJson);

            view.Initialize(state, colorPalette);

            Assert.That(capVisual.activeSelf, Is.False);
            Assert.That(DOTween.IsTweening(capTransform), Is.False);
            Assert.That(completionSparkle.isPaused, Is.True);
        }

        [Test]
        public void OnPointerClick_NotifiesSubscribers()
        {
            BottleView clickedView = null;
            view.Clicked += bottleView => clickedView = bottleView;

            view.OnPointerClick(new PointerEventData(null));

            Assert.That(clickedView, Is.SameAs(view));
        }

        private static LiquidColorPalette CreateColorPalette()
        {
            LiquidColorPalette palette =
                ScriptableObject.CreateInstance<LiquidColorPalette>();
            SerializedObject serializedPalette = new SerializedObject(palette);
            SerializedProperty entries = serializedPalette.FindProperty("entries");
            entries.arraySize = 2;

            ConfigurePaletteEntry(entries, 0, "red", Color.red);
            ConfigurePaletteEntry(entries, 1, "blue", Color.blue);
            serializedPalette.ApplyModifiedPropertiesWithoutUndo();

            return palette;
        }

        private static void ConfigurePaletteEntry(
            SerializedProperty entries,
            int index,
            string liquidId,
            Color color)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("liquidId").stringValue = liquidId;
            entry.FindPropertyRelative("color").colorValue = color;
        }

        private bool RenderedLiquidContainsColor(Color expectedColor)
        {
            liquidGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                liquidGraphic.canvasRenderer.GetMesh());

            try
            {
                foreach (Color color in mesh.colors)
                {
                    if (color == expectedColor)
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        private bool RenderedHiddenLiquidHasGeometry()
        {
            hiddenLiquidGraphic.Rebuild(CanvasUpdate.PreRender);
            Mesh mesh = UnityEngine.Object.Instantiate(
                hiddenLiquidGraphic.canvasRenderer.GetMesh());

            try
            {
                return mesh.vertexCount > 0;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        private static BottleState CreateBottleState(int capacity, string json)
        {
            BottleData data = JsonUtility.FromJson<BottleData>(json);
            return new BottleState(capacity, data);
        }

        private const string EmptyBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [],
          ""hiddenLiquidIndices"": []
        }";

        private const string FilledBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": [0]
        }";

        private const string VisibleBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": []
        }";

        private const string CompletedBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""red"", ""red"", ""red""],
          ""hiddenLiquidIndices"": []
        }";

        private const string PartiallyFilledSingleColorBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""red"", ""red""],
          ""hiddenLiquidIndices"": []
        }";
    }
}
