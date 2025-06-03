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
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            currentGameData = new GameData();
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Load game data
        /// </summary>
        /// <param name="gameData">The game data to load</param>
        public void LoadGame(GameData gameData)
        {
            currentGameData = gameData;
            SceneManager.LoadScene(gameSceneName);
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
