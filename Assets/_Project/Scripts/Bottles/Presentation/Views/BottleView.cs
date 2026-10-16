using System;
using UnityEngine;
using UnityEngine.EventSystems;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class BottleView : MonoBehaviour, IPointerClickHandler
    {
        private readonly Vector3[] liquidContainerWorldCorners =
            new Vector3[4];

        [Header("Liquid")]
        [SerializeField] private RectTransform liquidContainer;
        [SerializeField] private BottleLiquidGraphic liquidGraphic;

        [Header("Animation")]
        [SerializeField] private BottleCompletionAnimator completionAnimator;
        [SerializeField] private BottleSelectionAnimator selectionAnimator;
        [SerializeField] private BottlePourAnimator pourAnimator;

        private BottleState bottleState;
        private LiquidColorPalette colorPalette;
        private Color[] liquidColorsBottomToTop;
        private float[] liquidFillAmountsBottomToTop;
        private bool[] hiddenLiquidsBottomToTop;

        public event Action<BottleView> Clicked;
        public event Action CompletionAnimationFinished;

        public BottleState State => bottleState;

        internal BottlePourAnimator PourAnimator => pourAnimator;

        public void Initialize(
            BottleState state,
            LiquidColorPalette palette)
        {
            bottleState = state ?? throw new ArgumentNullException(nameof(state));
            colorPalette = palette ?? throw new ArgumentNullException(nameof(palette));

            if (liquidGraphic == null)
            {
                throw new InvalidOperationException(
                    "A bottle liquid graphic is required.");
            }

            if (pourAnimator == null)
            {
                throw new InvalidOperationException(
                    "A bottle pour animator is required.");
            }

            liquidColorsBottomToTop = new Color[bottleState.Capacity];
            liquidFillAmountsBottomToTop = new float[bottleState.Capacity];
            hiddenLiquidsBottomToTop = new bool[bottleState.Capacity];
            RefreshLiquids();
            completionAnimator.Hide();
        }

        public void Refresh()
        {
            RefreshLiquids();

            if (bottleState.IsCompleted)
            {
                completionAnimator.PlayCompletion(() => CompletionAnimationFinished?.Invoke());
                return;
            }

            completionAnimator.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void AnimateSelection()
        {
            selectionAnimator.PlaySelection();
            pourAnimator.PlayLiquidInteraction();
        }

        public void AnimateDeselection()
        {
            selectionAnimator.PlayDeselection();
            pourAnimator.PlayLiquidInteraction();
        }

        internal Vector2 PrepareForPour()
        {
            selectionAnimator.CompleteSelection();
            return selectionAnimator.RestingPosition;
        }

        internal Rect GetLiquidBounds()
        {
            liquidContainer.GetWorldCorners(liquidContainerWorldCorners);
            RectTransform bottleTransform = (RectTransform)transform;
            Vector2 bottomLeft = bottleTransform.InverseTransformPoint(
                liquidContainerWorldCorners[0]);
            Vector2 topRight = bottleTransform.InverseTransformPoint(
                liquidContainerWorldCorners[2]);
            return Rect.MinMaxRect(
                bottomLeft.x,
                bottomLeft.y,
                topRight.x,
                topRight.y);
        }

        internal void ShowPourOutProgress(
            int liquidCountBefore,
            int pouredLiquidCount,
            float progress)
        {
            float transferredLiquidCount =
                Mathf.Clamp01(progress) * pouredLiquidCount;

            for (int pouredLiquidIndex = 0;
                 pouredLiquidIndex < pouredLiquidCount;
                 pouredLiquidIndex++)
            {
                int slotIndex = liquidCountBefore - pouredLiquidIndex - 1;
                float slotProgress = Mathf.Clamp01(
                    transferredLiquidCount - pouredLiquidIndex);
                liquidFillAmountsBottomToTop[slotIndex] = 1f - slotProgress;
            }

            UpdateLiquidGraphic();
        }

        internal void ShowPourInProgress(
            int liquidCountBefore,
            int pouredLiquidCount,
            Color liquidColor,
            float progress)
        {
            float transferredLiquidCount =
                Mathf.Clamp01(progress) * pouredLiquidCount;

            for (int pouredLiquidIndex = 0;
                 pouredLiquidIndex < pouredLiquidCount;
                 pouredLiquidIndex++)
            {
                int slotIndex = liquidCountBefore + pouredLiquidIndex;
                float slotProgress = Mathf.Clamp01(
                    transferredLiquidCount - pouredLiquidIndex);
                liquidColorsBottomToTop[slotIndex] = liquidColor;
                liquidFillAmountsBottomToTop[slotIndex] = slotProgress;
                hiddenLiquidsBottomToTop[slotIndex] = false;
            }

            UpdateLiquidGraphic();
        }

        internal void PlayLiquidImpact()
        {
            pourAnimator.PlayLiquidImpact();
        }

        private void RefreshLiquids()
        {
            for (int liquidIndex = 0; liquidIndex < bottleState.Capacity; liquidIndex++)
            {
                if (liquidIndex >= bottleState.LiquidCount)
                {
                    liquidFillAmountsBottomToTop[liquidIndex] = 0f;
                    hiddenLiquidsBottomToTop[liquidIndex] = false;
                    continue;
                }

                string liquidId = bottleState.LiquidIdsBottomToTop[liquidIndex];
                Color liquidColor = colorPalette.GetColor(liquidId);
                bool isHidden = bottleState.IsLiquidHidden(liquidIndex);

                liquidColorsBottomToTop[liquidIndex] = liquidColor;
                liquidFillAmountsBottomToTop[liquidIndex] = 1f;
                hiddenLiquidsBottomToTop[liquidIndex] = isHidden;
            }

            UpdateLiquidGraphic();
        }

        private void UpdateLiquidGraphic()
        {
            liquidGraphic.SetLayers(
                liquidColorsBottomToTop,
                liquidFillAmountsBottomToTop,
                hiddenLiquidsBottomToTop);
        }
    }
}
