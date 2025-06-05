using UnityEngine;
using TMPro;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the player information panel within the HUD
    /// </summary>
    public class PlayerInfoPanel : MonoBehaviour
    {
        [Header("UI Text Components")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerLevelText;

        [Header("Optional UI Elements")]
        [SerializeField] private GameObject characterIconHolder;

        /// <summary>
        /// Update the player information display
        /// </summary>
        /// <param name="playerName">The player's name</param>
        /// <param name="playerLevel">The player's level</param>
        public void UpdatePlayerInfo(string playerName, int playerLevel)
        {
            if (playerNameText != null)
            {
                playerNameText.text = playerName;
            }

            if (playerLevelText != null)
            {
                playerLevelText.text = $"Level {playerLevel}";
            }
        }

        /// <summary>
        /// Update the player information display using GameData
        /// </summary>
        /// <param name="gameData">The current game data</param>
        public void UpdatePlayerInfoFromGameData(GameData gameData)
        {
            if (gameData == null) return;
            
            UpdatePlayerInfo(gameData.playerName, gameData.level);
        }

        /// <summary>
        /// Set default values for the player information
        /// </summary>
        public void SetDefaultValues()
        {
            UpdatePlayerInfo("Player", 1);
        }
    }
}
