using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the pause menu panel that appears when the game is paused
    /// Allows players to resume, save, return to main menu, or quit the game
    /// </summary>
    public class PauseMenuPanel : MonoBehaviour
    {
        [Header("UI Container")]
        [SerializeField] private GameObject buttonContainer;
        [SerializeField] private GameObject settingsPanel;

        [Header("Button References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitGameButton;

        [Header("Optional UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;

        // Input settings
        [Header("Input Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode alternativePauseKey = KeyCode.P;
        
        [Header("External Pause Button")]
        [SerializeField] private Button externalPauseButton;

        // Singleton instance
        private static PauseMenuPanel _instance;
        
        // Keep track of whether the panel is currently visible
        private bool isPanelVisible = false;

        private void Awake()
        {
            // Setup singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }        private void Start()
        {
            // Initialize panel in hidden state
            SetPanelActive(false);
            
            // Initialize button listeners
            InitializeButtons();
            
            // Setup external pause button if assigned
            SetupExternalPauseButton();
        }        private void Update()
        {
            // Detect Escape or alternative key press to toggle pause menu
            if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(alternativePauseKey))
            {
                TogglePauseMenu();
            }
        }
        
        /// <summary>
        /// Setup external pause button if one is assigned in the editor
        /// </summary>
        private void SetupExternalPauseButton()
        {
            if (externalPauseButton != null)
            {
                externalPauseButton.onClick.RemoveAllListeners();
                externalPauseButton.onClick.AddListener(ShowPauseMenu);
            }
        }        /// <summary>
        /// Initialize all button click listeners
        /// </summary>
        private void InitializeButtons()
        {
            // Resume button setup
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(ResumeGame);
            }            // Save Game button setup
            if (saveGameButton != null)
            {
                saveGameButton.onClick.RemoveAllListeners();
                saveGameButton.onClick.AddListener(SaveGame);
            }

            // Settings button setup
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OpenSettingsPanel);
            }

            // Main Menu button setup            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            // Quit Game button setup
            if (quitGameButton != null)
            {
                quitGameButton.onClick.RemoveAllListeners();
                quitGameButton.onClick.AddListener(QuitGame);
            }
        }
          /// <summary>
        /// Toggle the pause menu on/off
        /// </summary>
        public void TogglePauseMenu()
        {
            isPanelVisible = !isPanelVisible;
            
            // Use GameManager to handle pause state if available
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
            else
            {
                // Fallback: Set time scale directly if GameManager is not available
                Time.timeScale = isPanelVisible ? 0f : 1f;
            }
            
            // Always set panel visibility based on isPanelVisible
            SetPanelActive(isPanelVisible);
        }
          /// <summary>
        /// Show or hide the pause menu panel
        /// </summary>
        /// <param name="active">Whether to show the panel</param>
        public void SetPanelActive(bool active)
        {
            // Update current state
            isPanelVisible = active;
            
            // Set panel active state
            gameObject.SetActive(active);
            
            // Ensure button container is also active if panel is active
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(active);
            }
            
            // If panel is activated, position at center of screen
            if (active)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                // Ensure panel is shown in front of other UI elements
                transform.SetAsLastSibling();
            }
        }        /// <summary>
        /// Resume the game by closing the pause menu
        /// </summary>
        public void ResumeGame()
        {
            // If GameManager exists, use it to unpause the game
            if (GameManager.Instance != null)
            {
                // Only unpause if the game is currently paused
                if (Time.timeScale == 0)
                {
                    GameManager.Instance.TogglePause();
                }
            }
            else
            {
                // Fallback: Set time scale to 1
                Time.timeScale = 1;
            }
            
            SetPanelActive(false);
        }        /// <summary>
        /// Save the current game state
        /// </summary>
        public void SaveGame()
        {
            // If GameManager exists, use it to save the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
        }        /// <summary>
        /// Return to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            // If GameManager exists, use it to return to the main menu
            if (GameManager.Instance != null)
            {
                // Resume normal time scale before scene transition
                Time.timeScale = 1;
                
                GameManager.Instance.ReturnToMainMenu();
            }
        }        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            // If GameManager exists, use it to quit the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }
        
        /// <summary>
        /// Show the pause menu and pause the game
        /// </summary>
        public void ShowPauseMenu()
        {
            isPanelVisible = true;
            
            // If game is not paused yet, pause it via GameManager or directly
            if (Time.timeScale != 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TogglePause();
                }
                else
                {
                    Time.timeScale = 0f;
                }
            }
            
            // Always show the panel
            SetPanelActive(true);
        }        /// <summary>
        /// Get the current singleton instance
        /// </summary>
        public static PauseMenuPanel GetInstance()
        {
            return _instance;
        }
        
        /// <summary>
        /// Static method to show pause menu from other scripts
        /// </summary>
        public static void ShowPauseMenuStatic()
        {
            if (_instance != null)
            {
                _instance.ShowPauseMenu();
            }
        }
          /// <summary>
        /// Add listener to external pause button
        /// </summary>
        /// <param name="button">Button to use as external pause activator</param>
        public void SetExternalPauseButton(Button button)
        {
            if (button != null)
            {
                externalPauseButton = button;
                SetupExternalPauseButton();
            }
        }        /// <summary>
        /// Get the current pause menu state
        /// </summary>
        /// <returns>True if the panel is visible, false otherwise</returns>
        public bool IsPanelVisible()
        {
            return isPanelVisible;
        }
        
        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OpenSettingsPanel()
        {
            // Hide pause menu buttons when opening settings
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(false);
            }
            
            // Make sure SettingsManager exists
            if (SettingsManager.Instance == null)
            {
                GameObject settingsManagerObj = new GameObject("SettingsManager");
                settingsManagerObj.AddComponent<SettingsManager>();
            }
            
            // Show the settings panel
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                
                // Check if it has SettingsPanel component
                SettingsPanel panelComponent = settingsPanel.GetComponent<SettingsPanel>();
                if (panelComponent == null)
                {
                    panelComponent = settingsPanel.AddComponent<SettingsPanel>();
                }
                
                // Ensure settings panel is in front
                settingsPanel.transform.SetAsLastSibling();
                
                // Get the close button from the settings panel
                Button closeButton = panelComponent.CloseButton;
                if (closeButton != null)
                {
                    closeButton.onClick.RemoveAllListeners();
                    closeButton.onClick.AddListener(CloseSettingsPanel);
                }
            }
        }
        
        /// <summary>
        /// Closes the settings panel and shows the pause menu again
        /// </summary>
        public void CloseSettingsPanel()
        {
            // Hide settings panel
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            // Show pause menu buttons again
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(true);
            }
        }
    }
}
