using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingdomClash;
using System;

namespace KingdomClash.UI
{    /// <summary>
    /// Panel UI untuk FarmBuilding dengan fitur upgrade dengan delay
    /// </summary>
    public class FarmUpgradeProgressPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider upgradeProgressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button cancelButton;
        
        [Header("References")]
        [SerializeField] private FarmBuilding linkedFarm; // Farm yang terhubung secara langsung di editor
        
        [Header("Debug Options")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Use the linkedFarm if no target farm is set at runtime
        private FarmBuilding targetFarm;
        
        private void Start()
        {
            // Setup tombol cancel jika ada
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(CancelUpgrade);
                if (showDebugLogs) Debug.Log("Cancel button listener set in Start()");
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Cancel button reference is missing in Start()");
            }

            // Periksa elemen UI lain
            if (progressText == null)
            {
                if (showDebugLogs) Debug.LogWarning("Progress text reference is missing!");
            }

            if (upgradeProgressBar == null)
            {
                if (showDebugLogs) Debug.LogWarning("Upgrade progress bar reference is missing!");
            }
            
            // Initialize the target farm with linkedFarm if available
            if (linkedFarm != null)
            {
                SetupForFarm(linkedFarm);
                if (showDebugLogs) Debug.Log($"Initialized with editor-linked farm: {linkedFarm.GetBuildingName()}");
            }

            // Sembunyikan panel saat awal
            gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            // Dipanggil setiap kali panel diaktifkan
            if (showDebugLogs) Debug.Log("Farm upgrade progress panel enabled");
            
            // Check if we already have a linked farm but no target farm
            if (targetFarm == null && linkedFarm != null)
            {
                targetFarm = linkedFarm;
                if (showDebugLogs) Debug.Log($"Using linked farm as target: {linkedFarm.GetBuildingName()}");
            }
            
            // Re-subscribe ke event jika perlu
            if (targetFarm != null && targetFarm.OnUpgradeProgressChanged != null)
            {
                if (showDebugLogs) Debug.Log("Re-subscribing to farm upgrade progress events");
                targetFarm.OnUpgradeProgressChanged -= UpdateProgressDisplay; // Hapus untuk menghindari duplikasi
                targetFarm.OnUpgradeProgressChanged += UpdateProgressDisplay;
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning("No farm is connected to the upgrade panel!");
            }
        }        /// <summary>
        /// Setup panel untuk farm tertentu
        /// </summary>
        public void SetupForFarm(FarmBuilding farm)
        {
            // Hapus listener lama jika ada
            if (targetFarm != null)
            {
                targetFarm.OnUpgradeProgressChanged -= UpdateProgressDisplay;
            }
            
            // Set farm baru
            targetFarm = farm;
            
            // Subscribe ke event progress
            if (targetFarm != null)
            {
                targetFarm.OnUpgradeProgressChanged += UpdateProgressDisplay;
                
                // Reset progress bar dan text
                if (upgradeProgressBar != null)
                {
                    upgradeProgressBar.value = 0;
                }
                
                // Reset dan update text
                if (progressText != null)
                {
                    progressText.text = "Upgrade Progress: 0%";
                    // Pastikan text terlihat
                    progressText.gameObject.SetActive(true);
                }
                
                // Log status setup
                if (showDebugLogs) Debug.Log($"Farm upgrade panel setup for {farm.GetBuildingName()} (Level {farm.GetCurrentLevel()})");
                
                // Periksa apakah tombol cancel valid
                if (cancelButton != null)
                {
                    cancelButton.onClick.RemoveAllListeners();
                    cancelButton.onClick.AddListener(CancelUpgrade);
                    cancelButton.interactable = true;
                    if (showDebugLogs) Debug.Log("Cancel button listener setup successfully");
                }
                else
                {
                    if (showDebugLogs) Debug.LogWarning("Cancel button reference is missing!");
                }
            }
        }/// <summary>
        /// Update UI progress
        /// </summary>
        private void UpdateProgressDisplay(float progress)
        {
            // Log progress untuk debugging
            if (showDebugLogs)
            {
                Debug.Log($"Upgrade Progress: {progress:P0} ({progress * 100}%)");
            }
            
            // Update progress bar
            if (upgradeProgressBar != null)
            {
                upgradeProgressBar.value = progress;
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Progress bar reference is missing!");
            }
            
            // Update text jika ada
            if (progressText != null)
            {
                int percentage = Mathf.RoundToInt(progress * 100);
                progressText.text = $"Upgrade Progress: {percentage}%";
                
                // Pastikan text terlihat
                if (!progressText.gameObject.activeSelf)
                {
                    progressText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Progress text reference is missing!");
            }
            
            // Pastikan panel tetap aktif selama upgrade berlangsung
            if (!gameObject.activeSelf && progress > 0 && progress < 1)
            {
                gameObject.SetActive(true);
                if (showDebugLogs) Debug.Log("Activating upgrade progress panel");
            }
            else if (gameObject.activeSelf && (progress <= 0 || progress >= 1))
            {
                gameObject.SetActive(false);
                if (showDebugLogs) Debug.Log("Deactivating upgrade progress panel");
            }
        }        /// <summary>
        /// Batalkan upgrade yang sedang berjalan
        /// </summary>
        private void CancelUpgrade()
        {
            if (showDebugLogs) Debug.Log("Cancel upgrade button clicked");
            
            // Check if there's a linked farm from the editor first, then try targetFarm
            FarmBuilding farmToCancel = targetFarm != null ? targetFarm : linkedFarm;
            
            if (farmToCancel != null)
            {
                try
                {
                    // Panggil metode cancel di FarmBuilding
                    farmToCancel.CancelUpgrade();
                    if (showDebugLogs) Debug.Log("Farm upgrade cancelled successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error cancelling farm upgrade: {e.Message}");
                }
                
                // Sembunyikan panel
                gameObject.SetActive(false);
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Cannot cancel upgrade: No target farm assigned");
            }
        }
          private void OnDestroy()
        {
            // Unsubscribe dari event saat panel dihancurkan
            if (targetFarm != null)
            {
                targetFarm.OnUpgradeProgressChanged -= UpdateProgressDisplay;
                if (showDebugLogs) Debug.Log("Unsubscribed from farm upgrade events on destroy");
            }
        }
    }
}
