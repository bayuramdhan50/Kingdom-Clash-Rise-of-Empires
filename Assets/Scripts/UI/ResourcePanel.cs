using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the resource display panel within the HUD
    /// </summary>
    public class ResourcePanel : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceDisplay
        {
            public Image resourceIcon;
            public TextMeshProUGUI resourceText;
        }

        [Header("Wood Resource Display")]
        [SerializeField] private ResourceDisplay woodDisplay;

        [Header("Stone Resource Display")]
        [SerializeField] private ResourceDisplay stoneDisplay;

        [Header("Iron Resource Display")]
        [SerializeField] private ResourceDisplay ironDisplay;

        [Header("Food Resource Display")]
        [SerializeField] private ResourceDisplay foodDisplay;

        /// <summary>
        /// Updates all resource displays with the provided resource data
        /// </summary>
        /// <param name="resources">The current resources data</param>
        public void UpdateResourceDisplay(Resources resources)
        {
            if (resources == null) return;

            // Update wood resource
            if (woodDisplay != null && woodDisplay.resourceText != null)
            {
                woodDisplay.resourceText.text = resources.wood.ToString();
            }
            
            // Update stone resource
            if (stoneDisplay != null && stoneDisplay.resourceText != null)
            {
                stoneDisplay.resourceText.text = resources.stone.ToString();
            }
            
            // Update iron resource
            if (ironDisplay != null && ironDisplay.resourceText != null)
            {
                ironDisplay.resourceText.text = resources.iron.ToString();
            }
            
            // Update food resource
            if (foodDisplay != null && foodDisplay.resourceText != null)
            {
                foodDisplay.resourceText.text = resources.food.ToString();
            }
        }
        
        /// <summary>
        /// Set default values for the resource display
        /// </summary>
        public void SetDefaultValues()
        {
            if (woodDisplay != null && woodDisplay.resourceText != null)
            {
                woodDisplay.resourceText.text = "0";
            }
            
            if (stoneDisplay != null && stoneDisplay.resourceText != null)
            {
                stoneDisplay.resourceText.text = "0";
            }
            
            if (ironDisplay != null && ironDisplay.resourceText != null)
            {
                ironDisplay.resourceText.text = "0";
            }
            
            if (foodDisplay != null && foodDisplay.resourceText != null)
            {
                foodDisplay.resourceText.text = "0";
            }
        }
        
        /// <summary>
        /// Updates all resource displays using the current game data
        /// </summary>
        public void UpdateResourceDisplay()
        {
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return;
                
            UpdateResourceDisplay(GameManager.Instance.GetCurrentGameData().resources);
        }
    }
}
