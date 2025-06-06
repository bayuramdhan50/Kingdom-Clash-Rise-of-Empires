using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using TMPro;

namespace KingdomClash
{
    /// <summary>
    /// Handles the Save Game UI interactions with pre-created save slots
    /// </summary>
    public class SaveSceneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject[] saveSlots; // Array of pre-created save slot GameObjects
        [SerializeField] private Button backButton; // Button to return to game
        [SerializeField] private string gameSceneName = "GameScene"; // Scene to return to after saving

        // Path where save files are stored
        private string saveDirectoryPath;

        private void Start()
        {
            // Initialize save directory path
            saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(saveDirectoryPath))
            {
                Directory.CreateDirectory(saveDirectoryPath);
            }

            // Initialize back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToGame);
            }

            // Check if we have save slots
            if (saveSlots == null || saveSlots.Length == 0)
            {
                Debug.LogError("No save slot GameObjects assigned!");
                return;
            }

            // Load and initialize save slots
            InitializeSaveSlots();
        }

        /// <summary>
        /// Initialize all save slots
        /// </summary>
        private void InitializeSaveSlots()
        {
            // Get all save files
            string[] saveFiles = GetSaveFiles();

            // Configure each save slot
            for (int i = 0; i < saveSlots.Length; i++)
            {
                GameObject slot = saveSlots[i];
                if (slot == null) continue;                // Check if there's a save file for this slot
                string saveFileName = $"SaveSlot_{i}";
                bool saveExists = Array.Exists(saveFiles, file => file == saveFileName);

                // Set up the slot based on whether it has a save or not
                if (saveExists)
                {
                    // Load and display existing save data
                    GameData saveData = LoadSaveData(saveFileName);
                    UpdateSlotUI(slot, saveData, i);
                }
                else
                {
                    // Set up empty slot
                    TextMeshProUGUI slotText = slot.GetComponentInChildren<TextMeshProUGUI>();
                    if (slotText != null)
                    {
                        slotText.text = i == 0 ? "Auto Save" : "Empty Slot";
                    }
                }

                // Configure the button (final index needs to be captured for lambda)
                int slotIndex = i;
                Button slotButton = slot.GetComponentInChildren<Button>();
                if (slotButton != null)
                {
                    slotButton.onClick.RemoveAllListeners();
                    slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
                    slotButton.interactable = true; // All slots are clickable
                }
            }
        }

        /// <summary>
        /// Handle slot button click
        /// </summary>
        /// <param name="slotIndex">Index of the clicked save slot</param>
        private void OnSlotClicked(int slotIndex)
        {
            string saveFileName = $"SaveSlot_{slotIndex}";
            
            // Get current game data
            GameData currentGameData = GetCurrentGameData();
            if (currentGameData == null)
            {
                Debug.LogError("No game data to save!");
                return;
            }

            // Save game to this slot
            SaveGame(saveFileName, currentGameData);

            // Update the slot UI
            UpdateSlotUI(saveSlots[slotIndex], currentGameData, slotIndex);
            
            // Show confirmation message
            Debug.Log($"Game saved to slot {slotIndex}");
        }

        /// <summary>
        /// Update the UI elements of a save slot
        /// </summary>
        /// <param name="slot">The slot GameObject to update</param>
        /// <param name="saveData">The save data</param>
        /// <param name="slotIndex">The slot index</param>
        private void UpdateSlotUI(GameObject slot, GameData saveData, int slotIndex)
        {
            if (saveData == null) return;

            // Update slot text
            TextMeshProUGUI slotText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (slotText != null)
            {
                string slotType = slotIndex == 0 ? "Auto Save" : $"Save {slotIndex}";
                slotText.text = $"{slotType}: {saveData.playerName}\nLevel {saveData.level} - {saveData.dateTime}";
            }
        }

        /// <summary>
        /// Get the current game data to save
        /// </summary>
        /// <returns>The current game data</returns>
        private GameData GetCurrentGameData()
        {
            // Try to get data from GameManager
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.GetCurrentGameData();
            }
            
            // Fallback: create basic game data
            GameData data = new GameData
            {
                playerName = "Player",
                level = 1,
                dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                // Add other game state data here
            };
            
            return data;
        }

        /// <summary>
        /// Get all save files in the save directory
        /// </summary>
        /// <returns>Array of save file names (without extension)</returns>
        private string[] GetSaveFiles()
        {
            if (!Directory.Exists(saveDirectoryPath))
            {
                return new string[0];
            }

            try
            {
                // Get all json files in the save directory
                string[] filePaths = Directory.GetFiles(saveDirectoryPath, "*.json");
                
                // Extract just the filename without extension
                string[] saveFiles = new string[filePaths.Length];
                for (int i = 0; i < filePaths.Length; i++)
                {
                    saveFiles[i] = Path.GetFileNameWithoutExtension(filePaths[i]);
                }
                
                return saveFiles;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get save files: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// Save game data to file
        /// </summary>
        /// <param name="saveFileName">The name of the save file</param>
        /// <param name="saveData">The game data to save</param>
        private void SaveGame(string saveFileName, GameData saveData)
        {
            try
            {
                // Update the save time
                saveData.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Convert data to JSON
                string json = JsonUtility.ToJson(saveData, true);
                
                // Write to file
                string filePath = Path.Combine(saveDirectoryPath, saveFileName + ".json");
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}");
            }
        }

        /// <summary>
        /// Load save data from file
        /// </summary>
        /// <param name="saveFileName">The name of the save file</param>
        /// <returns>The loaded game data or null if loading fails</returns>
        private GameData LoadSaveData(string saveFileName)
        {
            string filePath = Path.Combine(saveDirectoryPath, saveFileName + ".json");
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Return to the game scene
        /// </summary>
        public void ReturnToGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
