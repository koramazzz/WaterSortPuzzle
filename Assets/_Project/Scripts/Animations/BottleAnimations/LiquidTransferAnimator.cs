using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using WaterSortPuzzle.Gameplay.Bottles.Presentation;

namespace WaterSortPuzzle.Animations
{
    public readonly struct LiquidTransferAnimationRequest
    {
        public LiquidTransferAnimationRequest(
            BottlePourAnimator sourceAnimator,
            BottlePourAnimator destinationAnimator,
            Vector2 sourceRestingPosition,
            Rect sourceLiquidBounds,
            Rect destinationLiquidBounds,
            int capacity,
            int sourceLiquidCount,
            int destinationLiquidCount,
            int pouredLiquidCount,
            Color liquidColor)
        {
            SourceAnimator = sourceAnimator;
            DestinationAnimator = destinationAnimator;
            SourceRestingPosition = sourceRestingPosition;
            SourceLiquidBounds = sourceLiquidBounds;
            DestinationLiquidBounds = destinationLiquidBounds;
            Capacity = capacity;
            SourceLiquidCount = sourceLiquidCount;
            DestinationLiquidCount = destinationLiquidCount;
            PouredLiquidCount = pouredLiquidCount;
            LiquidColor = liquidColor;
        }

        public BottlePourAnimator SourceAnimator { get; }

        public BottlePourAnimator DestinationAnimator { get; }

        public RectTransform Destination => DestinationAnimator?.RectTransform;

        public Vector2 SourceRestingPosition { get; }

        public Rect SourceLiquidBounds { get; }

        public Rect DestinationLiquidBounds { get; }

        public int Capacity { get; }

        public int SourceLiquidCount { get; }

        public int DestinationLiquidCount { get; }

        public int PouredLiquidCount { get; }

        public Color LiquidColor { get; }
    }

    [DisallowMultipleComponent]
    public sealed class LiquidTransferAnimator : MonoBehaviour
    {
        [Header("Position")]
        [FormerlySerializedAs("mouthGapRatio")]
        [SerializeField, Min(0f)] private float mouthVerticalGapRatio;
        [SerializeField, Min(0f)] private float mouthHorizontalGapRatio;

        [Header("Transfer")]
        [FormerlySerializedAs("basePourDuration")]
        [SerializeField, Min(0f)] private float streamEnterDuration;
        [FormerlySerializedAs("pourDurationPerLiquid")]
        [SerializeField, Min(0f)] private float transferDurationPerLiquid;
        [SerializeField, Min(0f)] private float streamExitDuration;
        [SerializeField] private Ease pourEase;

        [Header("Stream")]
        [SerializeField, Min(0f)] private float streamWidthRatio;
        [SerializeField, Min(0f)] private float streamFlowCyclesPerSecond;
        [SerializeField] private PourStreamView streamView;

        private Sequence transferSequence;
        private BottlePourAnimator animatedSource;
        private Action animationCanceled;

