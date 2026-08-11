using UnityEngine;
using WaterSortPuzzle.Animations;

namespace WaterSortPuzzle.Progress.Presentation
{
    public sealed class PlayerResourcesHudController : MonoBehaviour
    {
        private const float RefreshIntervalSeconds = 1f;

        [SerializeField] private PlayerResourcesHudView resourcesHudView;
        [SerializeField] private HudFeedbackAnimator hudAnimator;

        private readonly PlayerPrefsPlayerResourcesStore resourcesStore =
            new PlayerPrefsPlayerResourcesStore();

        private float nextRefreshTime;

        private void Start()
        {
            RefreshOnSchedule();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            RefreshOnSchedule();
        }

        public PlayerResources Refresh()
        {
            return Show(resourcesStore.Load());
        }

        public PlayerResources AddGold(int amount)
        {
            PlayerResources resources = Show(resourcesStore.AddGold(amount));
            hudAnimator.PlayChanged(PlayerResourceType.Gold);
            return resources;
        }

        public PlayerResources ConsumeLife()
        {
            PlayerResources resources = Show(resourcesStore.ConsumeLife());
            hudAnimator.PlayChanged(PlayerResourceType.Life);
            return resources;
        }

        public void PlayInsufficientFeedback(PlayerResourceType resourceType)
        {
            hudAnimator.PlayInsufficient(resourceType);
        }

        private PlayerResources Show(PlayerResources resources)
        {
            resourcesHudView.Show(resources);
            return resources;
        }

        private void RefreshOnSchedule()
        {
            Refresh();
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;
        }
    }
}
