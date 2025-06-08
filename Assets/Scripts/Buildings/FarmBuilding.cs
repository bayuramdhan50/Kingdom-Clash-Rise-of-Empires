using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Farm building yang dapat menghasilkan resource dan dapat diupgrade
    /// </summary>
    public class FarmBuilding : Building
    {
        [Header("Farm Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int maxLevel = 3;
        
        [Header("Upgrade Costs")]
        [SerializeField] private int[] upgradeCostWood;
        [SerializeField] private int[] upgradeCostStone;
        [SerializeField] private int[] upgradeCostIron;
        
        [Header("Level Stats")]
        [SerializeField] private int[] levelHealthBonus; // Bonus health per level
        [SerializeField] private int[] levelProductionRate; // Resource per production cycle
        [SerializeField] private float[] levelProductionSpeed; // Seconds per production cycle
        [SerializeField] private int[] levelStorageCapacity; // Maksimum resource yang dapat disimpan
        
        // Resource storage untuk farm
        private int storedResources = 0;
        private int maxStoredResources = 100; // Default, akan diupdate berdasarkan level
        
        [Header("UI Elements")]
        [SerializeField] private GameObject upgradeButtonPrefab;
        [SerializeField] private GameObject resourceIndicator;
        
        private void Awake()
        {
            // Pastikan array memiliki panjang yang sesuai dengan maxLevel
            EnsureArrayLengths();
            
            // Setup bangunan berdasarkan level saat ini
            UpdateBuildingStats();
        }
        
        /// <summary>
        /// Memastikan array upgrade dan stats memiliki panjang yang tepat
        /// </summary>
        private void EnsureArrayLengths()
        {
            // Jika array belum diinisialisasi atau ukurannya tidak sesuai, inisialisasi dengan default value
            if (upgradeCostWood == null || upgradeCostWood.Length < maxLevel - 1)
            {
                upgradeCostWood = new int[maxLevel - 1];
                for (int i = 0; i < maxLevel - 1; i++)
                    upgradeCostWood[i] = 100 * (i + 1);
            }
            
            if (upgradeCostStone == null || upgradeCostStone.Length < maxLevel - 1)
            {
                upgradeCostStone = new int[maxLevel - 1];
                for (int i = 0; i < maxLevel - 1; i++)
                    upgradeCostStone[i] = 50 * (i + 1);
            }
            
            if (upgradeCostIron == null || upgradeCostIron.Length < maxLevel - 1)
            {
                upgradeCostIron = new int[maxLevel - 1];
                for (int i = 0; i < maxLevel - 1; i++)
                    upgradeCostIron[i] = 25 * (i + 1);
            }
            
            if (levelHealthBonus == null || levelHealthBonus.Length < maxLevel)
            {
                levelHealthBonus = new int[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                    levelHealthBonus[i] = 50 * (i + 1);
            }
            
            if (levelProductionRate == null || levelProductionRate.Length < maxLevel)
            {
                levelProductionRate = new int[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                    levelProductionRate[i] = 10 * (i + 1);
            }
            
            if (levelProductionSpeed == null || levelProductionSpeed.Length < maxLevel)
            {
                levelProductionSpeed = new float[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                    levelProductionSpeed[i] = 60f / (i + 1); // Semakin tinggi level, semakin cepat produksi
            }
            
            if (levelStorageCapacity == null || levelStorageCapacity.Length < maxLevel)
            {
                levelStorageCapacity = new int[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                    levelStorageCapacity[i] = 100 * (i + 1);
            }
        }
        
        /// <summary>
        /// Update stats bangunan berdasarkan level saat ini
        /// </summary>
        private void UpdateBuildingStats()
        {
            int levelIndex = currentLevel - 1; // Array 0-based, level 1-based
            
            // Update health
            int newMaxHealth = 100 + levelHealthBonus[levelIndex]; // Base health + level bonus
            SetMaxHealth(newMaxHealth);
            SetHealth(newMaxHealth); // Heal to full when upgrading
            
            // Update production rate
            SetProductionAmount(levelProductionRate[levelIndex]);
            
            // Update production interval
            SetProductionInterval(levelProductionSpeed[levelIndex]);
            
            // Update storage capacity
            maxStoredResources = levelStorageCapacity[levelIndex];
            
            // Update visual (optional)
            UpdateVisual();
        }
        
        /// <summary>
        /// Perbarui tampilan visual farm berdasarkan level
        /// </summary>
        private void UpdateVisual()
        {
            // TODO: Implementasi untuk mengubah model 3D atau efek berdasarkan level
            // Contoh:
            // - Level 1: Model farm kecil
            // - Level 2: Model farm medium
            // - Level 3: Model farm besar
            
            // Update nama bangunan untuk menunjukkan level
            SetBuildingName($"Farm Level {currentLevel}");
        }
        
        /// <summary>
        /// Cek apakah bangunan bisa diupgrade
        /// </summary>
        public bool CanUpgrade()
        {
            // Cek apakah sudah mencapai level maksimum
            if (currentLevel >= maxLevel)
                return false;
                
            // Cek apakah resource cukup
            int levelIndex = currentLevel - 1;
            
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
                return false;
            
            Resources playerResources = GameManager.Instance.GetCurrentGameData().resources;
            
            return playerResources.wood >= upgradeCostWood[levelIndex] &&
                   playerResources.stone >= upgradeCostStone[levelIndex] &&
                   playerResources.iron >= upgradeCostIron[levelIndex];
        }
        
        /// <summary>
        /// Upgrade farm ke level berikutnya
        /// </summary>
        public void Upgrade()
        {
            if (!CanUpgrade())
                return;
                
            // Ambil biaya upgrade
            int levelIndex = currentLevel - 1;
            
            // Kurangi resource player
            GameManager.Instance.UpdateResource("wood", -upgradeCostWood[levelIndex]);
            GameManager.Instance.UpdateResource("stone", -upgradeCostStone[levelIndex]);
            GameManager.Instance.UpdateResource("iron", -upgradeCostIron[levelIndex]);
            
            // Naikkan level
            currentLevel++;
            
            // Update stats
            UpdateBuildingStats();
            
            // Efek upgrade (optional)
            PlayUpgradeEffect();
            
            Debug.Log($"Farm upgraded to level {currentLevel}!");
        }
        
        /// <summary>
        /// Menampilkan efek visual saat upgrade
        /// </summary>
        private void PlayUpgradeEffect()
        {
            // TODO: Implementasi efek upgrade seperti particle system
        }
          /// <summary>
        /// Custom produksi resource - menyimpan ke storage farm
        /// </summary>
        public void ProduceResourceToStorage()
        {
            // Cek apakah sudah waktunya untuk produksi
            float timeSinceLastProduction = Time.time - GetLastProductionTime();
            if (timeSinceLastProduction < GetProductionInterval())
                return;
                
            // Cek apakah storage masih tersedia
            if (storedResources >= maxStoredResources)
            {
                // Storage penuh, tidak bisa produksi
                return;
            }
            
            // Produksi resource ke storage farm
            int productionAmount = GetProductionAmount();
            storedResources = Mathf.Min(storedResources + productionAmount, maxStoredResources);
            
            // Reset timer produksi
            ResetProductionTimer();
            
            // Update UI indicator jika ada
            UpdateResourceIndicator();
            
            Debug.Log($"Farm produced {productionAmount} wood. Storage: {storedResources}/{maxStoredResources}");
        }
        
        /// <summary>
        /// Update indikator resource di UI
        /// </summary>
        private void UpdateResourceIndicator()
        {
            if (resourceIndicator != null)
            {
                // Implementasi untuk menampilkan jumlah resource tersimpan
            }
        }
        
        /// <summary>
        /// Ambil resource dari farm
        /// </summary>
        /// <param name="amount">Jumlah yang ingin diambil</param>
        /// <returns>Jumlah yang berhasil diambil</returns>
        public int HarvestResources(int amount)
        {
            // Cek berapa yang bisa diambil
            int harvestAmount = Mathf.Min(amount, storedResources);
            
            // Kurangi storage
            storedResources -= harvestAmount;
            
            // Update UI
            UpdateResourceIndicator();
            
            // Tambahkan ke resource global player
            if (GameManager.Instance != null && harvestAmount > 0)
            {
                GameManager.Instance.UpdateResource("wood", harvestAmount);
                Debug.Log($"Harvested {harvestAmount} wood from farm");
            }
            
            return harvestAmount;
        }
        
        /// <summary>
        /// Worker dapat memanggil ini untuk "panen" dari farm
        /// </summary>
        public int HarvestResources()
        {
            // Default harvest all
            return HarvestResources(storedResources);
        }
        
        /// <summary>
        /// Dapatkan informasi level saat ini
        /// </summary>
        public int GetCurrentLevel()
        {
            return currentLevel;
        }
        
        /// <summary>
        /// Dapatkan informasi level maksimum
        /// </summary>
        public int GetMaxLevel()
        {
            return maxLevel;
        }
        
        /// <summary>
        /// Dapatkan jumlah resource yang tersimpan
        /// </summary>
        public int GetStoredResources()
        {
            return storedResources;
        }
        
        /// <summary>
        /// Dapatkan kapasitas storage
        /// </summary>
        public int GetMaxStoredResources()
        {
            return maxStoredResources;
        }
        
        /// <summary>
        /// Dapatkan info biaya upgrade
        /// </summary>
        public void GetUpgradeCost(out int wood, out int stone, out int iron)
        {
            wood = stone = iron = 0;
            
            if (currentLevel < maxLevel)
            {
                int levelIndex = currentLevel - 1;
                wood = upgradeCostWood[levelIndex];
                stone = upgradeCostStone[levelIndex];
                iron = upgradeCostIron[levelIndex];
            }
        }
        
        /// <summary>
        /// Dapatkan persentase produksi
        /// </summary>
        public float GetProductionPercentage()
        {
            float timeSinceLastProduction = Time.time - GetLastProductionTime();
            return Mathf.Clamp01(timeSinceLastProduction / GetProductionInterval());
        }
        
        /// <summary>
        /// Dapatkan persentase storage yang terisi
        /// </summary>
        public float GetStoragePercentage()
        {
            return (float)storedResources / maxStoredResources;
        }

        private void Update()
        {
            // Cek apakah bangunan memproduksi resource
            if (GetIsProducer())
            {
                // Coba produksi resource ke storage internal
                ProduceResourceToStorage();
            }
        }
        
        /// <summary>
        /// Mendapatkan waktu produksi terakhir
        /// </summary>
        private float GetLastProductionTime()
        {
            // Reflection untuk mengakses private field di kelas induk
            var field = typeof(Building).GetField("lastProductionTime", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                return (float)field.GetValue(this);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Reset timer produksi
        /// </summary>
        private void ResetProductionTimer()
        {
            // Reflection untuk mengakses private field di kelas induk
            var field = typeof(Building).GetField("lastProductionTime", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(this, Time.time);
            }
        }
        
        /// <summary>
        /// Mendapatkan status produksi resource
        /// </summary>
        private bool GetIsProducer()
        {
            // Reflection untuk mengakses private field di kelas induk
            var field = typeof(Building).GetField("producesResources", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                return (bool)field.GetValue(this);
            }
            
            return false;
        }
        
        /// <summary>
        /// Mendapatkan interval produksi resource
        /// </summary>
        public float GetProductionInterval()
        {
            var property = typeof(Building).GetMethod("GetProductionInterval", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
                
            if (property != null)
            {
                return (float)property.Invoke(this, null);
            }
            
            // Default value
            return 60f;
        }
        
        /// <summary>
        /// Mendapatkan jumlah produksi resource
        /// </summary>
        public int GetProductionAmount()
        {
            var field = typeof(Building).GetField("productionAmount", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                return (int)field.GetValue(this);
            }
            
            // Default value
            return 10;
        }
        
        /// <summary>
        /// Set jumlah produksi resource
        /// </summary>
        private void SetProductionAmount(int amount)
        {
            var field = typeof(Building).GetField("productionAmount", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(this, amount);
            }
        }
        
        /// <summary>
        /// Set interval produksi resource
        /// </summary>
        private void SetProductionInterval(float interval)
        {
            var field = typeof(Building).GetField("productionInterval", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(this, interval);
            }
        }
        
        /// <summary>
        /// Set nama bangunan (untuk saat upgrade)
        /// </summary>
        private void SetBuildingName(string name)
        {
            // Reflection untuk mengakses private field di kelas induk
            var field = typeof(Building).GetField("buildingName", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(this, name);
            }
        }
    }
}
