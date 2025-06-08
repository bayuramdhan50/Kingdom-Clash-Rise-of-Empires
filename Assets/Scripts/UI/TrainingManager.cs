using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manager untuk menangani proses pelatihan yang berjalan di background
    /// </summary>
    public class TrainingManager : MonoBehaviour
    {
        // Singleton instance
        public static TrainingManager Instance { get; private set; }
        
        // Struktur data untuk menyimpan informasi training
        [System.Serializable]
        public class TrainingInfo
        {
            public string buildingName;
            public string unitType;
            public float progress;
            public float trainingTime;
            public int count;
            
            public TrainingInfo(string name, string unitType, float time, int unitCount)
            {
                buildingName = name;
                this.unitType = unitType;
                trainingTime = time;
                count = unitCount;
                progress = 0f;
            }
        }
        
        // Dictionary untuk menyimpan antrian training berdasarkan nama bangunan
        private Dictionary<string, TrainingInfo> trainingQueues = new Dictionary<string, TrainingInfo>();
        
        // Unit prefabs (untuk instansiasi unit setelah training selesai)
        [Header("Unit Prefabs")]
        [SerializeField] private GameObject infantryPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject cavalryPrefab;
        
        // Delegate untuk event training
        public delegate void TrainingProgressHandler(string buildingName, float progress);
        public delegate void TrainingCompletedHandler(string buildingName);
        
        // Event yang bisa disubscribe oleh panel
        public event TrainingProgressHandler OnTrainingProgressUpdated;
        public event TrainingCompletedHandler OnTrainingCompleted;
        
        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Pastikan objek ini tidak dihancurkan saat pindah scene
            DontDestroyOnLoad(gameObject);
            
            // Inisialisasi dictionary
            trainingQueues = new Dictionary<string, TrainingInfo>();
            
            Debug.Log("TrainingManager initialized and set to persist between scenes");
        }
        
        private void Start()
        {
            // Load training data saat game dimulai (jika melanjutkan permainan yang sudah ada)
            LoadTrainingStateIfNeeded();
        }
        
        /// <summary>
        /// Load training state jika game sedang dilanjutkan dari save
        /// </summary>
        private void LoadTrainingStateIfNeeded()
        {
            // Check GameManager tersedia dan game sedang dilanjutkan dari save
            if (GameManager.Instance != null && 
                GameManager.Instance.IsContinuing && 
                GameManager.Instance.GetCurrentGameData() != null)
            {
                LoadTrainingState();
            }
        }
          private void Update()
        {
            // Update progress untuk semua training yang aktif
            UpdateAllTrainingProgress();
        }
        
        /// <summary>
        /// Update progress untuk semua proses pelatihan yang aktif
        /// </summary>
        private void UpdateAllTrainingProgress()
        {
            if (trainingQueues == null || trainingQueues.Count == 0)
                return;
                
            // Buat daftar bangunan yang selesai dilatih untuk dihapus setelah iterasi
            List<string> completedBuildings = new List<string>();
            
            foreach (KeyValuePair<string, TrainingInfo> entry in trainingQueues)
            {
                string buildingName = entry.Key;
                TrainingInfo info = entry.Value;
                
                // Lewati jika tidak ada unit dalam antrian
                if (info.count <= 0)
                    continue;
                
                // Update progress
                info.progress += Time.deltaTime;
                
                // Normalkan progress untuk event (0-1)
                float normalizedProgress = Mathf.Clamp01(info.progress / info.trainingTime);
                
                // Kirim event progress update
                OnTrainingProgressUpdated?.Invoke(buildingName, normalizedProgress);
                
                // Cek apakah training selesai
                if (info.progress >= info.trainingTime)
                {
                    // Unit selesai dilatih
                    info.count--;
                    
                    // Kirim event training selesai
                    OnTrainingCompleted?.Invoke(buildingName);
                    
                    // Buat unit baru yang sudah selesai ditraining
                    SpawnCompletedUnit(buildingName, info.unitType);
                    
                    // Reset progress untuk unit berikutnya atau tandai untuk dihapus
                    if (info.count <= 0)
                    {
                        completedBuildings.Add(buildingName);
                    }
                    else
                    {
                        info.progress = 0f;
                    }
                    
                    // Simpan status training setelah perubahan
                    SaveTrainingState();
                }
            }
            
            // Hapus bangunan yang sudah selesai semua antrian pelatihannya
            foreach (string building in completedBuildings)
            {
                trainingQueues.Remove(building);
            }
        }
          /// <summary>
        /// Membuat unit baru yang telah selesai dilatih
        /// </summary>
        private void SpawnCompletedUnit(string buildingName, string unitType)
        {
            // Cari building object berdasarkan nama
            Building building = FindBuildingByName(buildingName);
            
            if (building == null)
            {
                Debug.LogWarning($"Building '{buildingName}' not found for unit spawn. Will try again when available.");
                return; // Akan coba lagi pada update berikutnya
            }
            
            // Tentukan spawn position (di depan bangunan)
            // Menggunakan right vector karena bangunan dirotasi -90 derajat pada sumbu X
            // Ini berarti 'forward' bangunan sebenarnya mengarah ke right dalam world space
            Vector3 spawnPos = building.transform.position + building.transform.right * 3f;
              // Set nilai default untuk unit stats
            GameObject prefabToSpawn = null;
            int health = 100;
            int maxHealth = 100; // Menambahkan variabel maxHealth
            int attack = 10;
            int defense = 10;
            
            // Pilih prefab dan stats berdasarkan tipe unit
            switch (unitType.ToLower())
            {                case "infantry":
                    prefabToSpawn = infantryPrefab;
                    health = 120;
                    maxHealth = 120;
                    attack = 15;
                    defense = 10;
                    break;
                    
                case "archer":
                    prefabToSpawn = archerPrefab;
                    health = 80;
                    maxHealth = 80;
                    attack = 20;
                    defense = 5;
                    break;
                    
                case "cavalry":
                    prefabToSpawn = cavalryPrefab;
                    health = 150;
                    maxHealth = 150;
                    attack = 25;
                    defense = 15;
                    break;
                    
                default:
                    prefabToSpawn = infantryPrefab;
                    Debug.LogWarning($"Unknown unit type: {unitType}, using infantry as default");
                    break;
            }
            
            // Buat UnitData untuk data game
            UnitData unitData = new UnitData(unitType, health, health, attack, defense, spawnPos);
              // Instantiate unit jika prefab tersedia
            if (prefabToSpawn != null)
            {
                GameObject unitObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                
                // Set up unit stats if there's a Unit component
                Characters.Unit unitComponent = unitObj.GetComponent<Characters.Unit>();
                if (unitComponent != null)
                {
                    unitComponent.SetHealth(health);
                    unitComponent.SetMaxHealth(maxHealth);
                    unitComponent.SetAttack(attack);
                    unitComponent.SetDefense(defense);
                }
                else
                {
                    // If no Unit component found, add one
                    unitComponent = unitObj.AddComponent<Characters.Unit>();
                    unitComponent.SetHealth(health);
                    unitComponent.SetMaxHealth(maxHealth);
                    unitComponent.SetAttack(attack);
                    unitComponent.SetDefense(defense);
                }
                
                // Unit spawned successfully
            }
            else
            {
                // Prefab not assigned
            }
            
            // Tambahkan unit data ke game data (terlepas dari apakah instantiate berhasil)
            if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameData() != null)
            {
                GameManager.Instance.GetCurrentGameData().units.Add(unitData);
            }
        }
          /// <summary>
        /// Mencari building berdasarkan nama
        /// </summary>
        private Building FindBuildingByName(string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
                return null;
                
            // Cari semua bangunan di scene
            Building[] allBuildings = FindObjectsOfType<Building>();
            
            // Cari bangunan dengan nama yang cocok
            foreach (Building building in allBuildings)
            {
                if (building != null && building.GetBuildingName() == buildingName)
                {
                    return building;
                }
            }
            
            return null;
        }
          /// <summary>
        /// Mulai atau tambah unit ke antrian pelatihan
        /// </summary>
        public void QueueTraining(string buildingName, string unitType, float trainingTime)
        {
            // Validasi input
            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(unitType) || trainingTime <= 0)
            {
                Debug.LogError("Invalid training parameters: need valid building name, unit type, and positive training time");
                return;
            }
            
            // Cek apakah sudah ada antrian untuk bangunan ini
            if (trainingQueues.TryGetValue(buildingName, out TrainingInfo info))
            {
                // Jika sudah ada, tambahkan jumlah unit
                info.count++;
                Debug.Log($"Added {unitType} to training queue for {buildingName}. Queue count: {info.count}");
            }
            else
            {
                // Jika belum ada, buat antrian baru
                trainingQueues.Add(buildingName, new TrainingInfo(buildingName, unitType, trainingTime, 1));
                Debug.Log($"Started new training queue for {buildingName} with {unitType}");
            }
            
            // Simpan status training setelah perubahan
            SaveTrainingState();
        }
        
        /// <summary>
        /// Mendapatkan jumlah unit dalam antrian pelatihan
        /// </summary>
        public int GetQueueCount(string buildingName)
        {
            // Validasi input
            if (string.IsNullOrEmpty(buildingName) || trainingQueues == null)
                return 0;
                
            // Return jumlah unit dalam antrian jika ada
            return trainingQueues.TryGetValue(buildingName, out TrainingInfo info) ? info.count : 0;
        }
        
        /// <summary>
        /// Mendapatkan progress pelatihan (0-1)
        /// </summary>
        public float GetTrainingProgress(string buildingName)
        {
            // Validasi input
            if (string.IsNullOrEmpty(buildingName) || trainingQueues == null)
                return 0f;
                
            // Return nilai progress normalisasi (0-1) jika ada
            if (trainingQueues.TryGetValue(buildingName, out TrainingInfo info) && info.trainingTime > 0)
            {
                return Mathf.Clamp01(info.progress / info.trainingTime);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Mendapatkan tipe unit yang sedang dilatih
        /// </summary>
        public string GetTrainingUnitType(string buildingName)
        {
            // Validasi input
            if (string.IsNullOrEmpty(buildingName) || trainingQueues == null)
                return string.Empty;
                
            // Return tipe unit jika ada
            return trainingQueues.TryGetValue(buildingName, out TrainingInfo info) ? info.unitType : string.Empty;
        }
        
        /// <summary>
        /// Memeriksa apakah ada pelatihan aktif untuk bangunan tertentu
        /// </summary>
        public bool IsTraining(string buildingName)
        {
            // Validasi input
            if (string.IsNullOrEmpty(buildingName) || trainingQueues == null)
                return false;
                
            // Cek apakah ada entry di dictionary dan unit dalam antrian > 0
            return trainingQueues.TryGetValue(buildingName, out TrainingInfo info) && info.count > 0;
        }
          /// <summary>
        /// Menyimpan status training saat ini ke game data
        /// </summary>
        public void SaveTrainingState()
        {
            // Validasi GameManager dan GameData tersedia
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
            {
                Debug.LogWarning("Cannot save training state: GameManager or GameData not available");
                return;
            }
                
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            
            // Inisialisasi list jika belum ada
            if (gameData.trainingProcesses == null)
            {
                gameData.trainingProcesses = new List<TrainingData>();
            }
            else
            {
                gameData.trainingProcesses.Clear();
            }
            
            // Tambahkan data training terkini
            foreach (KeyValuePair<string, TrainingInfo> entry in trainingQueues)
            {
                TrainingInfo info = entry.Value;
                
                // Simpan hanya yang jumlah unitnya > 0
                if (info.count > 0)
                {
                    TrainingData data = new TrainingData(
                        info.buildingName,
                        info.unitType,
                        info.count,
                        info.progress,
                        info.trainingTime
                    );
                    gameData.trainingProcesses.Add(data);
                }
            }
            
            Debug.Log($"Saved {gameData.trainingProcesses.Count} training processes to game data");
        }
        
        /// <summary>
        /// Memuat status training dari game data
        /// </summary>
        public void LoadTrainingState()
        {
            // Validasi GameManager dan GameData tersedia
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
            {
                Debug.LogWarning("Cannot load training state: GameManager or GameData not available");
                return;
            }
                
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            
            // Reset antrian yang ada
            trainingQueues.Clear();
            
            // Jika tidak ada data training, keluar
            if (gameData.trainingProcesses == null || gameData.trainingProcesses.Count == 0)
            {
                Debug.Log("No training processes to load from game data");
                return;
            }
                
            // Load semua training process
            foreach (TrainingData data in gameData.trainingProcesses)
            {
                // Tambahkan hanya jika jumlah unitnya > 0
                if (data.count > 0)
                {
                    // Buat training info baru
                    TrainingInfo info = new TrainingInfo(
                        data.buildingName,
                        data.unitType,
                        data.trainingTime,
                        data.count
                    );
                    
                    // Restore progress
                    info.progress = data.progress;
                    
                    // Tambahkan ke dictionary
                    trainingQueues.Add(data.buildingName, info);
                }
            }
            
            // Training processes loaded
        }
        
        /// <summary>
        /// Menghapus semua antrian training (misalnya saat reset game)
        /// </summary>
        public void ClearAllTraining()
        {
            trainingQueues.Clear();
            // Training queues cleared
        }
        
        /// <summary>
        /// Gets all current training queues for manual saving
        /// </summary>
        /// <returns>Dictionary of building names to training info</returns>
        public Dictionary<string, TrainingInfo> GetAllTrainingQueues()
        {
            return new Dictionary<string, TrainingInfo>(trainingQueues);
        }
        
        /// <summary>
        /// Forces an immediate save of the training state
        /// </summary>
        public void ForceSaveTrainingState()
        {            SaveTrainingState();
        }
        
        /// <summary>
        /// Gets the dictionary of unit prefabs for loading saved units
        /// </summary>
        /// <returns>Dictionary mapping unit type to prefab</returns>
        public Dictionary<string, GameObject> GetUnitPrefabs()
        {
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
            
            // Add prefabs to dictionary if they exist
            if (infantryPrefab != null)
                prefabs.Add("infantry", infantryPrefab);
                
            if (archerPrefab != null)
                prefabs.Add("archer", archerPrefab);
                
            if (cavalryPrefab != null)
                prefabs.Add("cavalry", cavalryPrefab);
                
            return prefabs;
        }
    }
}
