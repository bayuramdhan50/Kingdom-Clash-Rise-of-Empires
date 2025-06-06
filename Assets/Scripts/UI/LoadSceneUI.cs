using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq;
using TMPro;

namespace KingdomClash
{
    /// <summary>
    /// Handles the Load Game Scene UI interactions and logic
    /// </summary>
    public class LoadSceneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject[] saveSlots; // Array of pre-created save slot GameObjects
        [SerializeField] private Button backButton; // Button to return to main menu
        [SerializeField] private TextMeshProUGUI noSavesText; // Text to display when no saves exist

        [Header("Settings")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private int maxSaveSlots = 10; // Maximum number of save slots to display
        
        [Header("Auto Save")]
        [SerializeField] private bool useAutoSave = true; // Whether to treat slot 0 as auto-save
        [SerializeField] private string autoSaveFileName = "SaveSlot_0"; // Name of auto-save file

        // Path where save files are stored
        private string saveDirectoryPath;

        private void Start()
        {
            // Initialize save directory path
            saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Saves");

            // Initialize back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToMainMenu);
            }
            else
            {
                Debug.LogError("Back button reference is missing!");
            }

            // Check if we have enough save slots
            if (saveSlots == null || saveSlots.Length == 0)
            {
                Debug.LogError("No save slot GameObjects assigned!");
                return;
            }

            // Load and display save slots
            LoadSaveSlots();
        }

        /// <summary>
        /// Load and display all save slots
        /// </summary>
        private void LoadSaveSlots()
        {
            // Make sure all slots are active first (all slots remain visible in the UI)
            foreach (GameObject slot in saveSlots)
            {
                if (slot != null)
                {
                    slot.SetActive(true);
                    
                    // Clear previous text (optional)
                    TextMeshProUGUI slotText = slot.GetComponentInChildren<TextMeshProUGUI>();
                    if (slotText != null)
                    {
                        slotText.text = "Empty Slot";
                    }
                    
                    // Remove previous button listeners
                    Button slotButton = slot.GetComponentInChildren<Button>();
                    if (slotButton != null)
                    {
                        slotButton.onClick.RemoveAllListeners();
                        // Optionally disable the button for empty slots
                        slotButton.interactable = false;
                    }
                }
            }

            // Check if SaveManager exists
            if (SaveManager.Instance == null)
            {
                GameObject saveManagerObj = new GameObject("SaveManager");
                saveManagerObj.AddComponent<SaveManager>();
            }

            // Get all save files
            string[] saveFiles = SaveManager.Instance.GetAllSaveFiles();

            // Show/hide "No Saves" text
            if (noSavesText != null)
            {
                noSavesText.gameObject.SetActive(saveFiles.Length == 0);
            }

            if (saveFiles.Length == 0)
            {
                Debug.Log("No save files found.");
                return;
            }

            // Handle auto-save slot first if it exists
            if (useAutoSave && saveSlots.Length > 0)
            {
                // Check if auto-save exists
                bool autoSaveExists = Array.Exists(saveFiles, file => file == autoSaveFileName);
                
                if (autoSaveExists)
                {
                    // Configure the first slot as auto-save
                    ConfigureSaveSlot(saveSlots[0], autoSaveFileName, true);
                    
                    // Set text to indicate it's auto-save
                    TextMeshProUGUI slotText = saveSlots[0].GetComponentInChildren<TextMeshProUGUI>();
                    if (slotText != null)
                    {
                        string currentText = slotText.text;
                        slotText.text = $"Auto Save\n{currentText}";
                    }
                }
                else
                {
                    // Set text to indicate this is the auto-save slot but empty
                    TextMeshProUGUI slotText = saveSlots[0].GetComponentInChildren<TextMeshProUGUI>();
                    if (slotText != null)
                    {
                        slotText.text = "Auto Save\n(Empty)";
                    }
                }
            }

            // Configure remaining save slots for each save file (skipping auto-save if already handled)
            int startIndex = useAutoSave ? 1 : 0;
            int fileIndex = 0;
            
            // Limit the number of slots to process based on maxSaveSlots setting
            int slotsToProcess = Mathf.Min(saveSlots.Length, maxSaveSlots);
            
            for (int i = startIndex; i < slotsToProcess && fileIndex < saveFiles.Length; i++)
            {
                // Skip the auto-save file when processing regular slots
                if (useAutoSave && saveFiles[fileIndex] == autoSaveFileName)
                {
                    fileIndex++;
                    // Skip this iteration if we've reached the end of save files
                    if (fileIndex >= saveFiles.Length) break;
                }
                
                // Configure this slot
                ConfigureSaveSlot(saveSlots[i], saveFiles[fileIndex], false);
                fileIndex++;
            }
        }

        /// <summary>
        /// Configure a UI save slot for a save file
        /// </summary>
        /// <param name="slotObject">The slot GameObject to configure</param>
        /// <param name="saveFileName">The name of the save file</param>
        /// <param name="isAutoSave">Whether this is the auto-save slot</param>
        private void ConfigureSaveSlot(GameObject slotObject, string saveFileName, bool isAutoSave)
        {
            // Get save data
            GameData saveData = LoadSaveData(saveFileName);
            if (saveData == null)
            {
                Debug.LogError($"Failed to load save data for {saveFileName}");
                return;
            }

            // Set slot info
            TextMeshProUGUI slotText = slotObject.GetComponentInChildren<TextMeshProUGUI>();
            if (slotText != null)
            {
                // Format: "Player Name - Level X - DateTime"
                if (!isAutoSave)
                {
                    slotText.text = $"{saveData.playerName} - Level {saveData.level}\n{saveData.dateTime}";
                }
                else
                {
                    // For auto-save, text is set in LoadSaveSlots with the "Auto Save" prefix
                    slotText.text = $"{saveData.playerName} - Level {saveData.level}\n{saveData.dateTime}";
                }
            }

            // Add load action to button
            Button slotButton = slotObject.GetComponentInChildren<Button>();
            if (slotButton != null)
            {
                // Make sure the button is interactable
                slotButton.interactable = true;
                
                // Remove any existing listeners to prevent duplicates
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => LoadGame(saveData));
            }
        }

        /// <summary>
        /// Load save data from file
        /// </summary>
        /// <param name="saveFileName">The name of the save file</param>
        /// <returns>The loaded game data or null if loading fails</returns>
        private GameData LoadSaveData(string saveFileName)
        {
            if (!Directory.Exists(saveDirectoryPath))
            {
                Debug.LogError("Save directory does not exist!");
                return null;
            }

            string filePath = Path.Combine(saveDirectoryPath, saveFileName + ".json");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file {filePath} does not exist!");
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
        /// Load a game from the selected save data
        /// </summary>
        /// <param name="saveData">The save data to load</param>
        public void LoadGame(GameData saveData)
        {
            Debug.Log($"Loading game for {saveData.playerName} - Level {saveData.level}");

            // Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }

            // Pass data to GameManager and load game
            GameManager.Instance.LoadGame(saveData);
        }

        /// <summary>
        /// Return to the main menu scene
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("Returning to main menu");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
