using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform liquidContainer;
        [SerializeField] private LiquidSlotView liquidSlotPrefab;

        private readonly List<LiquidSlotView> liquidSlotsBottomToTop = new List<LiquidSlotView>();

        private BottleState bottleState;
        private LiquidColorPalette colorPalette;

        public event Action<BottleView> Clicked;

        public BottleState State => bottleState;

        public void Initialize(
            BottleState state,
            LiquidColorPalette palette)
        {
            bottleState = state ?? throw new ArgumentNullException(nameof(state));
            colorPalette = palette ?? throw new ArgumentNullException(nameof(palette));

            CreateLiquidSlots(bottleState.Capacity);
            Refresh();
        }

        public void Refresh()
        {
            for (int liquidIndex = 0; liquidIndex < bottleState.Capacity; liquidIndex++)
            {
                LiquidSlotView liquidSlot = liquidSlotsBottomToTop[liquidIndex];

                if (liquidIndex >= bottleState.LiquidCount)
                {
                    liquidSlot.ShowEmpty();
                    continue;
                }

                string liquidId = bottleState.LiquidIdsBottomToTop[liquidIndex];
                Color liquidColor = colorPalette.GetColor(liquidId);
                bool isHidden = bottleState.IsLiquidHidden(liquidIndex);

                liquidSlot.ShowLiquid(liquidColor, isHidden);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        private void CreateLiquidSlots(int capacity)
        {
            for (int slotIndex = 0; slotIndex < capacity; slotIndex++)
            {
                LiquidSlotView liquidSlot = Instantiate(
                    liquidSlotPrefab,
                    liquidContainer,
                    false);

                liquidSlot.transform.SetAsFirstSibling();
                liquidSlotsBottomToTop.Add(liquidSlot);
            }
        }
    }
}
