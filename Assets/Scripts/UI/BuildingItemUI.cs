using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace KingdomClash.UI
{
    /// <summary>
    /// UI component for a building item in the shop
    /// </summary>
    public class BuildingItemUI : MonoBehaviour
    {
        [Header("Building Information")]
        [SerializeField] public string buildingName;
        [SerializeField] public string description;
        [SerializeField] public int woodCost;
        [SerializeField] public int stoneCost;
        [SerializeField] public int ironCost;
        [SerializeField] public GameObject buildingPrefab;
        
        [Header("UI Components")]
        [SerializeField] private Image buildingIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI woodCostText;
        [SerializeField] private TextMeshProUGUI stoneCostText;
        [SerializeField] private TextMeshProUGUI ironCostText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button itemButton;
        
        private Action<BuildingItemUI> onPurchaseCallback;
        private Action<BuildingItemUI> onSelectCallback;
        
        /// <summary>
        /// Initialize this building item with callbacks
        /// </summary>
        public void Initialize(Action<BuildingItemUI> onPurchase, Action<BuildingItemUI> onSelect)
        {
            this.onPurchaseCallback = onPurchase;
            this.onSelectCallback = onSelect;
            
            // Set UI texts
            if (nameText != null)
                nameText.text = buildingName;
                
            if (woodCostText != null)
                woodCostText.text = woodCost.ToString();
                
            if (stoneCostText != null)
                stoneCostText.text = stoneCost.ToString();
                
            if (ironCostText != null)
                ironCostText.text = ironCost.ToString();
            
            // Setup purchase button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
            
            // Setup item button for showing details
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnItemClicked);
            }
            else
            {
                // If no separate button is assigned, make the whole item clickable
                Button itemClickArea = GetComponent<Button>();
                if (itemClickArea != null)
                {
                    itemClickArea.onClick.RemoveAllListeners();
                    itemClickArea.onClick.AddListener(OnItemClicked);
                }
            }
            
            // Check if player can afford this building
            UpdateAvailability();
        }
          /// <summary>
        /// Updates the purchase button interactability based on resource availability
        /// </summary>
        public void UpdateAvailability()
        {
            if (purchaseButton == null || GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return;
                
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            Resources resources = gameData.resources;
            
            bool canAfford = resources.wood >= woodCost &&
                            resources.stone >= stoneCost &&
                            resources.iron >= ironCost;
            
            // Update button interactability
            purchaseButton.interactable = canAfford;
        }
        
        /// <summary>
        /// Called when the purchase button is clicked
        /// </summary>
        private void OnPurchaseClicked()
        {
            if (onPurchaseCallback != null)
            {
                onPurchaseCallback(this);
            }
        }
        
        /// <summary>
        /// Called when the item itself is clicked to show details
        /// </summary>
        private void OnItemClicked()
        {
            if (onSelectCallback != null)
            {
                onSelectCallback(this);
            }
        }
        
        /// <summary>
        /// Gets the building icon sprite
        /// </summary>
        public Sprite GetBuildingIcon()
        {
            return buildingIcon != null ? buildingIcon.sprite : null;
        }
    }
}
