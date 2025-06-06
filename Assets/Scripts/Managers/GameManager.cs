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

        [Header("Auto Save Settings")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 menit dalam detik
        [SerializeField] private bool autoSaveOnLevelUp = true;
        [SerializeField] private bool autoSaveOnResourceThreshold = true;
        [SerializeField] private int resourceChangeThreshold = 200; // Auto-save jika perubahan sumber daya melebihi nilai ini

        // Current game state
        private GameData currentGameData;
        
        // Game state flag
        private bool isGamePaused = false;
        
        // Flag to indicate if we are continuing a saved game or starting a new game
        private bool isContinuing = false;

        // Auto-save tracking
        private float lastAutoSaveTime = 0f;
        private Resources lastResourceState;

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
            
            // Initialize resource tracking for auto-save
            if (currentGameData != null && currentGameData.resources != null)
            {
                lastResourceState = new Resources
                {
                    wood = currentGameData.resources.wood,
                    stone = currentGameData.resources.stone,
                    iron = currentGameData.resources.iron,
                    food = currentGameData.resources.food
                };
            }
            else
            {
                lastResourceState = new Resources();
            }
        }

        private void Update()
        {
            // Skip auto-save logic if game is paused or auto-save is disabled
            if (isGamePaused || !enableAutoSave || currentGameData == null) 
                return;

            // Only run auto-save logic in the game scene
            if (SceneManager.GetActiveScene().name != gameSceneName)
                return;
                
            // Check if it's time for a timed auto-save
            if (Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                Debug.Log("Melakukan auto-save berdasarkan waktu");
                AutoSaveGame();
                lastAutoSaveTime = Time.time;
            }

            // Check for resource-based auto-save if enabled
            if (autoSaveOnResourceThreshold && currentGameData.resources != null)
            {
                CheckResourceChangeForAutoSave();
            }
        }
        
        /// <summary>
        /// Checks if resources have changed enough to trigger an auto-save
        /// </summary>
        private void CheckResourceChangeForAutoSave()
        {
            if (lastResourceState == null || currentGameData.resources == null)
                return;
                
            // Calculate total resource difference
            int woodDiff = Mathf.Abs(currentGameData.resources.wood - lastResourceState.wood);
            int stoneDiff = Mathf.Abs(currentGameData.resources.stone - lastResourceState.stone);
            int ironDiff = Mathf.Abs(currentGameData.resources.iron - lastResourceState.iron);
            int foodDiff = Mathf.Abs(currentGameData.resources.food - lastResourceState.food);
            
            int totalDiff = woodDiff + stoneDiff + ironDiff + foodDiff;
            
            // Auto-save if difference exceeds threshold
            if (totalDiff >= resourceChangeThreshold)
            {
                Debug.Log($"Melakukan auto-save karena perubahan sumber daya ({totalDiff})");
                AutoSaveGame();
                
                // Update last resource state
                lastResourceState.wood = currentGameData.resources.wood;
                lastResourceState.stone = currentGameData.resources.stone;
                lastResourceState.iron = currentGameData.resources.iron;
                lastResourceState.food = currentGameData.resources.food;
            }
        }

        /// <summary>
        /// Called when player levels up or completes an objective
        /// </summary>
        /// <param name="newLevel">The new level</param>
        public void OnLevelUp(int newLevel)
        {
            // Update the current game data
            if (currentGameData != null)
            {
                currentGameData.level = newLevel;
                
                // Auto-save when leveling up if enabled
                if (autoSaveOnLevelUp)
                {
                    Debug.Log($"Melakukan auto-save karena naik level ke {newLevel}");
                    AutoSaveGame();
                }
            }
        }

        /// <summary>
        /// Start a new game, resetting all data
        /// </summary>
        public void StartNewGame()
        {
            // Reset all data by creating a new GameData
            currentGameData = new GameData();
            
            // If character was selected, use it, otherwise use default
            if (Characters.CharacterManager.Instance != null && 
                Characters.CharacterManager.Instance.GetSelectedCharacter() != null)
            {
                currentGameData.selectedCharacter = Characters.CharacterManager.Instance.GetSelectedCharacter().CharacterType;
            }
            else
            {
                currentGameData.selectedCharacter = Characters.CharacterType.Arvandir;
            }
            
            // Set default resources
            currentGameData.resources = new Resources { wood = 500, stone = 300, iron = 200, food = 600 };
            currentGameData.playerName = "Player";
            currentGameData.level = 1;
            
            // Reset auto-save tracking
            lastResourceState = new Resources
            {
                wood = currentGameData.resources.wood,
                stone = currentGameData.resources.stone,
                iron = currentGameData.resources.iron,
                food = currentGameData.resources.food
            };
            lastAutoSaveTime = Time.time;
            
            // Set the flag to indicate this is a new game, not a continuation
            isContinuing = false;
            
            // Load the game scene without auto-saving
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Load game data and continue from saved point
        /// </summary>
        /// <param name="gameData">The game data to load</param>
        public void LoadGame(GameData gameData)
        {
            currentGameData = gameData;
            
            // Initialize resource tracking
            if (currentGameData.resources != null)
            {
                lastResourceState = new Resources
                {
                    wood = currentGameData.resources.wood,
                    stone = currentGameData.resources.stone,
                    iron = currentGameData.resources.iron,
                    food = currentGameData.resources.food
                };
            }
            
            // Reset auto-save timer
            lastAutoSaveTime = Time.time;
            
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
        /// Auto-save the current game to slot 0
        /// </summary>
        public void AutoSaveGame()
        {
            if (currentGameData != null)
            {
                // Update data with current game state before saving
                UpdateGameData();
                
                // Use SaveManager to save the data to the auto-save slot (0)
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveCurrentGame(currentGameData, 0);
                }
            }
        }
        
        /// <summary>
        /// Update a specific resource and check for auto-save
        /// </summary>
        /// <param name="resourceType">Type of resource to update</param>
        /// <param name="amount">Amount to add (positive) or subtract (negative)</param>
        public void UpdateResource(string resourceType, int amount)
        {
            if (currentGameData == null || currentGameData.resources == null)
                return;
                
            // Update the appropriate resource
            switch (resourceType.ToLower())
            {
                case "wood":
                    currentGameData.resources.wood += amount;
                    break;
                case "stone":
                    currentGameData.resources.stone += amount;
                    break;
                case "iron":
                    currentGameData.resources.iron += amount;
                    break;
                case "food":
                    currentGameData.resources.food += amount;
                    break;
            }
            
            // Check for resource-based auto-save
            if (autoSaveOnResourceThreshold)
            {
                CheckResourceChangeForAutoSave();
            }
        }
        
        /// <summary>
        /// Save game prompt - this redirects to the save scene instead of saving directly
        /// </summary>
        public void SaveGame()
        {
            // Just update the game data before going to save screen
            if (currentGameData != null)
            {
                UpdateGameData();
            }
            
            // The actual saving will be done in SaveSceneUI
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
            // No automatic save before returning to main menu
            // Player must manually save their game
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
            // Auto-save the game before quitting if enabled
            if (enableAutoSave && currentGameData != null)
            {
                Debug.Log("Melakukan auto-save sebelum keluar dari game");
                AutoSaveGame();
            }

            Debug.Log("Quitting game");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

