using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace KingdomClash
{
    /// <summary>
    /// Handles the Main Menu UI interactions and logic
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject settingsPanel; // Ini akan menjadi panel di scene

        [Header("Settings")]
        [SerializeField] private string newGameSceneName = "GameScene";
        [SerializeField] private string characterSelectionSceneName = "SelectCharachters";
        [SerializeField] private string LoadSceneName = "LoadScene";

        private void Start()
        {
            InitializeButtons();
            CheckForSaveGame();
            ShowMainMenu();
            
            // Pastikan panel lain dinonaktifkan pada start
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(false);
                
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
                
            // Ensure SettingsManager exists
            EnsureSettingsManagerExists();
        }
        
        /// <summary>
        /// Ensure that a SettingsManager instance exists in the scene
        /// </summary>
        private void EnsureSettingsManagerExists()
        {
            if (SettingsManager.Instance == null)
            {
                GameObject settingsManagerObj = new GameObject("SettingsManager");
                settingsManagerObj.AddComponent<SettingsManager>();
            }
        }
        
        /// <summary>
        /// Shows the main menu panel
        /// </summary>
        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Main menu panel reference is missing!");
            }
            
            // Hide other panels
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(false);
                
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        
        /// <summary>
        /// Hides the main menu panel
        /// </summary>
        private void HideMainMenu()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Initialize all button click listeners
        /// </summary>
        private void InitializeButtons()
        {
            // New Game button setup
            if (newGameButton != null)
                newGameButton.onClick.AddListener(StartNewGame);

            // Continue button setup
            if (continueButton != null)
                continueButton.onClick.AddListener(ContinueGame);

            // Load Game button setup
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OpenLoadGamePanel);

            // Settings button setup
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettingsPanel);

            // Exit button setup
            if (exitButton != null)
                exitButton.onClick.AddListener(ExitGame);
        }

        /// <summary>
        /// Check if save data exists to enable/disable the Continue button
        /// </summary>
        private void CheckForSaveGame()
        {
            if (continueButton != null)
            {
                // Check if any save files exist
                bool savesExist = SaveManager.Instance.DoSavesExist();
                continueButton.gameObject.SetActive(savesExist);
            }
        }
        
        /// <summary>
        /// Loads the character selection scene to start a new game
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("Loading character selection scene for new game");
            
            // Ensure GameManager exists and set isContinuing to false
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }
            
            // Ensure SaveManager exists for later save creation
            if (SaveManager.Instance == null)
            {
                GameObject saveManagerObj = new GameObject("SaveManager");
                saveManagerObj.AddComponent<SaveManager>();
            }
            
            // Load character selection scene
            SceneManager.LoadScene(characterSelectionSceneName);
        }

        /// <summary>
        /// Continues the last saved game
        /// </summary>
        public void ContinueGame()
        {
            Debug.Log("Continuing last game");
            
            // Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }
            
            // Load the saved game data from auto-save slot (slot 0)
            GameData savedData = SaveManager.Instance.LoadGameFromSlot(0);
            
            // Pass data to GameManager and set continuing flag
            if (savedData != null && GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame(savedData);
            }
            else
            {
                Debug.LogError("Failed to load saved game data!");
                SceneManager.LoadScene(newGameSceneName);
            }
        }

        /// <summary>
        /// Opens the load game panel to select a save file
        /// </summary>
        public void OpenLoadGamePanel()
        {
            Debug.Log("Opening load game panel");
            SceneManager.LoadScene(LoadSceneName);
            // Implementation will be added when we create the load game panel
        }
        
        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OpenSettingsPanel()
        {
            
            // Hide main menu panel first
            HideMainMenu();
            
            // Make sure SettingsManager exists
            EnsureSettingsManagerExists();
            
            // Show the settings panel (should be a local panel in the scene)
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                
                // Check if it has SettingsPanel component
                SettingsPanel panelComponent = settingsPanel.GetComponent<SettingsPanel>();
                if (panelComponent == null)
                {
                    panelComponent = settingsPanel.AddComponent<SettingsPanel>();
                }
                
                // Panel will sync with SettingsManager via its OnEnable method
            }
            else
            {
                Debug.LogError("Settings panel reference not found! Cannot open settings.");
                // Show main menu again as fallback
                ShowMainMenu();
            }
        }

        /// <summary>
        /// Exits the game
        /// </summary>
        public void ExitGame()
        {
            Debug.Log("Exiting game");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
