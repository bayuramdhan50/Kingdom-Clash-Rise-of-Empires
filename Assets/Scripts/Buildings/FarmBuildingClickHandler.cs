using UnityEngine;
using KingdomClash.UI; // Tambahkan namespace UI untuk BuildingHealthBar

namespace KingdomClash
{
    /// <summary>
    /// Click handler untuk Farm Building
    /// </summary>
    public class FarmBuildingClickHandler : MonoBehaviour
    {
        // Reference ke farm building
        private FarmBuilding farmBuilding;
        
        private void Start()
        {
            // Get reference ke farm building
            farmBuilding = GetComponent<FarmBuilding>();
            if (farmBuilding == null)
            {
                farmBuilding = GetComponentInParent<FarmBuilding>();
            }
        }
        
        private void OnMouseDown()
        {
            if (farmBuilding == null)
                return;            // Menggunakan FarmBuildingPanel untuk menangani FarmBuilding
            if (KingdomClash.UI.FarmBuildingPanel.Instance != null)
            {
                // Pastikan BuildingPanel tidak aktif untuk menghindari tumpang tindih
                if (KingdomClash.UI.BuildingPanel.Instance != null && 
                    KingdomClash.UI.BuildingPanel.Instance.gameObject.activeSelf)
                {
                    KingdomClash.UI.BuildingPanel.Instance.ClosePanel();
                }
                
                // Tampilkan FarmBuildingPanel
                KingdomClash.UI.FarmBuildingPanel.Instance.OpenPanel(farmBuilding);
            }// Menampilkan health bar - Gunakan namespace lengkap
            KingdomClash.UI.BuildingHealthBar healthBar = farmBuilding.GetComponentInChildren<KingdomClash.UI.BuildingHealthBar>();
            if (healthBar != null)
            {
                healthBar.ShowTemporary();
            }
            else
            {
                // Coba cari dari GameObject ini juga
                healthBar = GetComponentInChildren<KingdomClash.UI.BuildingHealthBar>();
                if (healthBar != null)
                {
                    healthBar.ShowTemporary();
                }
                else
                {
                    Debug.Log("Health bar tidak ditemukan di farm building");
                }
            }
        }
    }
}
