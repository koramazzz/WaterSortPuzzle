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
        private float restingPositionY;

        public void PlaySelection()
        {
            RectTransform bottleTransform = (RectTransform)transform;
            restingPositionY = bottleTransform.anchoredPosition.y;
            float selectedPositionY = restingPositionY + bottleTransform.rect.height * selectionLiftRatio;

            AnimatePositionY(bottleTransform, selectedPositionY);
        }

        public void PlayDeselection()
        {
            AnimatePositionY((RectTransform)transform, restingPositionY);
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
