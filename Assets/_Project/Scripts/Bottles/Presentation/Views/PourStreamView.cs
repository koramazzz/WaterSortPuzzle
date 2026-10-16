using System;
using UnityEngine;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PourStreamView : MonoBehaviour
    {
        [SerializeField] private PourStreamGraphic streamGraphic;

        public void Show(
            Vector3 sourceInteriorWorldPosition,
            Vector3 sourceMouthWorldPosition,
            Vector3 destinationMouthWorldPosition,
            Vector3 destinationInteriorWorldPosition,
            Color color)
        {
            UpdatePath(
                sourceInteriorWorldPosition,
                sourceMouthWorldPosition,
                destinationMouthWorldPosition,
                destinationInteriorWorldPosition);

            streamGraphic.SetFlowPhase(0f);
            streamGraphic.color = color;
            gameObject.SetActive(true);
        }

        public void UpdatePath(
            Vector3 sourceInteriorWorldPosition,
            Vector3 sourceMouthWorldPosition,
            Vector3 destinationMouthWorldPosition,
            Vector3 destinationInteriorWorldPosition)
        {
            EnsureStreamGraphic();
            RectTransform coordinateSpace = streamGraphic.rectTransform;
            Vector2 sourceInteriorPosition =
                coordinateSpace.InverseTransformPoint(
                    sourceInteriorWorldPosition);
            Vector2 sourceMouthPosition =
                coordinateSpace.InverseTransformPoint(
                    sourceMouthWorldPosition);
            Vector2 destinationMouthPosition =
                coordinateSpace.InverseTransformPoint(
                    destinationMouthWorldPosition);
            Vector2 destinationInteriorPosition =
                coordinateSpace.InverseTransformPoint(
                    destinationInteriorWorldPosition);
            streamGraphic.SetPath(
                sourceInteriorPosition,
                sourceMouthPosition,
                destinationMouthPosition,
                destinationInteriorPosition);
        }

        public void SetWidth(float width)
        {
            if (width < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            EnsureStreamGraphic();
            streamGraphic.SetWidth(width);
        }

        public void SetFlowPhase(float phase)
        {
            if (phase < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(phase));
            }

            EnsureStreamGraphic();
            streamGraphic.SetFlowPhase(phase);
        }

        public void SetVisibleRange(float startProgress, float endProgress)
        {
            if (startProgress < 0f || startProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(startProgress));
            }

            if (endProgress < 0f || endProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(endProgress));
            }

            if (startProgress > endProgress)
            {
                throw new ArgumentException(
                    "Stream start progress cannot exceed its end progress.");
            }

            EnsureStreamGraphic();
            streamGraphic.SetVisibleRange(startProgress, endProgress);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void EnsureStreamGraphic()
        {
            if (streamGraphic == null)
            {
                throw new InvalidOperationException(
                    "A pour stream graphic is required.");
            }
        }
    }
}
