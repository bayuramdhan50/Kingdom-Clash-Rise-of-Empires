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
        private string latestSaveFilePath;

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
            latestSaveFilePath = Path.Combine(saveDirectoryPath, "latest_save.json");

            // Create save directory if it doesn't exist
            if (!Directory.Exists(saveDirectoryPath))
            {
                Directory.CreateDirectory(saveDirectoryPath);
            }
        }

        /// <summary>
        /// Checks if any save file exists
        /// </summary>
        /// <returns>True if save exists, false otherwise</returns>
        public bool DoesSaveExist()
        {
            return File.Exists(latestSaveFilePath);
        }        /// <summary>
        /// Creates a new save file
        /// </summary>
        public void CreateNewSave(Characters.CharacterType characterType = Characters.CharacterType.Arvandir)
        {
            GameData newGameData = new GameData
            {
                playerName = "Player",
                level = 1,
                resources = new Resources { wood = 500, stone = 300, iron = 200, food = 600 },
                selectedCharacter = characterType
            };

            SaveGameData(newGameData, latestSaveFilePath);
            Debug.Log($"New save created with character {characterType} at: {latestSaveFilePath}");
        }

        /// <summary>
        /// Loads the last saved game
        /// </summary>
        /// <returns>The game data or null if no save exists</returns>
        public GameData LoadLastSave()
        {
            if (!DoesSaveExist())
            {
                Debug.LogWarning("No save file exists to load!");
                return null;
            }

            string json = File.ReadAllText(latestSaveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game loaded successfully from: " + latestSaveFilePath);
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
    public void SaveCurrentGame(GameData gameData)
    {
        // Save to the latest save file
        SaveGameData(gameData, latestSaveFilePath);
        
        // Also save a timestamped version for the load game menu
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string timestampedFilePath = Path.Combine(saveDirectoryPath, $"save_{timestamp}.json");
        SaveGameData(gameData, timestampedFilePath);
        
        Debug.Log("Game saved successfully to: " + latestSaveFilePath);
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
    }    /// <summary>
    /// Stores all game data for saving and loading
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public string playerName;
        public int level;
        public Resources resources;
        public Characters.CharacterType selectedCharacter;
        public string dateTime = System.DateTime.Now.ToString();
        // Add more game state variables as needed
    }/// <summary>
    /// Stores resource information for the game
    /// </summary>
    [System.Serializable]
    public class Resources
    {
        public int wood;  // Kayu
        public int stone; // Batu
        public int iron;  // Besi
        public int food;  // Makanan
    }
}
