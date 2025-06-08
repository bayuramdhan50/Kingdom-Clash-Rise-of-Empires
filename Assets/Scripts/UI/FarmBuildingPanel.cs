using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingdomClash;

namespace KingdomClash.UI
{
    /// <summary>
    /// UI Panel untuk mengelola Farm Building
    /// </summary>
    public class FarmBuildingPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI storageText;
        [SerializeField] private TextMeshProUGUI productionRateText;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("Upgrade Panel")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private TextMeshProUGUI upgradeCostWoodText;
        [SerializeField] private TextMeshProUGUI upgradeCostStoneText;
        [SerializeField] private TextMeshProUGUI upgradeCostIronText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeLevelText;
        
        [Header("Progress Bars")]
        [SerializeField] private Slider storageProgressBar;
        [SerializeField] private Slider productionProgressBar;
        
        [Header("Action Buttons")]
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button destroyButton;
        
        // Reference to the current farm building
        private FarmBuilding currentFarm;
        
        // Singleton instance
        public static FarmBuildingPanel Instance { get; private set; }
        
        private void Awake()
        {
            // Setup singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Hide panel at start
            gameObject.SetActive(false);
        }
          private void Start()
        {
            // Setup button listeners
            if (upgradeButton != null)
            {
                // Remove any existing listeners first
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(UpgradeCurrentFarm);
                Debug.Log("Upgrade button listener setup");
            }
            else
            {
                Debug.LogWarning("Upgrade button reference missing in FarmBuildingPanel");
            }
                
            if (harvestButton != null)
            {
                // Remove any existing listeners first
                harvestButton.onClick.RemoveAllListeners();
                harvestButton.onClick.AddListener(HarvestCurrentFarm);
                Debug.Log("Harvest button listener setup");
            }
            else
            {
                Debug.LogWarning("Harvest button reference missing in FarmBuildingPanel");
            }
                
            if (closeButton != null)
            {
                // Remove any existing listeners first
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseButtonPressed);
                Debug.Log("Close button listener setup");
            }
            else
            {
                Debug.LogWarning("Close button reference missing in FarmBuildingPanel");
            }
            
            if (destroyButton != null)
            {
                // Remove any existing listeners first
                destroyButton.onClick.RemoveAllListeners();
                destroyButton.onClick.AddListener(DestroyCurrentFarm);
                Debug.Log("Destroy button listener setup");
            }
            else
            {
                Debug.LogWarning("Destroy button reference missing in FarmBuildingPanel");
            }
            