        public void Play(
            LiquidTransferAnimationRequest request,
            Action pourStarted,
            Action<float, float> liquidProgressed,
            Action destinationImpacted,
            Action liquidTransferred,
            Action completed,
            Action canceled = null)
        {
            Validate(request);

            if (transferSequence != null && transferSequence.IsActive())
            {
                throw new InvalidOperationException(
                    "A liquid transfer animation is already playing.");
            }

            BottlePourAnimator sourceAnimator = request.SourceAnimator;
            RectTransform source = sourceAnimator.RectTransform;
            RectTransform sourceParent = (RectTransform)source.parent;
            Vector2 destinationMouth = GetMouthPosition(
                request.DestinationAnimator,
                sourceParent);
            float direction = ResolvePourDirection(
                source,
                request.Destination,
                sourceParent);
            Vector2 pouringMouth = destinationMouth +
                Vector2.up * request.Destination.rect.height *
                mouthVerticalGapRatio +
                Vector2.right * request.Destination.rect.width *
                mouthHorizontalGapRatio * direction;
            Vector2 sourceMouth =
                sourceAnimator.GetLocalPouringMouthPosition(direction);
            float startAngle = CalculatePourAngle(
                source,
                request.SourceLiquidBounds,
                request.SourceLiquidCount,
                request.Capacity,
                direction);
            float transferDuration =
                transferDurationPerLiquid * request.PouredLiquidCount;
            float streamWidth = source.rect.width * streamWidthRatio;

            try
            {
                sourceAnimator.Begin(request.SourceRestingPosition);
                animationCanceled = canceled;
                animatedSource = sourceAnimator;
                transferSequence = DOTween.Sequence();
                transferSequence.Append(sourceAnimator.CreateTravelTween(
                    pouringMouth,
                    sourceMouth,
                    startAngle));
                transferSequence.AppendCallback(() =>
                {
                    ShowStream(
                        sourceAnimator,
                        sourceMouth,
                        request,
                        request.SourceLiquidCount,
                        0f);
                    streamView.SetWidth(0f);
                    streamView.SetVisibleRange(0f, 0f);
                    liquidProgressed?.Invoke(0f, 0f);
                    pourStarted?.Invoke();
                });
                transferSequence.Append(CreatePourTween(
                    sourceAnimator,
                    pouringMouth,
                    sourceMouth,
                    request,
                    direction,
                    transferDuration,
                    streamWidth,
                    liquidProgressed,
                    destinationImpacted));
                transferSequence.AppendCallback(() =>
                {
                    streamView.Hide();
                    liquidTransferred?.Invoke();
                    sourceAnimator.CaptureReturnPose();
                });
                transferSequence.Append(sourceAnimator.CreateReturnTween());
                transferSequence
                    .SetTarget(gameObject)
                    .SetLink(gameObject)
                    .OnComplete(() => CompleteAnimation(completed));
            }
            catch
            {
                CancelAnimation(false);
                throw;
            }
        }

        private Tween CreatePourTween(
            BottlePourAnimator sourceAnimator,
            Vector2 pouringMouth,
            Vector2 sourceMouth,
            LiquidTransferAnimationRequest request,
            float direction,
            float transferDuration,
            float streamWidth,
            Action<float, float> liquidProgressed,
            Action destinationImpacted)
        {
            bool hasDestinationImpactStarted = false;
            float sourceDuration = streamEnterDuration + transferDuration;
            float destinationDuration = transferDuration + streamExitDuration;
            float totalDuration = streamEnterDuration +
                transferDuration +
                streamExitDuration;

            return DOVirtual
                .Float(0f, totalDuration, totalDuration, elapsedTime =>
                {
                    float sourceProgress =
                        CalculateProgress(
                            elapsedTime,
                            0f,
                            sourceDuration);
                    float destinationProgress =
                        CalculateProgress(
                            elapsedTime,
                            streamEnterDuration,
                            destinationDuration);
                    float streamHeadProgress = CalculateProgress(
                        elapsedTime,
                        0f,
                        streamEnterDuration);
                    float streamTailProgress = CalculateProgress(
                        elapsedTime,
                        streamEnterDuration + transferDuration,
                        streamExitDuration);
                    float liquidCount = request.SourceLiquidCount -
                        request.PouredLiquidCount * sourceProgress;
                    float angle = CalculatePourAngle(
                        sourceAnimator.RectTransform,
                        request.SourceLiquidBounds,
                        liquidCount,
                        request.Capacity,
                        direction);
                    sourceAnimator.ApplyPourPose(
                        pouringMouth,
                        sourceMouth,
                        angle);
                    UpdateStreamPath(
                        sourceAnimator,
                        sourceMouth,
                        request,
                        liquidCount,
                        destinationProgress);
                    streamView.SetFlowPhase(
                        elapsedTime * streamFlowCyclesPerSecond);
                    streamView.SetVisibleRange(
                        streamTailProgress,
                        streamHeadProgress);
                    streamView.SetWidth(
                        streamWidth * CalculateStreamWidthScale(
                            streamHeadProgress,
                            streamTailProgress));
                    liquidProgressed?.Invoke(
                        sourceProgress,
                        destinationProgress);

                    if (!hasDestinationImpactStarted &&
                        destinationProgress > 0f)
                    {
                        hasDestinationImpactStarted = true;
                        destinationImpacted?.Invoke();
                    }
                })
                .SetEase(pourEase);
        }

        private void ShowStream(
            BottlePourAnimator sourceAnimator,
            Vector2 sourceMouth,
            LiquidTransferAnimationRequest request,
            float sourceLiquidCount,
            float destinationProgress)
        {
            CalculateStreamPath(
                sourceAnimator,
                sourceMouth,
                request,
                sourceLiquidCount,
                destinationProgress,
                out Vector3 sourceInterior,
                out Vector3 sourceMouthPosition,
                out Vector3 destinationMouth,
                out Vector3 destinationInterior);
            streamView.Show(
                sourceInterior,
                sourceMouthPosition,
                destinationMouth,
                destinationInterior,
                request.LiquidColor);
        }

