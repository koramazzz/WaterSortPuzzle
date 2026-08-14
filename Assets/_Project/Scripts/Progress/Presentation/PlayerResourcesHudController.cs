using UnityEngine;
using WaterSortPuzzle.Animations;
using WaterSortPuzzle.Configuration;

namespace WaterSortPuzzle.Progress.Presentation
{
    public sealed class PlayerResourcesHudController : MonoBehaviour
    {
        private const float RefreshIntervalSeconds = 1f;

        [SerializeField] private PlayerResourcesHudView resourcesHudView;
        [SerializeField] private HudFeedbackAnimator hudAnimator;

        private readonly PlayerResourcesService resourcesService =
            new PlayerResourcesService(new PlayerPrefsPlayerResourcesStore());

        private float nextRefreshTime;

        private void Start()
        {
            RefreshHudOnSchedule();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            RefreshHudOnSchedule();
        }

        public void RewardGold(int amount)
        {
            Show(resourcesService.AddGold(amount));
            hudAnimator.PlayChanged(PlayerResourceType.Gold);
        }

        public bool TrySpendGold(int amount)
        {
            bool spent = resourcesService.TrySpendGold(amount, out PlayerResources resources);

            Show(resources);
            PlayTransactionFeedback(spent, PlayerResourceType.Gold);

            return spent;
        }

        public bool TryConsumeLife()
        {
            bool consumed = resourcesService.TryConsumeLife(out PlayerResources resources);

            Show(resources);
            PlayTransactionFeedback(consumed, PlayerResourceType.Life);

            return consumed;
        }

        public bool CheckLifeAvailability()
        {
            PlayerResources resources = LoadAndShow();

            bool hasAvailableLife = resources.Lives > GameBalance.MinimumLives;

            if (!hasAvailableLife)
            {
                hudAnimator.PlayInsufficient(PlayerResourceType.Life);
            }

            return hasAvailableLife;
        }

        private void Show(PlayerResources resources)
        {
            resourcesHudView.Show(resources);
        }

        private PlayerResources LoadAndShow()
        {
            PlayerResources resources = resourcesService.Load();
            Show(resources);
            return resources;
        }

        private void PlayTransactionFeedback(
            bool succeeded,
            PlayerResourceType resourceType)
        {
            if (succeeded)
            {
                hudAnimator.PlayChanged(resourceType);
                return;
            }

            hudAnimator.PlayInsufficient(resourceType);
        }

        private void RefreshHudOnSchedule()
        {
            LoadAndShow();
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;
        }
    }
}
