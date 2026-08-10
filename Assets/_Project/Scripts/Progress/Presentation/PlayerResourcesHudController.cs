using UnityEngine;

namespace WaterSortPuzzle.Progress.Presentation
{
    public sealed class PlayerResourcesHudController : MonoBehaviour
    {
        private const float RefreshIntervalSeconds = 1f;

        [SerializeField] private PlayerResourcesHudView resourcesHudView;

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
            return Show(resourcesStore.AddGold(amount));
        }

        public PlayerResources ConsumeLife()
        {
            return Show(resourcesStore.ConsumeLife());
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