        private void UpdateStreamPath(
            BottlePourAnimator sourceAnimator,
            Vector2 sourceMouth,
            LiquidTransferAnimationRequest request,
            float sourceLiquidCount,
            float destinationProgress)
        {
            CalculateStreamPath(
                sourceAnimator,
                sourceMouth,
                request,
                sourceLiquidCount,
                destinationProgress,
                out Vector3 sourceInterior,
                out Vector3 sourceMouthPosition,
                out Vector3 destinationMouth,
                out Vector3 destinationInterior);
            streamView.UpdatePath(
                sourceInterior,
                sourceMouthPosition,
                destinationMouth,
                destinationInterior);
        }

        private static void CalculateStreamPath(
            BottlePourAnimator sourceAnimator,
            Vector2 sourceMouth,
            LiquidTransferAnimationRequest request,
            float sourceLiquidCount,
            float destinationProgress,
            out Vector3 sourceInterior,
            out Vector3 sourceMouthPosition,
            out Vector3 destinationMouth,
            out Vector3 destinationInterior)
        {
            RectTransform source = sourceAnimator.RectTransform;
            Vector2 sourceInteriorPosition = CalculateSourceSurfacePosition(
                sourceAnimator.LiquidSurfaceAngle,
                sourceMouth,
                request.SourceLiquidBounds,
                sourceLiquidCount,
                request.Capacity);
            Vector2 destinationMouthPosition =
                request.DestinationAnimator.GetLocalMouthCenter();
            float destinationLiquidCount =
                request.DestinationLiquidCount +
                request.PouredLiquidCount * destinationProgress;
            float destinationFillRatio = Mathf.Clamp01(
                destinationLiquidCount / request.Capacity);
            Vector2 destinationInteriorPosition = new Vector2(
                request.DestinationLiquidBounds.center.x,
                Mathf.Lerp(
                    request.DestinationLiquidBounds.yMin,
                    request.DestinationLiquidBounds.yMax,
                    destinationFillRatio));

            sourceInterior = source.TransformPoint(sourceInteriorPosition);
            sourceMouthPosition = source.TransformPoint(sourceMouth);
            destinationMouth = request.Destination.TransformPoint(
                destinationMouthPosition);
            destinationInterior = request.Destination.TransformPoint(
                destinationInteriorPosition);
        }

        private static float CalculatePourAngle(
            RectTransform source,
            Rect liquidBounds,
            float liquidCount,
            int capacity,
            float direction)
        {
            float fillRatio = Mathf.Clamp01(liquidCount / capacity);
            return LiquidSurfaceGeometry.CalculateSpillAngle(
                    source.rect,
                    liquidBounds,
                    fillRatio) *
                direction;
        }

        private static Vector2 CalculateSourceSurfacePosition(
            float surfaceAngle,
            Vector2 sourceMouth,
            Rect liquidBounds,
            float liquidCount,
            int capacity)
        {
            Rect renderingBounds = LiquidSurfaceGeometry.ExtendTop(
                liquidBounds,
                sourceMouth.y);
            float relativeSurfaceAngle = Mathf.Clamp(
                -surfaceAngle,
                -LiquidSurfaceGeometry.MaximumSurfaceAngle,
                LiquidSurfaceGeometry.MaximumSurfaceAngle);
            float surfaceSlope = Mathf.Tan(
                relativeSurfaceAngle * Mathf.Deg2Rad);
            float fillRatio =
                LiquidSurfaceGeometry.CalculateAreaPreservingFillRatio(
                    liquidBounds,
                    renderingBounds,
                    liquidCount / capacity);
            float surfaceIntercept =
                LiquidSurfaceGeometry.CalculateBoundaryIntercept(
                    renderingBounds,
                    surfaceSlope,
                    fillRatio);
            float surfacePositionX = Mathf.Clamp(
                sourceMouth.x,
                renderingBounds.xMin,
                renderingBounds.xMax);
            float surfacePositionY = Mathf.Clamp(
                surfaceSlope * surfacePositionX + surfaceIntercept,
                renderingBounds.yMin,
                renderingBounds.yMax);
            return new Vector2(surfacePositionX, surfacePositionY);
        }

