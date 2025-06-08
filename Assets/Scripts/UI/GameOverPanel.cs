using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Tambahkan namespace untuk TextMeshPro

namespace KingdomClash.UI
{
    /// <summary>
    /// Handles the game over UI display
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {        [Header("UI References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button restartButton;
        
        [Header("Messages")]
        [SerializeField] private string victoryTitle = "VICTORY!";
        [SerializeField] private string defeatTitle = "DEFEAT!";
        [SerializeField] private string victoryMessage = "You have conquered the enemy kingdom!";
        [SerializeField] private string defeatMessage = "Your kingdom has fallen!";
        
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameScene";
        
        private void Start()
        {
            // Hide panel at start
            gameObject.SetActive(false);
            
            // Set up button listeners
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }
        
        /// <summary>
        /// Shows the game over panel with the appropriate message
        /// </summary>
        /// <param name="isVictory">True if player won, false if player lost</param>
        public void ShowGameOver(bool isVictory)
        {
            // Display panel
            gameObject.SetActive(true);
            
            // Set title text
            if (titleText != null)
            {
                titleText.text = isVictory ? victoryTitle : defeatTitle;
                titleText.color = isVictory ? Color.green : Color.red;
            }
            
            // Set message text
            if (messageText != null)
            {
                messageText.text = isVictory ? victoryMessage : defeatMessage;
            }
            
            // Pause game
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// Return to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Resume normal time scale
            Time.timeScale = 1f;
            
            // Load main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        /// <summary>
        /// Restart the current game
        /// </summary>
        public void RestartGame()
        {
            // Resume normal time scale
            Time.timeScale = 1f;
            
            // Reload the game scene
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
