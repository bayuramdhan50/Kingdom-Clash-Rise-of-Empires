using System.Collections.Generic;
using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Manages all buildings in the game, including saving and loading building data
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        // Singleton instance
        public static BuildingManager Instance { get; private set; }

        [Header("Building Prefabs")]
        [SerializeField] private List<GameObject> buildingPrefabs = new List<GameObject>();
        
        // Dictionary to map prefab names to actual prefabs for quick lookup
        private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
        
        // List of all placed buildings in the current game
        private List<Building> placedBuildings = new List<Building>();        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple BuildingManager instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject); // Pastikan tidak hancur saat pindah scene
            
            // Initialize prefab dictionary
            InitializePrefabDictionary();
            
            // Secara otomatis mendaftarkan semua bangunan yang sudah ada di scene
            Debug.Log("Scanning for existing buildings in scene...");
            Building[] existingBuildings = FindObjectsOfType<Building>();
            foreach (Building building in existingBuildings)
            {
                RegisterPlacedBuilding(building);
            }
            Debug.Log($"Found and registered {existingBuildings.Length} existing buildings.");
        }

        /// <summary>
        /// Create mapping of prefab names to actual prefabs for quick loading
        /// </summary>
        private void InitializePrefabDictionary()
        {
            prefabDictionary.Clear();
            
            foreach (GameObject prefab in buildingPrefabs)
            {
                if (prefab != null)
                {
                    Building building = prefab.GetComponent<Building>();
                    if (building != null)
                    {
                        // Use both name and building name for flexibility
                        prefabDictionary[prefab.name] = prefab;
                        prefabDictionary[building.GetBuildingName()] = prefab;
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab {prefab.name} does not have a Building component!");
                    }
                }
            }
            
            Debug.Log($"BuildingManager initialized with {prefabDictionary.Count} building prefabs.");
        }        /// <summary>
        /// Register a building that has been placed in the world
        /// </summary>
        public void RegisterPlacedBuilding(Building building)
        {
            if (building != null && !placedBuildings.Contains(building))
            {
                placedBuildings.Add(building);
                Debug.Log($"Building registered: {building.GetBuildingName()} at position {building.transform.position} (Total: {placedBuildings.Count})");
            }
            else if (building == null)
            {
                Debug.LogError("Attempted to register null building!");
            }
            else if (placedBuildings.Contains(building))
            {
                // Building already registered, but let's check if it's still valid
                if (building.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Building {building.GetBuildingName()} already registered!");
                }
                else
                {
                    Debug.LogWarning($"Building {building.GetBuildingName()} is already registered but inactive. Removing and re-registering.");
                    placedBuildings.Remove(building);
                    placedBuildings.Add(building);
                }
            }
        }
        
        /// <summary>
        /// Unregister a building that has been destroyed
        /// </summary>
        public void UnregisterBuilding(Building building)
        {
            if (building != null && placedBuildings.Contains(building))
            {
                placedBuildings.Remove(building);
            }
        }        /// <summary>
        /// Save all placed buildings to GameData
        /// </summary>
        public void SavePlacedBuildings(GameData gameData)
        {
            if (gameData == null)
            {
                Debug.LogError("SavePlacedBuildings: gameData is null!");
                return;
            }
                
            // Clear existing building data
            gameData.placedBuildings.Clear();
            
            // Log before saving for debugging
            Debug.Log($"Attempting to save {placedBuildings.Count} buildings to game data.");
            
            // Jika tidak ada bangunan yang terdaftar, coba temukan semua yang ada di scene
            if (placedBuildings.Count == 0 || placedBuildings.Count < 3) // Force scan if less than expected
            {
                Debug.LogWarning($"Missing buildings! Found only {placedBuildings.Count}. Scanning scene for all buildings...");
                Building[] sceneBuildings = FindObjectsOfType<Building>();
                
                if (sceneBuildings.Length > 0)
                {
                    Debug.Log($"Found {sceneBuildings.Length} buildings in scene. Re-registering all of them...");
                    
                    // Clear existing list and register all again
                    placedBuildings.Clear();
                    
                    // Register semua bangunan yang ditemukan
                    foreach (Building building in sceneBuildings)
                    {
                        if (building != null)
                        {
                            RegisterPlacedBuilding(building);
                            Debug.Log($"Re-registered: {building.GetBuildingName()} at {building.transform.position}");
                        }
                    }
                }
            }
            
            // Double check - are buildings null?
            int nullBuildingsCount = 0;
            foreach (Building building in placedBuildings)
            {
                if (building == null)
                    nullBuildingsCount++;
            }
            
            if (nullBuildingsCount > 0)
            {
                Debug.LogWarning($"Found {nullBuildingsCount} null buildings in placedBuildings list! Cleaning up...");
                placedBuildings.RemoveAll(b => b == null);
                Debug.Log($"After cleanup: {placedBuildings.Count} buildings");
            }
            
            // Add data for each placed building
            foreach (Building building in placedBuildings)
            {
                if (building != null)
                {
                    string prefabName = GetPrefabName(building.gameObject);
                    BuildingData buildingData = new BuildingData(building, prefabName);
                    gameData.placedBuildings.Add(buildingData);
                    Debug.Log($"Saved building: {building.GetBuildingName()} at position {building.transform.position}");
                }
            }
            
            Debug.Log($"Saved {gameData.placedBuildings.Count} buildings to game data.");
        }
          /// <summary>
        /// Get the original prefab name for an instantiated building
        /// </summary>
        private string GetPrefabName(GameObject buildingObject)
        {
            if (buildingObject == null)
            {
                Debug.LogError("GetPrefabName: buildingObject is null!");
                return "Unknown";
            }
            
            // Default to object name if we can't find a better match
            string prefabName = buildingObject.name;
            
            // Remove "(Clone)" suffix if present
            if (prefabName.EndsWith("(Clone)"))
            {
                prefabName = prefabName.Substring(0, prefabName.Length - 7);
            }
            
            // Get name of prefab from building itself if available
            Building building = buildingObject.GetComponent<Building>();
            if (building != null)
            {
                string buildingName = building.GetBuildingName();
                if (!string.IsNullOrEmpty(buildingName))
                {
                    return buildingName; // Prefer building name from component
                }
            }
            
            return prefabName;
        }
        
        /// <summary>
        /// Load all buildings from GameData
        /// </summary>
        public void LoadPlacedBuildings(GameData gameData)
        {
            if (gameData == null || gameData.placedBuildings == null)
                return;
                
            // Clear existing buildings
            ClearAllBuildings();
            
            // Instantiate buildings from saved data
            foreach (BuildingData buildingData in gameData.placedBuildings)
            {
                GameObject prefab = GetBuildingPrefab(buildingData.prefabName);
                
                if (prefab != null)
                {
                    // Instantiate the building at the saved position and rotation
                    Vector3 position = buildingData.position.ToVector3();
                    Quaternion rotation = buildingData.rotation.ToQuaternion();
                    
                    GameObject buildingObject = Instantiate(prefab, position, rotation);
                    Building building = buildingObject.GetComponent<Building>();
                    
                    if (building != null)
                    {
                        // Set building properties from saved data
                        building.SetHealth(buildingData.health);
                        
                        // Register the building
                        RegisterPlacedBuilding(building);
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find prefab for building: {buildingData.prefabName}");
                }
            }
            
            Debug.Log($"Loaded {gameData.placedBuildings.Count} buildings from game data.");
        }
        
        /// <summary>
        /// Get building prefab by name
        /// </summary>
        private GameObject GetBuildingPrefab(string prefabName)
        {
            // Try direct lookup first
            if (prefabDictionary.TryGetValue(prefabName, out GameObject prefab))
            {
                return prefab;
            }
            
            // If not found, try alternative names
            foreach (var entry in prefabDictionary)
            {
                if (entry.Key.Contains(prefabName) || prefabName.Contains(entry.Key))
                {
                    return entry.Value;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Clear all placed buildings from the scene
        /// </summary>
        public void ClearAllBuildings()
        {
            // Create a copy to avoid collection modification issues
            List<Building> buildingsCopy = new List<Building>(placedBuildings);
            
            foreach (Building building in buildingsCopy)
            {
                if (building != null)
                {
                    Destroy(building.gameObject);
                }
            }
            
            placedBuildings.Clear();
        }
        
        /// <summary>
        /// Get count of placed buildings
        /// </summary>
        public int GetPlacedBuildingCount()
        {
            return placedBuildings.Count;
        }
        
        /// <summary>
        /// Add a building prefab to the available prefabs
        /// </summary>
        public void AddBuildingPrefab(GameObject prefab)
        {
            if (prefab != null && prefab.GetComponent<Building>() != null)
            {
                buildingPrefabs.Add(prefab);
                
                // Update the dictionary
                Building building = prefab.GetComponent<Building>();
                prefabDictionary[prefab.name] = prefab;
                prefabDictionary[building.GetBuildingName()] = prefab;
            }
        }
          /// <summary>
        /// Memastikan instance BuildingManager ada dan valid
        /// </summary>
        /// <returns>Instance BuildingManager yang valid</returns>
        public static BuildingManager EnsureInstance()
        {
            if (Instance == null)
            {
                Debug.Log("Creating BuildingManager because it doesn't exist...");
                GameObject managerObject = new GameObject("BuildingManager");
                BuildingManager instance = managerObject.AddComponent<BuildingManager>();
                
                // Secara otomatis mendaftarkan semua bangunan yang sudah ada di scene
                Building[] existingBuildings = FindObjectsOfType<Building>();
                Debug.Log($"Found {existingBuildings.Length} existing buildings in scene during EnsureInstance()");
                
                foreach (Building building in existingBuildings)
                {
                    if (building != null)
                    {
                        instance.RegisterPlacedBuilding(building);
                        Debug.Log($"Auto-registered building: {building.GetBuildingName()} at {building.transform.position}");
                    }
                }
                
                return instance;
            }
            
            return Instance;
        }

        /// <summary>
        /// Membersihkan daftar bangunan yang terdaftar (untuk kasus reset)
        /// </summary>
        public void ClearBuildingsList()
        {
            placedBuildings.Clear();
            Debug.Log("BuildingManager: Building list cleared for fresh registration");
        }
    }
}
