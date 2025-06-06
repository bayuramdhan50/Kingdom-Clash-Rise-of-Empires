using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the shop building panel where players can purchase buildings
    /// </summary>
    public class ShopBuildingPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button closeButton;
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        
        [Header("References")]
        [SerializeField] private HUDPanel hudPanel;
        
        [Header("Building Items")]
        [SerializeField] private List<BuildingItemUI> buildingItems = new List<BuildingItemUI>();
        
        private void Awake()
        {
            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
            else
            {
                Debug.LogWarning("ShopBuildingPanel: Close button is not assigned");
            }
            
            // If HUDPanel is not assigned, try to find it
            if (hudPanel == null)
            {
                hudPanel = FindObjectOfType<HUDPanel>();
            }
            
            // Setup the BuildingInfoPanel reference
            if (buildingInfoPanel != null)
            {
                buildingInfoPanel.SetShopPanel(this);
            }
        }
        
        private void Start()
        {
            // Initialize all building items
            InitializeShop();
        }
        
        /// <summary>
        /// Initialize each building item in the shop
        /// </summary>
        private void InitializeShop()
        {
            // Initialize each building item
            foreach (BuildingItemUI buildingItem in buildingItems)
            {
                if (buildingItem != null)
                {
                    buildingItem.Initialize(OnBuildingPurchased, OnBuildingSelected);
                }
            }
            
            // Update UI based on current resources
            UpdateBuildingAvailability();
        }
          /// <summary>
        /// Updates all building items availability based on current resources
        /// </summary>
        public void UpdateBuildingAvailability()
        {
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return;
                
            foreach (BuildingItemUI buildingItem in buildingItems)
            {
                if (buildingItem != null)
                {
                    buildingItem.UpdateAvailability();
                }
            }
            
            // Also update info panel if it's active
            if (buildingInfoPanel != null && buildingInfoPanel.gameObject.activeSelf)
            {
                buildingInfoPanel.UpdatePurchaseButtonState();
            }
        }
        
        /// <summary>
        /// Called when a building item is selected to show details
        /// </summary>
        /// <param name="buildingItem">The selected building item</param>
        public void OnBuildingSelected(BuildingItemUI buildingItem)
        {
            if (buildingInfoPanel != null)
            {
                buildingInfoPanel.ShowBuildingInfo(buildingItem);
            }
        }
        
        /// <summary>
        /// Called when a building is purchased
        /// </summary>
        /// <param name="buildingItem">The building item that was purchased</param>
        public void OnBuildingPurchased(BuildingItemUI buildingItem)
        {
            // Check if player has enough resources
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return;
                
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            Resources resources = gameData.resources;
            
            // Check resources
            if (resources.wood >= buildingItem.woodCost &&
                resources.stone >= buildingItem.stoneCost &&
                resources.iron >= buildingItem.ironCost)
            {
                // Deduct resources
                GameManager.Instance.UpdateResource("wood", -buildingItem.woodCost);
                GameManager.Instance.UpdateResource("stone", -buildingItem.stoneCost);
                GameManager.Instance.UpdateResource("iron", -buildingItem.ironCost);
                
                // Here you would implement the actual building creation
                Debug.Log($"Building purchased: {buildingItem.buildingName}");
                
                // Refresh HUD to show updated resources
                if (hudPanel != null)
                {
                    hudPanel.RefreshHUD();
                }
                
                // Update UI based on new resource amounts
                UpdateBuildingAvailability();
            }
        }
        
        /// <summary>
        /// Closes the shop building panel
        /// </summary>
        public void ClosePanel()
        {
            if (hudPanel != null)
            {
                hudPanel.CloseShopBuildingPanel();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
