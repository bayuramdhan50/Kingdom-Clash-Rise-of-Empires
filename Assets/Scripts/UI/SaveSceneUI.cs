using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Get current game state data from GameManager's pre-captured data
        /// </summary>
        public static GameData PreCapturedGameData
        {
            get
            {
                if (GameManager.Instance != null)
                {
                    return GameManager.Instance.GetPreCapturedGameData();
                }
                return null;
            }
        }
        
        // Path where save files are stored
        private string saveDirectoryPath;        // Awake method removed to clean up debug logs

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
                if (slot == null) continue;
                
                // Check if there's a save file for this slot
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
                Debug.LogError("[SaveSceneUI.OnSlotClicked] No game data to save!");
                return;
            }

            // Save game to this slot
            SaveGame(saveFileName, currentGameData);

            // Update the slot UI
            UpdateSlotUI(saveSlots[slotIndex], currentGameData, slotIndex);
        }        /// <summary>
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
                
                // Count buildings, units and training
                int buildingCount = saveData.placedBuildings?.Count ?? 0;
                int unitCount = saveData.units?.Count ?? 0;
                int trainingCount = 0;
                
                // Count total units in training
                if (saveData.trainingProcesses != null)
                {
                    foreach (var training in saveData.trainingProcesses)
                    {
                        trainingCount += training.count;
                    }
                }
                
                // Format the text with more information
                slotText.text = $"{slotType}: {saveData.playerName}\n" +
                               $"Level {saveData.level} - {saveData.dateTime}\n" +
                               $"Buildings: {buildingCount} | Units: {unitCount} | Training: {trainingCount}";
            }
        }/// <summary>
        /// Get the current game data to save
        /// </summary>
        /// <returns>The current game data</returns>
        private GameData GetCurrentGameData()
        {
            // First check if we have pre-captured data from game scene - IMPORTANT!
            if (PreCapturedGameData != null)
            {
                // Make a deep copy to ensure no reference issues
                GameData gameCopy = new GameData();
                // Copy basic properties
                gameCopy.playerName = PreCapturedGameData.playerName;
                gameCopy.level = PreCapturedGameData.level;
                gameCopy.dateTime = PreCapturedGameData.dateTime;
                gameCopy.resources = PreCapturedGameData.resources;
                gameCopy.selectedCharacter = PreCapturedGameData.selectedCharacter;
                gameCopy.cameraPosition = PreCapturedGameData.cameraPosition;
                gameCopy.cameraRotation = PreCapturedGameData.cameraRotation;
                gameCopy.cameraZoom = PreCapturedGameData.cameraZoom;
                
                // Create new buildings list
                gameCopy.placedBuildings = new List<BuildingData>();
                
                // Deep copy buildings
                if (PreCapturedGameData.placedBuildings != null)
                {
                    foreach (var building in PreCapturedGameData.placedBuildings)
                    {
                        if (building != null)
                        {
                            // Create a completely new BuildingData object
                            BuildingData newBuildingData = new BuildingData();
                            
                            // Copy all properties
                            newBuildingData.buildingName = building.buildingName;
                            newBuildingData.prefabName = building.prefabName;
                            newBuildingData.position = building.position;
                            newBuildingData.rotation = building.rotation;
                            newBuildingData.health = building.health;
                            newBuildingData.maxHealth = building.maxHealth;
                            newBuildingData.producesResources = building.producesResources;
                            newBuildingData.resourceType = building.resourceType;
                            newBuildingData.productionAmount = building.productionAmount;
                            
                            // Add to our copy
                            gameCopy.placedBuildings.Add(newBuildingData);
                        }
                    }
                }
                
                // Save training data and units manually
                SaveTrainingDataAndUnits(gameCopy);
                
                return gameCopy;
            }
            
            // Try to get data from GameManager
            if (GameManager.Instance != null)
            {
                GameData gameData = GameManager.Instance.GetCurrentGameData();
                return gameData;
            }
              // Fallback: create basic game data
            GameData data = new GameData
            {
                playerName = "Player",
                level = 1,
                dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                placedBuildings = new List<BuildingData>() // Ensure list is initialized
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
            }            catch
            {
                return new string[0];
            }
        }
        
        /// <summary>
        /// Save game data to file - rebuilt from scratch to be cleaner
        /// </summary>
        /// <param name="saveFileName">The name of the save file</param>
        /// <param name="saveData">The game data to save</param>
        private void SaveGame(string saveFileName, GameData saveData)
        {            // TIMING: Delay execution until the end of the frame to ensure scene is fully loaded
            StartCoroutine(DelayedSaveGame(saveFileName, saveData));
        }        /// <summary>
        /// Delayed execution of save game to ensure scene is fully loaded
        /// </summary>
        private System.Collections.IEnumerator DelayedSaveGame(string saveFileName, GameData saveData)
        {
            // Wait for the end of the frame
            yield return new WaitForEndOfFrame();
            
            try
            {
                // Update the save time
                saveData.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Final check on required data
                if (saveData.placedBuildings == null)
                {
                    saveData.placedBuildings = new List<BuildingData>();
                }
                
                if (saveData.units == null)
                {
                    saveData.units = new List<UnitData>();
                }
                
                if (saveData.trainingProcesses == null)
                {
                    saveData.trainingProcesses = new List<TrainingData>();
                }
                
                // One final check to ensure we have training data and units
                SaveTrainingDataAndUnits(saveData);
                
                // Convert data to JSON
                string json = JsonUtility.ToJson(saveData, true);
                
                // Write to file
                string filePath = Path.Combine(saveDirectoryPath, saveFileName + ".json");
                File.WriteAllText(filePath, json);
                  Debug.Log($"Game saved to {filePath} with {saveData.placedBuildings.Count} buildings, " +
                          $"{saveData.units.Count} units, and {saveData.trainingProcesses.Count} training processes.");
                
                // Update slot UI after save is completed
                UpdateSlotUI(saveSlots[int.Parse(saveFileName.Split('_')[1])], saveData, int.Parse(saveFileName.Split('_')[1]));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
                GameData loadedData = JsonUtility.FromJson<GameData>(json);
                return loadedData;
            }            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Return to the game scene
        /// </summary>
        private void ReturnToGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Manually save training data and units from TrainingManager
        /// </summary>
        /// <param name="gameData">The game data to update with training and unit data</param>
        private void SaveTrainingDataAndUnits(GameData gameData)
        {
            // Ensure lists are initialized
            if (gameData.trainingProcesses == null)
            {
                gameData.trainingProcesses = new List<TrainingData>();
            }
            else
            {
                gameData.trainingProcesses.Clear();
            }
            
            if (gameData.units == null)
            {
                gameData.units = new List<UnitData>();
            }
            
            // Check if we have a TrainingManager instance
            if (UI.TrainingManager.Instance != null)
            {
                // Tell the TrainingManager to save its state to GameManager
                UI.TrainingManager.Instance.SaveTrainingState();
                
                // Get the saved training data from GameManager
                if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameData() != null)
                {
                    GameData currentData = GameManager.Instance.GetCurrentGameData();
                    
                    // Copy training processes
                    if (currentData.trainingProcesses != null)
                    {
                        foreach (var trainingProcess in currentData.trainingProcesses)
                        {
                            // Create a new training data
                            TrainingData newTraining = new TrainingData(
                                trainingProcess.buildingName,
                                trainingProcess.unitType,
                                trainingProcess.count,
                                trainingProcess.progress,
                                trainingProcess.trainingTime
                            );
                            
                            // Add to our copy
                            gameData.trainingProcesses.Add(newTraining);
                        }
                        
                        Debug.Log($"Manually saved {gameData.trainingProcesses.Count} training processes");
                    }
                    
                    // Copy units
                    if (currentData.units != null)
                    {
                        // Clear the units list first
                        if (gameData.units == null)
                        {
                            gameData.units = new List<UnitData>();
                        }
                        
                        foreach (var unit in currentData.units)
                        {
                            // Create a new unit data
                            UnitData newUnit = new UnitData(
                                unit.unitType,
                                unit.health,
                                unit.maxHealth,
                                unit.attack,
                                unit.defense,
                                unit.position.ToVector3()
                            );
                            
                            // Add to our copy
                            gameData.units.Add(newUnit);
                        }
                        
                        Debug.Log($"Manually saved {gameData.units.Count} units");
                    }
                }
            }
            else
            {
                Debug.LogWarning("TrainingManager instance not found. Cannot save training data and units.");
            }
        }
    }
}
