using UnityEngine;
using KingdomClash.UI;

namespace KingdomClash
{
    /// <summary>
    /// Kelas untuk bangunan pelatihan prajurit sederhana
    /// </summary>
    public class TrainingBuilding : MonoBehaviour
    {
        [SerializeField] private string buildingType; // "Barracks", "Archery", atau "Stable"
        
        private Building building;
        
        private void Awake()
        {
            building = GetComponent<Building>();
              // Dapatkan Building script dan tambahkan event handler kustom
            if (building != null)
            {
                // Tambahkan BuildingClickHandler untuk menggantikan perilaku klik default
                // BuildingClickHandler akan mencegah OnMouseDown dari Building.cs dijalankan
                BuildingClickHandler clickHandler = building.gameObject.AddComponent<BuildingClickHandler>();
                clickHandler.Initialize(building, OnBuildingClick);
            }
        }
        
        /// <summary>
        /// Handler kustom untuk klik pada bangunan
        /// </summary>
        private void OnBuildingClick(Building clickedBuilding)
        {
            // Pastikan building ada
            if (clickedBuilding == null) return;
            
            // Tampilkan panel pelatihan
            if (TrainingPanel.Instance != null)
            {
                TrainingPanel.Instance.ShowForBuilding(clickedBuilding);
            }
        }
    }
    
    /// <summary>
    /// Helper class untuk menangani klik pada bangunan dan mencegah event default
    /// </summary>
    public class BuildingClickHandler : MonoBehaviour
    {
        private Building targetBuilding;
        private System.Action<Building> clickCallback;
        
        public void Initialize(Building building, System.Action<Building> callback)
        {
            targetBuilding = building;
            clickCallback = callback;
        }
          private void OnMouseDown()
        {
            // Mencegah event OnMouseDown di Building class dipanggil dengan memanipulasi event order
            // Karena ini adalah component yang ditambahkan setelah Building, OnMouseDown di sini 
            // akan dipanggil sebelum OnMouseDown di Building
            if (clickCallback != null && targetBuilding != null)
            {
                clickCallback(targetBuilding);
                
                // Menampilkan health bar sebagaimana Building.OnMouseDown() lakukan
                var healthBar = targetBuilding.GetComponentInChildren<BuildingHealthBar>();
                if (healthBar != null)
                {
                    healthBar.ShowTemporary();
                }
                
                // Untuk mencegah OnMouseDown di Building dijalankan, kita bisa menggunakan event.Use()
                // di Unity 2019+, tapi di sini kita gunakan cara lain untuk mencegah event bubbling
                
                // Cara alternatif: menunda sebelum menutup BuildingPanel jika terbuka
                if (BuildingPanel.Instance != null && BuildingPanel.Instance.gameObject.activeSelf)
                {
                    BuildingPanel.Instance.ClosePanel();
                }
            }
        }
    }
}
