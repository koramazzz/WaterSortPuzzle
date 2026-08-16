using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace WaterSortPuzzle.Animations
{
    public sealed class BottleGridLayoutAnimator : MonoBehaviour
    {
        [Header("Addition Animation")]
        [SerializeField, Min(0f)] private float additionDuration;
        [SerializeField] private Ease additionEase;

        private Sequence additionSequence;

        public void Play(
            RectTransform container,
            Vector2 targetBottleSize,
            IReadOnlyList<Vector2> targetPositions,
            RectTransform addedBottle)
        {
            CompleteAnimation();

            additionSequence = DOTween.Sequence();

            for (int bottleIndex = 0; bottleIndex < targetPositions.Count; bottleIndex++)
            {
                RectTransform bottle = (RectTransform)container.GetChild(bottleIndex);

                AnimateBottle(bottle, targetBottleSize, targetPositions[bottleIndex], addedBottle);
            }

            additionSequence
                .SetTarget(gameObject)
                .SetLink(gameObject)
                .OnComplete(() => additionSequence = null);
        }

        private void AnimateBottle(
            RectTransform bottle,
            Vector2 targetSize,
            Vector2 targetPosition,
            RectTransform addedBottle)
        {

            if (bottle == addedBottle)
            {
                AnimateAddedBottle(bottle, targetSize, targetPosition);
                return;
            }

            if (!AreApproximatelyEqual(bottle.anchoredPosition, targetPosition))
            {
                additionSequence.Join(bottle.DOAnchorPos(targetPosition, additionDuration).SetEase(additionEase));
            }

            if (!AreApproximatelyEqual(bottle.sizeDelta, targetSize))
            {
                additionSequence.Join(bottle.DOSizeDelta(targetSize, additionDuration).SetEase(additionEase));
            }
        }

        private void AnimateAddedBottle(
            RectTransform addedBottle,
            Vector2 targetSize,
            Vector2 targetPosition)
        {
            Vector3 restingScale = addedBottle.localScale;
            addedBottle.sizeDelta = targetSize;
            addedBottle.anchoredPosition = targetPosition;
            addedBottle.localScale = Vector3.zero;

            additionSequence.Join(addedBottle.DOScale(restingScale, additionDuration).SetEase(additionEase));
        }

        private static bool AreApproximatelyEqual(Vector2 first, Vector2 second)
        {
            return Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y);
        }

        private void CompleteAnimation()
        {
            additionSequence?.Complete();
            additionSequence = null;
        }
    }
}
