using UnityEngine;
using KingdomClash.UI;

namespace KingdomClash
{
    /// <summary>
    /// Kelas dasar untuk semua bangunan dalam game
    /// </summary>
    public class Building : MonoBehaviour
    {
        [Header("Properti Bangunan")]
        [SerializeField] private string buildingName;
        [SerializeField] private string buildingDescription;
        [SerializeField] private int health = 100;
        [SerializeField] private int maxHealth = 100;
        
        [Header("Produksi Resource")]
        [SerializeField] private bool producesResources = false;
        [SerializeField] private string resourceType = ""; // "wood", "stone", "iron", "food"
        [SerializeField] private int productionAmount = 0;
        [SerializeField] private float productionInterval = 60f; // dalam detik
        
        [Header("References")]
        [SerializeField] private BuildingHealthBar healthBar;
          // Tidak perlu lagi static reference karena kita akan menggunakan BuildingPanel.Instance
        
        // References
        private float lastProductionTime = 0f;        private void Start()
        {
            lastProductionTime = Time.time;
            
            // Buat health bar jika tidak ada
            if (healthBar == null)
            {
                // Cari di child
                healthBar = GetComponentInChildren<BuildingHealthBar>();
                
                if (healthBar != null)
                {
                    // Setup health bar dengan referensi ke bangunan ini
                    healthBar.SetTargetBuilding(this);
                }
            }
        }        private void OnMouseDown()
        {
            // Gunakan BuildingPanel singleton instance
            if (BuildingPanel.Instance != null)
            {
                BuildingPanel.Instance.ShowBuildingInfo(this);
            }
            
            // Tampilkan health bar ketika bangunan diklik
            if (healthBar != null)
            {
                healthBar.ShowTemporary();
            }
        }
        private void Update()
        {
            // Produksi resource jika waktunya
            if (producesResources && Time.time - lastProductionTime >= productionInterval)
            {
                ProduceResources();
                lastProductionTime = Time.time;
            }
        }
        
        /// <summary>
        /// Produksi resource jika bangunan ini menghasilkan resource
        /// </summary>
        private void ProduceResources()
        {
            if (!producesResources || string.IsNullOrEmpty(resourceType) || productionAmount <= 0)
                return;
                
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateResource(resourceType, productionAmount);
                Debug.Log($"{buildingName} menghasilkan {productionAmount} {resourceType}");
            }
        }
          /// <summary>
        /// Menerima damage saat bangunan diserang
        /// </summary>
        public void TakeDamage(int amount)
        {
            health -= amount;
            
            // Pastikan health tidak kurang dari 0
            health = Mathf.Max(0, health);
            
            // Update health bar
            if (healthBar != null)
            {
                healthBar.ShowTemporary();
            }
              // Update panel info jika terbuka
            if (BuildingPanel.Instance != null && BuildingPanel.Instance.gameObject.activeSelf)
            {
                BuildingPanel.Instance.UpdateInfo();
            }
            
            if (health <= 0)
            {
                DestroyBuilding();
            }
        }
        
        /// <summary>
        /// Menghancurkan bangunan
        /// </summary>
        public void DestroyBuilding()
        {
            // Efek penghancuran bisa ditambahkan di sini
              // Tutup panel info jika terbuka
            if (BuildingPanel.Instance != null && BuildingPanel.Instance.gameObject.activeSelf)
            {
                BuildingPanel.Instance.ClosePanel();
            }
            
            // Hapus bangunan
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Mendapatkan nama bangunan
        /// </summary>
        public string GetBuildingName()
        {
            return buildingName;
        }
        
        /// <summary>
        /// Mendapatkan health bangunan saat ini
        /// </summary>
        public int GetHealth()
        {
            return health;
        }
          /// <summary>
        /// Mendapatkan health maksimum bangunan
        /// </summary>
        public int GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// Mendapatkan deskripsi bangunan
        /// </summary>
        public string GetBuildingDescription()
        {
            return buildingDescription;
        }
    }
}
