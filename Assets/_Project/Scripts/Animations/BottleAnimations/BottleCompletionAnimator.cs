using System;
using Coffee.UIExtensions;
using UnityEngine;

namespace WaterSortPuzzle.Animations
{
    public sealed class BottleCompletionAnimator : MonoBehaviour
    {
        [SerializeField] private BottleCapAnimator capAnimator;
        [SerializeField] private UIParticle completionSparkle;

        public void Hide()
        {
            capAnimator.Hide();
            StopCompletionSparkle();
        }

        public void PlayCompletion(Action completed = null)
        {
            StopCompletionSparkle();
            capAnimator.PlayClosing(() => Complete(completed));
        }

        private void Complete(Action completed)
        {
            completionSparkle.Play();
            completed?.Invoke();
        }

        private void StopCompletionSparkle()
        {
            completionSparkle.Stop();
            completionSparkle.Clear();
        }
    }
}
