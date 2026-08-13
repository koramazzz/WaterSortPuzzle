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

        public void PlayCompletion()
        {
            StopCompletionSparkle();
            capAnimator.PlayClosing(completionSparkle.Play);
        }

        private void StopCompletionSparkle()
        {
            completionSparkle.Stop();
            completionSparkle.Clear();
        }
    }
}
