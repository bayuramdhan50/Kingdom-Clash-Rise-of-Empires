using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

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
        [SerializeField] private GameObject settingsPanel;

        [Header("Settings")]
        [SerializeField] private string newGameSceneName = "GameScene";
        [SerializeField] private string characterSelectionSceneName = "SelectCharachters";
        private void Start()
        {
            InitializeButtons();
            CheckForSaveGame();
            ShowMainMenu();
            
            // Hide other panels
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(false);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
          /// <summary>
        /// Shows the main menu panel
        /// </summary>
        public void ShowMainMenu()
        {
            Debug.Log("Showing main menu panel");
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Main menu panel reference is missing!");
            }
            
            // Ensure other panels are hidden
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
                
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(false);
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
                // Check if save file exists
                bool saveExists = SaveManager.Instance.DoesSaveExist();
                continueButton.gameObject.SetActive(saveExists);
            }
        }        /// <summary>
        /// Loads the character selection scene to start a new game
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("Loading character selection scene");
            SceneManager.LoadScene(characterSelectionSceneName);
        }
        
        /// <summary>
        /// Shows the character selection panel (deprecated - keeping for reference)
        /// </summary>
        private void ShowCharacterSelection()
        {
            if (characterSelectionPanel != null)
            {
                characterSelectionPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Continues the last saved game
        /// </summary>
        public void ContinueGame()
        {
            Debug.Log("Continuing last game");
            SaveManager.Instance.LoadLastSave();
            SceneManager.LoadScene(newGameSceneName);
        }

        /// <summary>
        /// Opens the load game panel to select a save file
        /// </summary>
        public void OpenLoadGamePanel()
        {
            Debug.Log("Opening load game panel");
            // Implementation will be added when we create the load game panel
        }        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OpenSettingsPanel()
        {
            Debug.Log("Opening settings panel");
            if (settingsPanel != null)
            {
                HideMainMenu();
                settingsPanel.SetActive(true);
            }
        }
          /// <summary>
        /// Closes the settings panel and shows the main menu
        /// </summary>
        public void CloseSettingsPanel()
        {
            Debug.Log("Closing settings panel and returning to main menu");
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            // Explicitly show the main menu panel
            ShowMainMenu();
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
