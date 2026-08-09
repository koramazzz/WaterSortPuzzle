using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Gameplay.Bottles.Presentation
{
    public sealed class LiquidSlotView : MonoBehaviour
    {
        [SerializeField] private Image liquidImage;
        [SerializeField] private GameObject hiddenVisual;

        public void ShowLiquid(Color color, bool isHidden)
        {
            liquidImage.color = color;
            liquidImage.enabled = !isHidden;
            hiddenVisual.SetActive(isHidden);
        }

        public void ShowEmpty()
        {
            liquidImage.enabled = false;
            hiddenVisual.SetActive(false);
        }
    }
}
