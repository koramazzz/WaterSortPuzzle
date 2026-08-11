using System;
using DG.Tweening;
using UnityEngine;
using WaterSortPuzzle.Progress;

namespace WaterSortPuzzle.Animations
{
    public sealed class HudFeedbackAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform goldHud;
        [SerializeField] private RectTransform lifeHud;
        [SerializeField, Min(1f)] private float changedScaleMultiplier;
        [SerializeField, Min(0f)] private float changedPhaseDuration;
        [SerializeField] private Ease changedEase;
        [SerializeField, Min(1f)] private float insufficientScaleMultiplier;
        [SerializeField, Min(0f)] private float insufficientPhaseDuration;
        [SerializeField] private Ease insufficientEase;

        private Tween feedbackTween;
        private RectTransform animatedHud;
        private Vector3 animatedHudRestingScale;

        public void PlayChanged(PlayerResourceType resourceType)
        {
            PlayFeedback(
                ResolveHud(resourceType),
                changedScaleMultiplier,
                changedPhaseDuration,
                changedEase);
        }

        public void PlayInsufficient(PlayerResourceType resourceType)
        {
            PlayFeedback(
                ResolveHud(resourceType),
                insufficientScaleMultiplier,
                insufficientPhaseDuration,
                insufficientEase);
        }

        private void PlayFeedback(
            RectTransform targetHud,
            float scaleMultiplier,
            float phaseDuration,
            Ease ease)
        {
            StopCurrentFeedback();
            animatedHud = targetHud;
            animatedHudRestingScale = targetHud.localScale;

            feedbackTween = DOTween.Sequence()
                .Append(
                    targetHud.DOScale(
                        animatedHudRestingScale * scaleMultiplier,
                        phaseDuration)
                    .SetEase(ease))
                .Append(
                    targetHud.DOScale(
                        animatedHudRestingScale,
                        phaseDuration)
                    .SetEase(ease))
                .SetTarget(targetHud)
                .SetLink(gameObject);
        }

        private void StopCurrentFeedback()
        {
            feedbackTween?.Kill();

            if (animatedHud == null)
            {
                return;
            }

            animatedHud.localScale = animatedHudRestingScale;
        }

        private RectTransform ResolveHud(PlayerResourceType resourceType)
        {
            return resourceType switch
            {
                PlayerResourceType.Gold => goldHud,
                PlayerResourceType.Life => lifeHud,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(resourceType),
                    resourceType,
                    null)
            };
        }
    }
}