            // Ensure panel is hidden at start
            gameObject.SetActive(false);
        }
          private void Update()
        {
            // Update UI elements only when panel is active and a farm is selected
            if (gameObject.activeInHierarchy && currentFarm != null)
            {
                UpdateProgressBars();
                
                // Update UI every second - use a more efficient approach if needed
                if (Time.frameCount % 30 == 0) // ~every half second at 60fps
                {
                    // Update other farm stats that might change
                    if (storageText != null)
                        storageText.text = $"Storage: {currentFarm.GetStoredResources()}/{currentFarm.GetMaxStoredResources()}";
                        
                    // Update button interactability based on current state
                    if (harvestButton != null)
                        harvestButton.interactable = currentFarm.GetStoredResources() > 0;
                        
                    if (upgradeButton != null)
                        upgradeButton.interactable = currentFarm.CanUpgrade();
                }
            }
        }
          /// <summary>
        /// Membuka panel untuk bangunan farm tertentu
        /// </summary>
        /// <param name="farm">Farm building untuk ditampilkan</param>
        public void OpenPanel(FarmBuilding farm)
        {
            if (farm == null)
            {
                Debug.LogWarning("Cannot open FarmBuildingPanel: farm is null");
                return;
            }
                
            currentFarm = farm;
            
            // Update semua UI elements
            UpdateUIElements();
            
            // Show panel and ensure parent canvas is active
            gameObject.SetActive(true);
            
            // Log untuk debugging
            Debug.Log($"FarmBuildingPanel opened for {farm.GetBuildingName()} (Level {farm.GetCurrentLevel()})");
        }
        
        /// <summary>
        /// Memperbarui semua elemen UI dengan data dari farm saat ini
        /// </summary>
        private void UpdateUIElements()
        {
            if (currentFarm == null)
                return;
                
            // Basic info
            if (buildingNameText != null)
                buildingNameText.text = "Farm";
                
            if (levelText != null)
                levelText.text = $"Level {currentFarm.GetCurrentLevel()}/{currentFarm.GetMaxLevel()}";
                
            if (storageText != null)
                storageText.text = $"Storage: {currentFarm.GetStoredResources()}/{currentFarm.GetMaxStoredResources()}";
                
            if (productionRateText != null)
                productionRateText.text = $"Production: {currentFarm.GetProductionAmount()} wood every {currentFarm.GetProductionInterval().ToString("F1")}s";
                
            if (healthText != null)
                healthText.text = $"Health: {currentFarm.GetHealth()}/{currentFarm.GetMaxHealth()}";
                
            // Upgrade panel info
            if (currentFarm.GetCurrentLevel() < currentFarm.GetMaxLevel())
            {
                int woodCost, stoneCost, ironCost;
                currentFarm.GetUpgradeCost(out woodCost, out stoneCost, out ironCost);
                
                if (upgradeCostWoodText != null)
                    upgradeCostWoodText.text = woodCost.ToString();
                    
                if (upgradeCostStoneText != null)
                    upgradeCostStoneText.text = stoneCost.ToString();
                    
                if (upgradeCostIronText != null)
                    upgradeCostIronText.text = ironCost.ToString();
                    
                if (upgradeLevelText != null)
                    upgradeLevelText.text = $"Upgrade to Level {currentFarm.GetCurrentLevel() + 1}";
                    
                // Enable or disable upgrade button based on resources
                if (upgradeButton != null)
                    upgradeButton.interactable = currentFarm.CanUpgrade();
                    
                // Show upgrade panel
                if (upgradePanel != null)
                    upgradePanel.SetActive(true);
            }
            else
            {
                // Hide upgrade panel if max level
                if (upgradePanel != null)
                    upgradePanel.SetActive(false);
            }
            
            // Progress bars initial update
            UpdateProgressBars();
            
            // Enable/disable harvest button based on storage
            if (harvestButton != null)
                harvestButton.interactable = currentFarm.GetStoredResources() > 0;
        }
        
        /// <summary>
        /// Update progress bars untuk produksi dan storage
        /// </summary>
        private void UpdateProgressBars()
        {
            if (currentFarm == null)
                return;
                
            // Update storage progress
            if (storageProgressBar != null)
                storageProgressBar.value = currentFarm.GetStoragePercentage();
                
            // Update production progress
            if (productionProgressBar != null)
                productionProgressBar.value = currentFarm.GetProductionPercentage();
                
            // Update storage text dynamically
            if (storageText != null)
                storageText.text = $"Storage: {currentFarm.GetStoredResources()}/{currentFarm.GetMaxStoredResources()}";
                
            // Update harvest button interactability
            if (harvestButton != null)
                harvestButton.interactable = currentFarm.GetStoredResources() > 0;
        }
        
        /// <summary>
        /// Upgrade farm yang sedang dipilih
        /// </summary>
        private void UpgradeCurrentFarm()
        {
            if (currentFarm == null)
                return;
                
            // Attempt to upgrade
            currentFarm.Upgrade();
            
            // Update UI after upgrade
            UpdateUIElements();
        }
        
        /// <summary>
        /// Panen resource dari farm yang sedang dipilih
        /// </summary>
        private void HarvestCurrentFarm()
        {
            if (currentFarm == null)
                return;
                
            // Harvest all resources
            currentFarm.HarvestResources();
            
            // Update UI after harvesting
            UpdateUIElements();
        }
          /// <summary>
        /// Tutup panel
        /// </summary>
        private void ClosePanel()
        {
            // Log untuk debugging
            Debug.Log("Closing FarmBuildingPanel");
            
            // Reset state
            gameObject.SetActive(false);
            currentFarm = null;
        }
        
        /// <summary>
        /// Tutup panel - versi publik untuk dipanggil dari luar
        /// </summary>
        public void CloseButtonPressed()
        {
            ClosePanel();
        }
          [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private Button confirmDestroyButton;
        [SerializeField] private Button cancelDestroyButton;
        [SerializeField] private TextMeshProUGUI confirmationText;
        
        /// <summary>
        /// Hancurkan farm yang sedang dipilih
        /// </summary>
        private void DestroyCurrentFarm()
        {
            if (currentFarm == null)
                return;

            // Tampilkan dialog konfirmasi jika tersedia
            if (confirmationDialog != null)
            {
                // Atur pesan konfirmasi
                if (confirmationText != null)
                {
                    confirmationText.text = $"Are you sure you want to destroy this {currentFarm.GetBuildingName()} (Level {currentFarm.GetCurrentLevel()})?\n\nThis action cannot be undone!";
                }
                
                // Atur tombol konfirmasi
                if (confirmDestroyButton != null)
                {
                    confirmDestroyButton.onClick.RemoveAllListeners();
                    confirmDestroyButton.onClick.AddListener(ConfirmDestroy);
                }
                
                // Atur tombol batal
                if (cancelDestroyButton != null)
                {
                    cancelDestroyButton.onClick.RemoveAllListeners();
                    cancelDestroyButton.onClick.AddListener(HideConfirmationDialog);
                }
                
                // Tampilkan dialog
                confirmationDialog.SetActive(true);
            }
            else
            {
                // Jika tidak ada dialog konfirmasi, langsung hancurkan
                ConfirmDestroy();
            }
        }
        
        /// <summary>
        /// Sembunyikan dialog konfirmasi
        /// </summary>
        private void HideConfirmationDialog()
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }
        }
        
        /// <summary>
        /// Konfirmasi penghancuran farm
        /// </summary>
        private void ConfirmDestroy()
        {
            if (currentFarm == null)
                return;
                
            // Sembunyikan dialog konfirmasi
            HideConfirmationDialog();
            
            // Hancurkan bangunan
            Debug.Log($"Destroying farm: {currentFarm.GetBuildingName()}");
            
            // Panggil metode DestroyBuilding dari Building base class
            currentFarm.DestroyBuilding();
            
            // Tutup panel
            ClosePanel();
        }
    }
}
