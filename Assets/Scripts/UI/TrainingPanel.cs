using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace KingdomClash.UI
{
    /// <summary>
    /// Panel sederhana untuk pelatihan prajurit yang menggunakan TrainingManager untuk manajemen di background
    /// </summary>
    public class TrainingPanel : MonoBehaviour
    {
        // Singleton instance
        public static TrainingPanel Instance { get; private set; }        
        
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI unitNameText;
        
        [Header("Resource Cost")]
        [SerializeField] private TextMeshProUGUI foodCostText;
        [SerializeField] private TextMeshProUGUI woodCostText;
        [SerializeField] private TextMeshProUGUI ironCostText;
        
        [SerializeField] private TextMeshProUGUI queueText;
        [SerializeField] private Button trainButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        // Referensi ke bangunan yang sedang dipilih
        private Building currentBuilding;
        
        // Nilai-nilai untuk pelatihan
        private int foodCost;
        private int woodCost;
        private int ironCost;
        private string currentUnitType;
        private int maxTrainingCount = 5;
        private float trainingTime = 30f; // dalam detik
        
        private void Awake()
        {
            // Setup singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Setup tombol close
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
            
            // Setup tombol train
            if (trainButton != null)
            {
                trainButton.onClick.AddListener(StartTraining);
            }
            
            // Cek apakah TrainingManager ada
            if (TrainingManager.Instance == null)
            {
                GameObject managerObj = new GameObject("TrainingManager");
                managerObj.AddComponent<TrainingManager>();
            }
            
            // Subscribe ke event TrainingManager
            TrainingManager.Instance.OnTrainingProgressUpdated += HandleTrainingProgressUpdated;
            TrainingManager.Instance.OnTrainingCompleted += HandleTrainingCompleted;
            
            // Sembunyikan panel saat awal
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe dari event untuk mencegah memory leak
            if (TrainingManager.Instance != null)
            {
                TrainingManager.Instance.OnTrainingProgressUpdated -= HandleTrainingProgressUpdated;
                TrainingManager.Instance.OnTrainingCompleted -= HandleTrainingCompleted;
            }
        }

        private void Update()
        {
            // Update UI jika panel aktif dan ada bangunan yang dipilih
            if (gameObject.activeSelf && currentBuilding != null)
            {
                string buildingName = currentBuilding.GetBuildingName();
                
                // Update progress UI berdasarkan data dari TrainingManager
                if (TrainingManager.Instance.IsTraining(buildingName))
                {
                    float progress = TrainingManager.Instance.GetTrainingProgress(buildingName);
                    progressBar.value = progress;
                    progressText.text = $"{Mathf.Round(progress * 100)}%";
                    
                    // Update jumlah dalam antrian
                    int queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
                    queueText.text = $"Training: {queueCount}/{maxTrainingCount}";
                    
                    // Update status tombol train
                    UpdateTrainButtonState();
                }
            }
        }

        /// <summary>
        /// Menampilkan panel untuk bangunan tertentu
        /// </summary>
        public void ShowForBuilding(Building building)
        {
            currentBuilding = building;
            
            if (building == null) return;
            
            // Periksa apakah ini bangunan pemain atau musuh
            if (building.CompareTag("EnemyBuilding"))
            {
                Debug.LogWarning("Mencoba membuka TrainingPanel untuk bangunan musuh. Operasi dibatalkan.");
                return;
            }
            
            // Set nilai-nilai berdasarkan tipe bangunan
            currentUnitType = "Infantry";
            foodCost = 50;
            woodCost = 0;
            ironCost = 20;
            
            if (building.GetBuildingName().Contains("Barracks"))
            {
                currentUnitType = "Infantry";
                foodCost = 50;
                woodCost = 10;
                ironCost = 20;
            }
            else if (building.GetBuildingName().Contains("Archery"))
            {
                currentUnitType = "Archer";
                foodCost = 40;
                woodCost = 30;
                ironCost = 10;
            }
            else if (building.GetBuildingName().Contains("Stable"))
            {
                currentUnitType = "Cavalry";
                foodCost = 60;
                woodCost = 5;
                ironCost = 25;
            }
            
            // Update UI
            buildingNameText.text = building.GetBuildingName();
            unitNameText.text = currentUnitType;
            
            // Update resource costs
            foodCostText.text = foodCost.ToString();
            woodCostText.text = woodCost.ToString();
            ironCostText.text = ironCost.ToString();
            
            // Dapatkan data queue dari TrainingManager
            string buildingName = building.GetBuildingName();
            int queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
            queueText.text = $"Training: {queueCount}/{maxTrainingCount}";
            
            // Update progress bar berdasarkan status training saat ini
            if (TrainingManager.Instance.IsTraining(buildingName))
            {
                // Jika sedang training, tampilkan progress saat ini
                float progress = TrainingManager.Instance.GetTrainingProgress(buildingName);
                progressBar.value = progress;
                progressText.text = $"{Mathf.Round(progress * 100)}%";
            }
            else
            {
                // Reset progress
                progressBar.value = 0;
                progressText.text = "0%";
            }
            
            // Update tombol train
            UpdateTrainButtonState();
            
            // Tampilkan panel
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Memulai pelatihan prajurit baru
        /// </summary>
        private void StartTraining()
        {
            if (currentBuilding == null) return;
            
            string buildingName = currentBuilding.GetBuildingName();
            
            // Cek apakah sudah mencapai limit
            int queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
            if (queueCount >= maxTrainingCount)
            {
                // Queue penuh
                return;
            }
            
            // Cek resource
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            if (gameData == null) return;
            
            Resources resources = gameData.resources;
            
            if (resources.food < foodCost || 
                resources.wood < woodCost || 
                resources.iron < ironCost)
            {
                // Resource tidak cukup
                return;
            }
            
            // Kurangi resource
            GameManager.Instance.UpdateResource("food", -foodCost);
            GameManager.Instance.UpdateResource("wood", -woodCost);
            GameManager.Instance.UpdateResource("iron", -ironCost);
            
            // Update UI resource jika perlu
            ResourcePanel resourcePanel = FindObjectOfType<ResourcePanel>();
            if (resourcePanel != null)
            {
                resourcePanel.UpdateResourceDisplay();
            }
            
            // Tambahkan unit ke antrian menggunakan TrainingManager
            TrainingManager.Instance.QueueTraining(buildingName, currentUnitType, trainingTime);
            
            // Refresh data queue
            queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
            queueText.text = $"Training: {queueCount}/{maxTrainingCount}";
            
            // Update tombol train
            UpdateTrainButtonState();
        }
        
        /// <summary>
        /// Update status tombol train
        /// </summary>
        private void UpdateTrainButtonState()
        {
            if (trainButton == null || currentBuilding == null) return;
            
            string buildingName = currentBuilding.GetBuildingName();
            
            // Cek jumlah yang dilatih
            int queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
            if (queueCount >= maxTrainingCount)
            {
                trainButton.interactable = false;
                return;
            }
            
            // Cek resource
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            if (gameData == null)
            {
                trainButton.interactable = false;
                return;
            }
            Resources resources = gameData.resources;
            
            bool canAfford = resources.food >= foodCost && 
                           resources.wood >= woodCost && 
                           resources.iron >= ironCost;
            
            trainButton.interactable = canAfford;
            
            // Update warna teks resource berdasarkan ketersediaan
            foodCostText.color = resources.food >= foodCost ? Color.white : Color.red;
            woodCostText.color = resources.wood >= woodCost ? Color.white : Color.red;
            ironCostText.color = resources.iron >= ironCost ? Color.white : Color.red;
        }

        /// <summary>
        /// Menutup panel tapi tetap melatih
        /// </summary>
        public void ClosePanel()
        {
            // Panel ditutup tapi proses training tetap berjalan karena TrainingManager
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Memeriksa apakah panel sedang melatih prajurit
        /// </summary>
        /// <returns>True jika sedang melatih, false jika tidak</returns>
        public bool IsTraining()
        {
            if (currentBuilding == null) return false;
            return TrainingManager.Instance.IsTraining(currentBuilding.GetBuildingName());
        }
        
        /// <summary>
        /// Mendapatkan informasi training saat ini
        /// </summary>
        /// <returns>Jumlah unit yang sedang dilatih</returns>
        public int GetTrainingCount()
        {
            if (currentBuilding == null) return 0;
            return TrainingManager.Instance.GetQueueCount(currentBuilding.GetBuildingName());
        }
        
        /// <summary>
        /// Mendapatkan progress pelatihan saat ini (0-1)
        /// </summary>
        public float GetTrainingProgress()
        {
            if (currentBuilding == null) return 0f;
            return TrainingManager.Instance.GetTrainingProgress(currentBuilding.GetBuildingName());
        }

        /// <summary>
        /// Handler untuk event progress update dari TrainingManager
        /// </summary>
        private void HandleTrainingProgressUpdated(string buildingName, float progress)
        {
            // Hanya update UI jika panel aktif dan bangunan cocok
            if (gameObject.activeSelf && currentBuilding != null && 
                currentBuilding.GetBuildingName() == buildingName)
            {
                // Update progress UI
                progressBar.value = progress;
                progressText.text = $"{Mathf.Round(progress * 100)}%";
            }
        }
        
        /// <summary>
        /// Handler untuk event training completed dari TrainingManager
        /// </summary>
        private void HandleTrainingCompleted(string buildingName)
        {
            // Hanya update UI jika panel aktif dan bangunan cocok
            if (gameObject.activeSelf && currentBuilding != null && 
                currentBuilding.GetBuildingName() == buildingName)
            {
                // Update queue text
                int queueCount = TrainingManager.Instance.GetQueueCount(buildingName);
                queueText.text = $"Training: {queueCount}/{maxTrainingCount}";
                
                // Update tombol train
                UpdateTrainButtonState();
            }
        }
    }
}
