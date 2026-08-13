using DG.Tweening;
using UnityEngine;

namespace WaterSortPuzzle.Animations
{
    public sealed class BottleCapAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform capVisual;
        [SerializeField, Min(0f)] private float closingStartOffsetRatio;
        [SerializeField, Min(0f)] private float closingDuration;
        [SerializeField] private Ease closingEase;

        private Tween closingTween;
        private Vector2 restingPosition;
        private bool hasRestingPosition;

        public void Hide()
        {
            StopClosingAnimation();
            capVisual.gameObject.SetActive(false);
        }

        public void PlayClosing()
        {
            StopClosingAnimation();
            capVisual.gameObject.SetActive(true);

            capVisual.anchoredPosition = restingPosition + Vector2.up * capVisual.rect.height * closingStartOffsetRatio;

            closingTween = capVisual
                .DOAnchorPos(restingPosition, closingDuration)
                .SetEase(closingEase)
                .SetLink(gameObject);
        }

        private void StopClosingAnimation()
        {
            CaptureRestingPosition();
            closingTween?.Kill();
            capVisual.anchoredPosition = restingPosition;
        }

        private void CaptureRestingPosition()
        {
            if (hasRestingPosition)
            {
                return;
            }

            restingPosition = capVisual.anchoredPosition;
            hasRestingPosition = true;
        }
    }
}
