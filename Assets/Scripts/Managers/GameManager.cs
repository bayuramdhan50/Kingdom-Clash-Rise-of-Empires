using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingdomClash
{
    /// <summary>
    /// Core game manager that oversees game state and other managers
    /// Implemented as a singleton to be accessible from any script
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameScene";

        // Current game state
        private GameData currentGameData;
        
        // Game state flag
        private bool isGamePaused = false;
        
        // Flag to indicate if we are continuing a saved game or starting a new game
        private bool isContinuing = false;

        public bool IsContinuing => isContinuing;

        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Initialize other systems if needed
        }        /// <summary>
        /// Start a new game, resetting all data
        /// </summary>
        public void StartNewGame()
        {
            // Reset all data by creating a new GameData
            currentGameData = new GameData();
            // Set the flag to indicate this is a new game, not a continuation
            isContinuing = false;
            
            // If SaveManager exists, create a new save
            if (SaveManager.Instance != null && currentGameData != null)
            {
                // If character was selected, use it, otherwise use default
                Characters.CharacterType selectedType = Characters.CharacterManager.Instance != null && 
                    Characters.CharacterManager.Instance.GetSelectedCharacter() != null ?
                    Characters.CharacterManager.Instance.GetSelectedCharacter().CharacterType :
                    Characters.CharacterType.Arvandir;
                    
                SaveManager.Instance.CreateNewSave(selectedType);
            }
            
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Load game data and continue from saved point
        /// </summary>
        /// <param name="gameData">The game data to load</param>
        public void LoadGame(GameData gameData)
        {
            currentGameData = gameData;
            // Set the flag to indicate we are continuing a saved game
            isContinuing = true;
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Get the current game data
        /// </summary>
        /// <returns>The current game data or null if not available</returns>
        public GameData GetCurrentGameData()
        {
            return currentGameData;
        }
        
        /// <summary>
        /// Check if we are continuing a saved game
        /// </summary>
        /// <returns>True if continuing a saved game, false if starting a new game</returns>
        public bool GetIsContinuing()
        {
            return isContinuing;
        }

        /// <summary>
        /// Save the current game
        /// </summary>
        public void SaveGame()
        {
            if (currentGameData != null)
            {
                // Update data with current game state before saving
                UpdateGameData();
                
                // Use SaveManager to save the data
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveCurrentGame(currentGameData);
                }
            }
        }

        /// <summary>
        /// Update the game data with current game state
        /// </summary>
        private void UpdateGameData()
        {
            // This is where you would update the currentGameData with the latest game state
            // For example, update resources, player progress, etc.
            Debug.Log("Updating game data before save");
        }

        /// <summary>
        /// Return to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Save the game before returning to the main menu
            SaveGame();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Toggle the pause state of the game
        /// </summary>
        public void TogglePause()
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0 : 1;
            Debug.Log("Game paused: " + isGamePaused);
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            // Save the game before quitting
            SaveGame();
            
            Debug.Log("Quitting game");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
