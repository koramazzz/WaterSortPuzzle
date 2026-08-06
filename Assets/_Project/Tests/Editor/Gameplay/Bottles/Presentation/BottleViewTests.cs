using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WaterSortPuzzle.Gameplay.Bottles;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;
using WaterSortPuzzle.Levels;

namespace WaterSortPuzzle.Tests.EditMode.Gameplay.Bottles.Presentation
{
    public sealed class BottleViewTests
    {
        private GameObject bottleObject;
        private RectTransform liquidContainer;
        private GameObject slotPrefabObject;
        private LiquidSlotView slotPrefab;
        private LiquidColorPalette colorPalette;
        private BottleView view;

        [SetUp]
        public void SetUp()
        {
            bottleObject = new GameObject("Bottle", typeof(RectTransform));

            GameObject containerObject = new GameObject(
                "Liquid Container",
                typeof(RectTransform));
            containerObject.transform.SetParent(bottleObject.transform);
            liquidContainer = containerObject.GetComponent<RectTransform>();

            CreateLiquidSlotPrefab();
            colorPalette = CreateColorPalette();
            view = bottleObject.AddComponent<BottleView>();

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("liquidContainer").objectReferenceValue =
                liquidContainer;
            serializedView.FindProperty("liquidSlotPrefab").objectReferenceValue =
                slotPrefab;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(bottleObject);
            UnityEngine.Object.DestroyImmediate(slotPrefabObject);
            UnityEngine.Object.DestroyImmediate(colorPalette);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void Initialize_WithCapacity_CreatesMatchingSlotCount(int capacity)
        {
            BottleState state = CreateBottleState(capacity, EmptyBottleJson);

            view.Initialize(state, colorPalette);

            Assert.That(liquidContainer.childCount, Is.EqualTo(capacity));
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
        public void Initialize_WithLiquids_DisplaysSlotsBottomToTop()
        {
            const int capacity = 4;
            BottleState state = CreateBottleState(capacity, FilledBottleJson);

            view.Initialize(state, colorPalette);

            LiquidSlotView bottomSlot = GetSlotFromBottom(0, capacity);
            LiquidSlotView hiddenSlot = GetSlotFromBottom(1, capacity);
            LiquidSlotView emptyTopSlot = GetSlotFromBottom(3, capacity);

            Assert.That(bottomSlot.GetComponent<Image>().color, Is.EqualTo(Color.red));
            Assert.That(bottomSlot.GetComponent<Image>().enabled, Is.True);
            Assert.That(
                bottomSlot.transform.Find("Hidden Visual").gameObject.activeSelf,
                Is.False);

            Assert.That(hiddenSlot.GetComponent<Image>().enabled, Is.False);
            Assert.That(
                hiddenSlot.transform.Find("Hidden Visual").gameObject.activeSelf,
                Is.True);

            Assert.That(emptyTopSlot.GetComponent<Image>().enabled, Is.False);
            Assert.That(
                emptyTopSlot.transform.Find("Hidden Visual").gameObject.activeSelf,
                Is.False);
        }

        [Test]
        public void Refresh_AfterRemovingLiquid_UpdatesFreedSlot()
        {
            const int capacity = 4;
            BottleState state = CreateBottleState(capacity, VisibleBottleJson);
            view.Initialize(state, colorPalette);

            state.RemoveTopLiquid();
            view.Refresh();

            LiquidSlotView freedSlot = GetSlotFromBottom(1, capacity);

            Assert.That(freedSlot.GetComponent<Image>().enabled, Is.False);
            Assert.That(
                freedSlot.transform.Find("Hidden Visual").gameObject.activeSelf,
                Is.False);
        }

        private void CreateLiquidSlotPrefab()
        {
            slotPrefabObject = new GameObject(
                "Liquid Slot",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(LiquidSlotView));

            GameObject hiddenVisual = new GameObject("Hidden Visual");
            hiddenVisual.transform.SetParent(slotPrefabObject.transform);
            hiddenVisual.SetActive(false);

            slotPrefab = slotPrefabObject.GetComponent<LiquidSlotView>();
            SerializedObject serializedSlot = new SerializedObject(slotPrefab);
            serializedSlot.FindProperty("liquidImage").objectReferenceValue =
                slotPrefabObject.GetComponent<Image>();
            serializedSlot.FindProperty("hiddenVisual").objectReferenceValue =
                hiddenVisual;
            serializedSlot.ApplyModifiedPropertiesWithoutUndo();
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

        private LiquidSlotView GetSlotFromBottom(int liquidIndex, int capacity)
        {
            int childIndex = capacity - liquidIndex - 1;
            return liquidContainer
                .GetChild(childIndex)
                .GetComponent<LiquidSlotView>();
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
          ""hiddenLiquidIndices"": [1]
        }";

        private const string VisibleBottleJson = @"
        {
          ""liquidIdsBottomToTop"": [""red"", ""blue""],
          ""hiddenLiquidIndices"": []
        }";
    }
}