        private static float CalculateStreamWidthScale(
            float headProgress,
            float tailProgress)
        {
            float enteringScale = Mathf.SmoothStep(
                0f,
                1f,
                headProgress);
            float exitingScale = 1f - Mathf.SmoothStep(
                0f,
                1f,
                tailProgress);
            return Mathf.Min(enteringScale, exitingScale);
        }

        private static float CalculateProgress(
            float elapsedTime,
            float startTime,
            float duration)
        {
            if (Mathf.Approximately(duration, 0f))
            {
                return elapsedTime >= startTime ? 1f : 0f;
            }

            return Mathf.Clamp01((elapsedTime - startTime) / duration);
        }

        private static Vector2 GetMouthPosition(
            BottlePourAnimator bottleAnimator,
            RectTransform targetSpace)
        {
            RectTransform bottle = bottleAnimator.RectTransform;
            Vector3 worldPosition = bottle.TransformPoint(
                bottleAnimator.GetLocalMouthCenter());
            return targetSpace.InverseTransformPoint(worldPosition);
        }

        private static float ResolvePourDirection(
            RectTransform source,
            RectTransform destination,
            RectTransform sourceParent)
        {
            float horizontalDifference =
                destination.anchoredPosition.x - source.anchoredPosition.x;
            float combinedHalfWidth =
                (source.rect.width + destination.rect.width) * 0.5f;
            bool bottleBoundsOverlap =
                Mathf.Abs(horizontalDifference) < combinedHalfWidth;

            if (!bottleBoundsOverlap)
            {
                return horizontalDifference > 0f ? -1f : 1f;
            }

            float sharedColumnCenter =
                (source.anchoredPosition.x + destination.anchoredPosition.x) *
                0.5f;
            return sharedColumnCenter <= sourceParent.rect.center.x
                ? 1f
                : -1f;
        }

        private void CompleteAnimation(Action completed)
        {
            BottlePourAnimator source = animatedSource;
            animatedSource = null;
            transferSequence = null;
            animationCanceled = null;
            source.Complete();
            completed?.Invoke();
        }

        private void OnDisable()
        {
            CancelAnimation(true);
        }

        private void CancelAnimation(bool notifyCancellation)
        {
            BottlePourAnimator source = animatedSource;
            Action canceled = animationCanceled;
            transferSequence?.Kill();
            transferSequence = null;
            animatedSource = null;
            animationCanceled = null;
            streamView?.Hide();
            source?.Cancel();

            if (notifyCancellation && source != null)
            {
                canceled?.Invoke();
            }
        }

        private void Validate(LiquidTransferAnimationRequest request)
        {
            if (request.SourceAnimator == null)
            {
                throw new ArgumentNullException(nameof(request.SourceAnimator));
            }

            if (request.DestinationAnimator == null)
            {
                throw new ArgumentNullException(
                    nameof(request.DestinationAnimator));
            }

            request.SourceAnimator.GetLocalMouthCenter();
            request.DestinationAnimator.GetLocalMouthCenter();

            RectTransform source = request.SourceAnimator.RectTransform;

            if (!(source.parent is RectTransform))
            {
                throw new ArgumentException(
                    "Source and destination bottles must have a RectTransform " +
                    "parent.");
            }

            if (ReferenceEquals(source, request.Destination))
            {
                throw new ArgumentException(
                    "Source and destination bottles must be different.");
            }

            if (source.parent != request.Destination.parent)
            {
                throw new ArgumentException(
                    "Source and destination bottles must share a parent.");
            }

            if (request.Capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Capacity));
            }

            if (request.SourceLiquidCount <= 0 ||
                request.SourceLiquidCount > request.Capacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.SourceLiquidCount));
            }

            if (request.DestinationLiquidCount < 0 ||
                request.DestinationLiquidCount > request.Capacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.DestinationLiquidCount));
            }

            if (request.PouredLiquidCount <= 0 ||
                request.PouredLiquidCount > request.SourceLiquidCount ||
                request.DestinationLiquidCount +
                    request.PouredLiquidCount > request.Capacity)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.PouredLiquidCount));
            }

            LiquidSurfaceGeometry.CalculateSpillAngle(
                source.rect,
                request.SourceLiquidBounds,
                request.SourceLiquidCount / (float)request.Capacity);

            if (streamView == null)
            {
                throw new InvalidOperationException(
                    "A pour stream view is required.");
            }
        }
    }
}
