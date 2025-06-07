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
        }        /// <summary>
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
                // Check if the building prefab exists
                if (buildingItem.buildingPrefab == null)
                {
                    Debug.LogError($"Building prefab untuk {buildingItem.buildingName} tidak tersedia!");
                    return;
                }
                
                // Start building placement mode
                if (BuildingPlacementSystem.Instance != null)
                {
                    // Kurangi sumber daya
                    GameManager.Instance.UpdateResource("wood", -buildingItem.woodCost);
                    GameManager.Instance.UpdateResource("stone", -buildingItem.stoneCost);
                    GameManager.Instance.UpdateResource("iron", -buildingItem.ironCost);
                    
                    // Mulai proses penempatan
                    BuildingPlacementSystem.Instance.StartPlacement(buildingItem.buildingPrefab);
                    
                    // Refresh HUD untuk menampilkan sumber daya yang diperbarui
                    if (hudPanel != null)
                    {
                        hudPanel.RefreshHUD();
                    }
                    
                    // Update UI berdasarkan jumlah sumber daya baru
                    UpdateBuildingAvailability();
                    
                    // Tutup panel toko saat penempatan
                    ClosePanel();
                }
                else
                {
                    Debug.LogError("BuildingPlacementSystem tidak ditemukan di scene!");
                }
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
