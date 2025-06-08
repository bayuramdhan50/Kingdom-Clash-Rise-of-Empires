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

        // Static property untuk menyimpan data game antar scene
        private static GameData preCapturedData = null;

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
            
            // Subscribe to the scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from the scene loaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if this is the game scene
            if (scene.name == gameSceneName)
            {
                HandleGameSceneLoaded();
            }
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
            
            // For new games, camera data will be set when first saved
            // Don't initialize with default values here, let the camera controller handle it
            
            // Reset auto-save tracking
            lastResourceState = new Resources
            {
                wood = currentGameData.resources.wood,
                stone = currentGameData.resources.stone,
                iron = currentGameData.resources.iron,
                food = currentGameData.resources.food
            };
            lastAutoSaveTime = Time.time;
            
            // Initialize empty lists
            currentGameData.placedBuildings = new System.Collections.Generic.List<BuildingData>();
            currentGameData.trainingProcesses = new System.Collections.Generic.List<TrainingData>();
            currentGameData.units = new System.Collections.Generic.List<UnitData>();
            
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
            
            // Initialize building list if it's null (backwards compatibility with older saves)
            if (currentGameData.placedBuildings == null)
            {
                currentGameData.placedBuildings = new System.Collections.Generic.List<BuildingData>();
            }
            
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
                // Dapatkan posisi kamera terkini
                RTSCameraController cameraController = FindObjectOfType<RTSCameraController>();
                if (cameraController != null)
                {
                    // Simpan posisi kamera saat ini langsung ke GameData
                    Vector3 currentPos = cameraController.transform.position;
                    Quaternion currentRot = cameraController.transform.rotation;
                    float currentZoom = cameraController.cam != null ? cameraController.cam.orthographicSize : 30f;
                    
                    // Simpan nilai-nilai ini langsung ke GameData
                    currentGameData.cameraPosition = new Vector3Data(currentPos);
                    currentGameData.cameraRotation = new QuaternionData(currentRot);
                    currentGameData.cameraZoom = currentZoom;
                }
                
                // Update data lainnya dengan state game saat ini
                UpdateGameData();
                
                // Gunakan SaveManager untuk menyimpan data ke slot auto-save (0)
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
            // Update the game data before going to save screen
            if (currentGameData != null)
            {
                // Simpan posisi kamera saat ini langsung ke gameData sebelum beralih ke SaveScene
                RTSCameraController cameraController = FindObjectOfType<RTSCameraController>();
                if (cameraController != null)
                {
                    Vector3 currentPos = cameraController.transform.position;
                    Quaternion currentRot = cameraController.transform.rotation;
                    float currentZoom = cameraController.cam != null ? cameraController.cam.orthographicSize : 30f;
                    
                    // Simpan nilai kamera langsung ke GameData
                    currentGameData.cameraPosition = new Vector3Data(currentPos);
                    currentGameData.cameraRotation = new QuaternionData(currentRot);
                    currentGameData.cameraZoom = currentZoom;
                }
                
                // Update semua data permainan lainnya
                UpdateGameData();
            }
            
            // The actual saving will be done in SaveSceneUI
        }

        /// <summary>
        /// Update the game data with current game state
        /// </summary>
        private void UpdateGameData()
        {
            // Save placed buildings data if building manager exists
            if (BuildingManager.Instance != null)
            {
                // Log untuk membantu debugging
                Debug.Log($"Updating GameData. BuildingManager has {BuildingManager.Instance.GetPlacedBuildingCount()} buildings registered.");
                BuildingManager.Instance.SavePlacedBuildings(currentGameData);
            }
            else
            {
                Debug.LogError("BuildingManager.Instance is null during UpdateGameData!");
                
                // Coba cari dan buat BuildingManager jika tidak ada
                if (FindObjectOfType<BuildingManager>() == null)
                {
                    Debug.Log("Creating BuildingManager during UpdateGameData...");
                    GameObject buildingManagerObj = new GameObject("BuildingManager");
                    BuildingManager newManager = buildingManagerObj.AddComponent<BuildingManager>();
                    
                    // Coba scan semua bangunan yang ada di scene
                    Building[] allBuildingsInScene = FindObjectsOfType<Building>();
                    foreach (Building building in allBuildingsInScene)
                    {
                        newManager.RegisterPlacedBuilding(building);
                    }
                    
                    // Save buildings dengan manager yang baru dibuat
                    newManager.SavePlacedBuildings(currentGameData);
                }
            }
            
            // Save training state if TrainingManager exists
            if (UI.TrainingManager.Instance != null)
            {
                Debug.Log("Saving training state to GameData.");
                UI.TrainingManager.Instance.SaveTrainingState();
            }
            
            // Save camera state if camera controller exists
            RTSCameraController cameraController = FindObjectOfType<RTSCameraController>();
            if (cameraController != null)
            {
                // Ambil posisi kamera terkini sebelum menyimpan
                Vector3 currentCameraPos = cameraController.transform.position;
                Quaternion currentCameraRot = cameraController.transform.rotation;
                float currentCameraZoom = cameraController.cam != null ? cameraController.cam.orthographicSize : 30f;
                
                // Validasi posisi kamera sebelum save
                if (currentCameraPos == Vector3.zero)
                {
                    currentCameraPos = new Vector3(20f, 20f, 20f);
                    cameraController.transform.position = currentCameraPos;
                }
                
                // Validasi zoom
                if (currentCameraZoom <= 0)
                {
                    currentCameraZoom = 30f;
                    if (cameraController.cam != null)
                        cameraController.cam.orthographicSize = currentCameraZoom;
                }
                
                // Simpan state kamera
                cameraController.SaveCameraState();
                
                // Langsung set nilai di GameData untuk memastikan nilai terkini tersimpan
                if (currentGameData != null)
                {
                    currentGameData.cameraPosition = new Vector3Data(currentCameraPos);
                    currentGameData.cameraRotation = new QuaternionData(currentCameraRot);
                    currentGameData.cameraZoom = currentCameraZoom;
                }
            }
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

        /// <summary>
        /// Menangani inisialisasi ketika game scene dimuat
        /// Dipanggil sebagai event dari OnSceneLoaded
        /// </summary>
        private void HandleGameSceneLoaded()
        {
            // Jika kita melakukan reload dari save, muat bangunan yang sudah dibangun
            if (isContinuing && currentGameData != null)
            {
                // Muat bangunan
                if (currentGameData.placedBuildings != null && currentGameData.placedBuildings.Count > 0)
                {
                    // Tunggu sedikit untuk memastikan BuildingManager sudah ter-initialize
                    StartCoroutine(LoadBuildingsWithDelay());
                }
                
                // Muat data training jika ada
                if (currentGameData.trainingProcesses != null && currentGameData.trainingProcesses.Count > 0)
                {
                    // Tunggu sebentar untuk memastikan TrainingManager sudah ter-initialize
                    StartCoroutine(LoadTrainingWithDelay());
                }

                // Muat data unit jika ada
                if (currentGameData.units != null && currentGameData.units.Count > 0)
                {
                    // Tunggu sebentar untuk memastikan CharacterManager sudah ter-inisialisasi
                    StartCoroutine(LoadUnitsWithDelay());
                }
            }
        }
        
        /// <summary>
        /// Coroutine untuk memuat data training dengan sedikit delay
        /// untuk memastikan TrainingManager sudah ter-inisialisasi
        /// </summary>
        private System.Collections.IEnumerator LoadTrainingWithDelay()
        {
            // Tunggu satu frame untuk memastikan semua manager sudah ter-inisialisasi
            yield return null;
            
            // Pastikan TrainingManager ada
            if (UI.TrainingManager.Instance == null)
            {
                // Create TrainingManager if not found
                GameObject trainingManagerObj = new GameObject("TrainingManager");
                trainingManagerObj.AddComponent<UI.TrainingManager>();
            }
            
            // Load training data dari save data
            if (UI.TrainingManager.Instance != null)
            {
                UI.TrainingManager.Instance.LoadTrainingState();
            }
        }

        /// <summary>
        /// Coroutine untuk memuat bangunan dengan sedikit delay
        /// untuk memastikan BuildingManager sudah ter-inisialisasi
        /// </summary>
        private System.Collections.IEnumerator LoadBuildingsWithDelay()
        {
            // Tunggu satu frame untuk memastikan semua manager sudah ter-inisialisasi
            yield return null;
            
            // Pastikan BuildingManager ada
            if (BuildingManager.Instance == null)
            {
                Debug.LogWarning("BuildingManager not found, creating one...");
                GameObject buildingManagerObj = new GameObject("BuildingManager");
                buildingManagerObj.AddComponent<BuildingManager>();
            }
            
            // Load bangunan dari save data
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.LoadPlacedBuildings(currentGameData);
                Debug.Log($"Loaded {currentGameData.placedBuildings.Count} buildings successfully");
            }
            else
            {
                Debug.LogError("Failed to create or find BuildingManager!");
            }
        }

        /// <summary>
        /// Coroutine untuk memuat unit dengan sedikit delay
        /// untuk memastikan CharacterManager sudah ter-inisialisasi
        /// </summary>
        private System.Collections.IEnumerator LoadUnitsWithDelay()
        {
            // Tunggu satu frame untuk memastikan semua manager sudah ter-inisialisasi
            yield return null;
            yield return null;  // Extra frame for other systems
            
            // Pastikan kita memiliki data unit
            if (currentGameData.units == null || currentGameData.units.Count == 0)
            {
                yield break;
            }
            
            // Loading units from saved data
            
            // Reference ke TrainingManager untuk mengakses unit prefabs
            UI.TrainingManager trainingManager = UI.TrainingManager.Instance;
            
            if (trainingManager == null)
            {
                // Create TrainingManager if not found
                GameObject trainingManagerObj = new GameObject("TrainingManager");
                trainingManager = trainingManagerObj.AddComponent<UI.TrainingManager>();
                // Wait one more frame for TrainingManager to initialize
                yield return null; 
            }
            
            // Buat callback untuk memuat prefab unit
            // Panel ini akan berisi komponen fungsi untuk memuat dan me-spawn unit
            GameObject unitLoaderPanel = new GameObject("UnitLoaderPanel");
            UnitLoaderPanel loaderPanel = unitLoaderPanel.AddComponent<UnitLoaderPanel>();
            
            // Mulai proses loading dan spawning unit
            int loadedCount = loaderPanel.LoadSavedUnits(currentGameData.units);
        }

        /// <summary>
        /// Set pre-captured game data untuk digunakan di SaveScene
        /// </summary>
        /// <param name="data">Data game yang akan disimpan</param>
        public void SetPreCapturedGameData(GameData data)
        {
            preCapturedData = data;
        }
        
        /// <summary>
        /// Get pre-captured game data untuk digunakan di SaveScene
        /// </summary>
        public GameData GetPreCapturedGameData()
        {
            return preCapturedData;
        }
    }
}

