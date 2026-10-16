using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Animations
{
    [DisallowMultipleComponent]
    public sealed class BottlePourAnimator : MonoBehaviour
    {
        [Header("Liquid")]
        [SerializeField] private BottleLiquidGraphic liquidGraphic;

        [Header("Mouth")]
        [SerializeField] private RectTransform mouthOpening;

        [Header("Travel")]
        [SerializeField, Min(0f)] private float travelDuration;
        [SerializeField, Min(0f)] private float travelArcHeightRatio;
        [SerializeField] private Ease travelEase;

        [Header("Return")]
        [SerializeField, Min(0f)] private float returnDuration;
        [SerializeField, Min(0f)] private float returnArcHeightRatio;
        [SerializeField] private Ease returnEase;

        [Header("Liquid Impact")]
        [SerializeField, Min(0f)] private float impactWaveHeightRatio;
        [SerializeField, Range(0f, 1f)] private float impactWaveWidthRatio;
        [FormerlySerializedAs("wobbleDuration")]
        [SerializeField, Min(0f)] private float impactWaveDuration;
        [FormerlySerializedAs("wobbleOscillationCount")]
        [SerializeField, Min(0f)] private float impactWaveCycleCount;
        [FormerlySerializedAs("wobbleEase")]
        [SerializeField] private Ease impactWaveEase;

        [Header("Liquid Interaction")]
        [SerializeField, Min(0f)] private float interactionWaveHeightRatio;
        [SerializeField, Range(0f, 1f)] private float interactionWaveWidthRatio;
        [SerializeField, Min(0f)] private float interactionWaveDuration;
        [SerializeField, Min(0f)] private float interactionWaveCycleCount;
        [SerializeField] private Ease interactionWaveEase;

        private RectTransform bottleTransform;
        private Tween surfaceWaveTween;
        private Vector2 restingPosition;
        private Vector2 returnStartPosition;
        private float returnStartAngle;
        private int siblingIndex;
        private bool isPouring;

        public RectTransform RectTransform => GetBottleTransform();

        internal float LiquidSurfaceAngle => liquidGraphic.SurfaceAngle;

        internal Vector2 GetLocalMouthCenter()
        {
            RectTransform opening = GetMouthOpening();
            return GetLocalMouthPosition(
                opening,
                opening.rect.center.x);
        }

        internal Vector2 GetLocalPouringMouthPosition(float direction)
        {
            RectTransform opening = GetMouthOpening();
            float mouthPositionX = direction < 0f
                ? opening.rect.xMax
                : opening.rect.xMin;
            return GetLocalMouthPosition(opening, mouthPositionX);
        }

        public void Begin(Vector2 sourceRestingPosition)
        {
            if (liquidGraphic == null)
            {
                throw new InvalidOperationException(
                    "A bottle liquid graphic is required.");
            }

            if (isPouring)
            {
                throw new InvalidOperationException(
                    "The bottle is already playing a pour animation.");
            }

            Vector3 mouthWorldPosition = GetMouthWorldPosition();
            bottleTransform = GetBottleTransform();
            restingPosition = sourceRestingPosition;
            siblingIndex = bottleTransform.GetSiblingIndex();
            bottleTransform.SetAsLastSibling();
            StopSurfaceWave();
            liquidGraphic.BeginPour(mouthWorldPosition);
            isPouring = true;
        }

        public Tween CreateTravelTween(
            Vector2 targetMouthPosition,
            Vector2 localMouthPosition,
            float targetAngle)
        {
            EnsurePouring();
            Vector2 startPosition = bottleTransform.anchoredPosition;
            Vector2 targetPosition = CalculateBottlePosition(
                targetMouthPosition,
                localMouthPosition,
                targetAngle);
            float startAngle = bottleTransform.localEulerAngles.z;
            float arcHeight = bottleTransform.rect.height *
                travelArcHeightRatio;

            return DOVirtual
                .Float(0f, 1f, travelDuration, progress =>
                {
                    bottleTransform.anchoredPosition = CalculateArcPosition(
                        startPosition,
                        targetPosition,
                        arcHeight,
                        progress);
                    SetRotation(Mathf.LerpAngle(
                        startAngle,
                        targetAngle,
                        progress));
                })
                .SetEase(travelEase)
                .SetTarget(this);
        }

        public void ApplyPourPose(
            Vector2 mouthPosition,
            Vector2 localMouthPosition,
            float angle)
        {
            EnsurePouring();
            bottleTransform.anchoredPosition = CalculateBottlePosition(
                mouthPosition,
                localMouthPosition,
                angle);
            SetRotation(angle);
        }

        public void CaptureReturnPose()
        {
            EnsurePouring();
            returnStartPosition = bottleTransform.anchoredPosition;
            returnStartAngle = bottleTransform.localEulerAngles.z;
        }

        public Tween CreateReturnTween()
        {
            EnsurePouring();
            float arcHeight = bottleTransform.rect.height *
                returnArcHeightRatio;

            return DOVirtual
                .Float(0f, 1f, returnDuration, progress =>
                {
                    bottleTransform.anchoredPosition = CalculateArcPosition(
                        returnStartPosition,
                        restingPosition,
                        arcHeight,
                        progress);
                    SetRotation(Mathf.LerpAngle(
                        returnStartAngle,
                        0f,
                        progress));
                })
                .SetEase(returnEase)
                .SetTarget(this);
        }

        public void Complete()
        {
            Restore();
            PlayLiquidImpact();
        }

        public void Cancel()
        {
            Restore();
        }

        public void PlayLiquidImpact()
        {
            PlaySurfaceWave(
                impactWaveHeightRatio,
                impactWaveWidthRatio,
                impactWaveDuration,
                impactWaveCycleCount,
                impactWaveEase);
        }

        public void PlayLiquidInteraction()
        {
            PlaySurfaceWave(
                interactionWaveHeightRatio,
                interactionWaveWidthRatio,
                interactionWaveDuration,
                interactionWaveCycleCount,
                interactionWaveEase);
        }

        private RectTransform GetBottleTransform()
        {
            bottleTransform = bottleTransform != null
                ? bottleTransform
                : (RectTransform)transform;
            return bottleTransform;
        }

        private RectTransform GetMouthOpening()
        {
            if (mouthOpening == null)
            {
                throw new InvalidOperationException(
                    "A bottle mouth opening is required.");
            }

            return mouthOpening;
        }

        private Vector3 GetMouthWorldPosition()
        {
            RectTransform opening = GetMouthOpening();
            return opening.TransformPoint(opening.rect.center);
        }

        private Vector2 GetLocalMouthPosition(
            RectTransform opening,
            float mouthPositionX)
        {
            Vector3 worldPosition = opening.TransformPoint(
                new Vector2(
                    mouthPositionX,
                    opening.rect.center.y));
            return RectTransform.InverseTransformPoint(worldPosition);
        }

        private Vector2 CalculateBottlePosition(
            Vector2 mouthPosition,
            Vector2 localMouthPosition,
            float angle)
        {
            Vector2 rotatedMouth = Quaternion.Euler(0f, 0f, angle) *
                localMouthPosition;
            return mouthPosition - rotatedMouth;
        }

        private static Vector2 CalculateArcPosition(
            Vector2 start,
            Vector2 destination,
            float arcHeight,
            float progress)
        {
            Vector2 position = Vector2.LerpUnclamped(
                start,
                destination,
                progress);
            position.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;
            return position;
        }

        private void SetRotation(float angle)
        {
            float signedAngle = Mathf.DeltaAngle(0f, angle);
            bottleTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                signedAngle);
            liquidGraphic.SetBottleAngle(signedAngle);
        }

        private void Restore()
        {
            if (!isPouring)
            {
                return;
            }

            bottleTransform.anchoredPosition = restingPosition;
            SetRotation(0f);
            liquidGraphic.EndPour();
            bottleTransform.SetSiblingIndex(siblingIndex);
            isPouring = false;
        }

        private void PlaySurfaceWave(
            float heightRatio,
            float maximumWidthRatio,
            float duration,
            float cycleCount,
            Ease ease)
        {
            if (liquidGraphic == null)
            {
                throw new InvalidOperationException(
                    "A bottle liquid graphic is required.");
            }

            StopSurfaceWave();

            if (Mathf.Approximately(duration, 0f) ||
                Mathf.Approximately(heightRatio, 0f) ||
                Mathf.Approximately(maximumWidthRatio, 0f) ||
                Mathf.Approximately(cycleCount, 0f))
            {
                liquidGraphic.ClearSurfaceWave();
                return;
            }

            Rect liquidBounds = liquidGraphic.rectTransform.rect;
            float waveCenterRatio = Mathf.InverseLerp(
                liquidBounds.xMin,
                liquidBounds.xMax,
                liquidBounds.center.x);
            float waveHeight = liquidBounds.height * heightRatio;

            surfaceWaveTween = DOVirtual
                .Float(0f, 1f, duration, progress =>
                {
                    float clampedProgress = Mathf.Clamp01(progress);
                    float damping = 1f - clampedProgress;
                    float widthRatio = maximumWidthRatio *
                        Mathf.SmoothStep(0f, 1f, clampedProgress);
                    liquidGraphic.SetSurfaceWave(
                        waveCenterRatio,
                        widthRatio,
                        waveHeight * damping,
                        cycleCount,
                        clampedProgress * cycleCount);
                })
                .SetEase(ease)
                .SetTarget(this)
                .SetLink(gameObject)
                .OnComplete(liquidGraphic.ClearSurfaceWave);
        }

        private void StopSurfaceWave()
        {
            surfaceWaveTween?.Kill();
            surfaceWaveTween = null;
            liquidGraphic?.ClearSurfaceWave();
        }

        private void EnsurePouring()
        {
            if (!isPouring)
            {
                throw new InvalidOperationException(
                    "Begin must be called before animating a bottle pour.");
            }
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
            StopSurfaceWave();
            Restore();
        }
    }
}
