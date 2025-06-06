using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the main HUD (Heads-Up Display) panel in the game scene
    /// </summary>
    public class HUDPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private PlayerInfoPanel playerInfoPanel;
        [SerializeField] private ResourcePanel resourcePanel;
        [SerializeField] private GameObject shopBuildingPanel;
        [SerializeField] private Button toggleShopButton;
        
        // You can add more panel references here as your HUD grows

        private void Awake()
        {
            // Setup toggle shop button if assigned
            if (toggleShopButton != null)
            {
                toggleShopButton.onClick.AddListener(ToggleShopBuildingPanel);
            }
        }

        private void Start()
        {
            // Initialize the HUD
            InitializeHUD();
            
            // Make sure the shop panel is closed at start
            if (shopBuildingPanel != null)
            {
                shopBuildingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Initialize the HUD with current game data
        /// </summary>
        private void InitializeHUD()
        {
            // Check if GameManager exists and has game data
            if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameData() != null)
            {
                GameData gameData = GameManager.Instance.GetCurrentGameData();
                UpdateHUD(gameData);
            }
            else
            {
                Debug.LogWarning("HUDPanel: GameManager or GameData not available. Using default values.");
                SetDefaultValues();
            }        
        }
        
        /// <summary>
        /// Update all HUD elements with data from GameData
        /// </summary>
        /// <param name="gameData">The current game data</param>
        public void UpdateHUD(GameData gameData)
        {
            // Update player info panel
            if (playerInfoPanel != null)
            {
                playerInfoPanel.UpdatePlayerInfoFromGameData(gameData);
            }
            
            // Update resource panel
            if (resourcePanel != null && gameData.resources != null)
            {
                resourcePanel.UpdateResourceDisplay(gameData.resources);            
            }
        }
        
        /// <summary>
        /// Set default values for HUD elements when game data is not available
        /// </summary>
        private void SetDefaultValues()
        {
            // Set default values for player info panel
            if (playerInfoPanel != null)
            {
                playerInfoPanel.SetDefaultValues();
            }
            
            // Set default values for resource panel
            if (resourcePanel != null)
            {
                resourcePanel.SetDefaultValues();            
            }
        }
        
        /// <summary>
        /// Public method to update the HUD from other scripts
        /// </summary>
        public void RefreshHUD()
        {
            InitializeHUD();
        }
        
        /// <summary>
        /// Opens the shop building panel
        /// </summary>
        public void OpenShopBuildingPanel()
        {
            if (shopBuildingPanel != null)
            {
                shopBuildingPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Closes the shop building panel
        /// </summary>
        public void CloseShopBuildingPanel()
        {
            if (shopBuildingPanel != null)
            {
                shopBuildingPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Toggles the shop building panel visibility
        /// </summary>
        public void ToggleShopBuildingPanel()
        {
            if (shopBuildingPanel != null)
            {
                shopBuildingPanel.SetActive(!shopBuildingPanel.activeSelf);
            }
        }
            
        /// <summary>
        /// Get reference to the player info panel
        /// </summary>
        public PlayerInfoPanel GetPlayerInfoPanel()
        {
            return playerInfoPanel;
        }
        
        /// <summary>
        /// Get reference to the resource panel
        /// </summary>
        public ResourcePanel GetResourcePanel()
        {
            return resourcePanel;
        }
    }
}
