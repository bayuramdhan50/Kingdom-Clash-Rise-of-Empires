using UnityEngine;
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
        
        // You can add more panel references here as your HUD grows

        private void Start()
        {
            // Initialize the HUD
            InitializeHUD();
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
            }        }
        
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
                resourcePanel.UpdateResourceDisplay(gameData.resources);            }
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
                resourcePanel.SetDefaultValues();            }
        }
        
        /// <summary>
        /// Public method to update the HUD from other scripts
        /// </summary>
        public void RefreshHUD()
        {
            InitializeHUD();
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
