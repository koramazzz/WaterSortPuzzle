using DG.Tweening;
using UnityEngine;

namespace WaterSortPuzzle.Animations
{
    public sealed class BottleSelectionAnimator : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float selectionLiftRatio;
        [SerializeField, Min(0f)] private float selectionDuration;
        [SerializeField] private Ease selectionEase;

        private Tween selectionTween;
        private Vector2 restingPosition;

        public Vector2 RestingPosition => restingPosition;

        public void PlaySelection()
        {
            RectTransform bottleTransform = (RectTransform)transform;
            restingPosition = bottleTransform.anchoredPosition;
            float selectedPositionY = restingPosition.y + bottleTransform.rect.height * selectionLiftRatio;

            AnimatePositionY(bottleTransform, selectedPositionY);
        }

        public void PlayDeselection()
        {
            AnimatePositionY((RectTransform)transform, restingPosition.y);
        }

        public void CompleteSelection()
        {
            selectionTween?.Complete();
        }

        private void AnimatePositionY(
            RectTransform bottleTransform,
            float targetPositionY)
        {
            selectionTween?.Kill();
            selectionTween = bottleTransform
                .DOAnchorPosY(targetPositionY, selectionDuration)
                .SetEase(selectionEase)
                .SetLink(gameObject);
        }
    }
}
