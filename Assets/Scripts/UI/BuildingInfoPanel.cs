using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingdomClash.UI
{
    /// <summary>
    /// Panel that shows detailed information about a selected building
    /// </summary>
    public class BuildingInfoPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image buildingImage;
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI woodCostText;
        [SerializeField] private TextMeshProUGUI stoneCostText;
        [SerializeField] private TextMeshProUGUI ironCostText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button purchaseButton;
        
        private BuildingItemUI currentBuildingItem;
        private ShopBuildingPanel shopPanel;
        
        private void Awake()
        {
            // Add listener to close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
            
            // Add listener to purchase button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(PurchaseBuilding);
            }
            
            // Hide panel initially
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Set the reference to the shop panel
        /// </summary>
        /// <param name="panel">The ShopBuildingPanel</param>
        public void SetShopPanel(ShopBuildingPanel panel)
        {
            shopPanel = panel;
        }
        
        /// <summary>
        /// Display building info in the panel
        /// </summary>
        /// <param name="buildingItem">The building item to display</param>
        public void ShowBuildingInfo(BuildingItemUI buildingItem)
        {
            if (buildingItem == null)
                return;
            
            currentBuildingItem = buildingItem;
            
            // Update UI elements
            if (buildingNameText != null)
                buildingNameText.text = buildingItem.buildingName;
                
            if (descriptionText != null)
                descriptionText.text = buildingItem.description;
                
            if (buildingImage != null)
                buildingImage.sprite = buildingItem.GetBuildingIcon();
                
            if (woodCostText != null)
                woodCostText.text = buildingItem.woodCost.ToString();
                
            if (stoneCostText != null)
                stoneCostText.text = buildingItem.stoneCost.ToString();
                
            if (ironCostText != null)
                ironCostText.text = buildingItem.ironCost.ToString();
            
            // Update purchase button availability
            UpdatePurchaseButtonState();
            
            // Show the panel
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Update the purchase button interactability based on available resources
        /// </summary>
        public void UpdatePurchaseButtonState()
        {
            if (purchaseButton == null || currentBuildingItem == null || 
                GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return;
            
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            Resources resources = gameData.resources;
            
            bool canAfford = resources.wood >= currentBuildingItem.woodCost &&
                            resources.stone >= currentBuildingItem.stoneCost &&
                            resources.iron >= currentBuildingItem.ironCost;
            
            // Update button interactability
            purchaseButton.interactable = canAfford;
        }
        
        /// <summary>
        /// Purchase the currently selected building
        /// </summary>
        private void PurchaseBuilding()
        {
            if (shopPanel != null && currentBuildingItem != null)
            {
                shopPanel.OnBuildingPurchased(currentBuildingItem);
                UpdatePurchaseButtonState(); // Update button after purchase
            }
        }
        
        /// <summary>
        /// Close the info panel
        /// </summary>
        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
