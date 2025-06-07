using UnityEngine;
using System.IO;
using System.Linq;

namespace KingdomClash
{
    /// <summary>
    /// Manages saving and loading game data
    /// Implemented as a singleton to be accessible from any script
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        // Singleton instance
        public static SaveManager Instance { get; private set; }

        // Path for save data
        private string saveDirectoryPath;

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

            // Initialize save paths
            saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Saves");

            // Create save directory if it doesn't exist
            if (!Directory.Exists(saveDirectoryPath))
            {
                Directory.CreateDirectory(saveDirectoryPath);
            }
        }

        /// <summary>
        /// Checks if any save files exist
        /// </summary>
        /// <returns>True if any saves exist, false otherwise</returns>
        public bool DoSavesExist()
        {
            string[] saveFiles = GetAllSaveFiles();
            return saveFiles != null && saveFiles.Length > 0;
        }        /// <summary>
        /// Save a game to a specific slot
        /// </summary>
        /// <param name="slotIndex">The slot index to save to</param>
        /// <param name="gameData">The game data to save</param>
        public void SaveGameToSlot(int slotIndex, GameData gameData)
        {
            string filePath = Path.Combine(saveDirectoryPath, $"SaveSlot_{slotIndex}.json");
            SaveGameData(gameData, filePath);
            Debug.Log($"Game saved to slot {slotIndex} at: {filePath}");
        }

        /// <summary>
        /// Load a game from a specific slot
        /// </summary>
        /// <param name="slotIndex">The slot index to load from</param>
        /// <returns>The game data or null if no save exists in that slot</returns>
        public GameData LoadGameFromSlot(int slotIndex)
        {
            string filePath = Path.Combine(saveDirectoryPath, $"SaveSlot_{slotIndex}.json");
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file exists in slot {slotIndex}!");
                return null;
            }

            string json = File.ReadAllText(filePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"Game loaded successfully from slot {slotIndex}");
            return data;
        }

    /// <summary>
    /// Saves the current game state
    /// </summary>
    /// <param name="gameData">The game data to save</param>
    /// <param name="filePath">The file path to save to</param>
    private void SaveGameData(GameData gameData, string filePath)
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Saves the current game state
    /// </summary>
    /// <param name="gameData">The game data to save</param>
    /// <param name="slotIndex">The slot index to save to (0 for autosave)</param>
    public void SaveCurrentGame(GameData gameData, int slotIndex = 0)
    {
        if (gameData == null)
        {
            Debug.LogError("Cannot save null game data!");
            return;
        }

        // Pastikan BuildingManager ada dan semua bangunan terdaftar
        if (BuildingManager.Instance == null)
        {
            Debug.LogWarning("BuildingManager.Instance is null during save! Creating temporary instance...");
            BuildingManager.EnsureInstance();
        }
        
        // Update timestamp
        gameData.dateTime = System.DateTime.Now.ToString();
        
        // Save to the specified slot
        string filePath = Path.Combine(saveDirectoryPath, $"SaveSlot_{slotIndex}.json");
        SaveGameData(gameData, filePath);
        
        // Log the JSON for debugging
        string jsonData = JsonUtility.ToJson(gameData, true);
        Debug.Log($"Saving game data:\n{jsonData}");
        
        Debug.Log($"Game saved successfully to slot {slotIndex} at: {filePath}");
    }

    /// <summary>
    /// Lists all available save files
    /// </summary>
    /// <returns>Array of save file names</returns>
    public string[] GetAllSaveFiles()
    {
        if (!Directory.Exists(saveDirectoryPath))
        {
            return new string[0];
        }

        return Directory.GetFiles(saveDirectoryPath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();
    }
}
    // GameData class dipindahkan ke GameData.cs
    
    // Vector3Data and QuaternionData classes dipindahkan ke GameData.cs    // Kelas Resources dipindahkan ke GameData.cs

    // Kelas BuildingData dipindahkan ke GameData.cs
}
