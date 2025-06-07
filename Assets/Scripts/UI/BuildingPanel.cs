using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingdomClash.UI
{
    /// <summary>
    /// Panel untuk menampilkan informasi bangunan yang dipilih
    /// </summary>
    public class BuildingPanel : MonoBehaviour
    {
        // Singleton instance
        public static BuildingPanel Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI buildingDescriptionText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button destroyButton;

        // Referensi ke bangunan yang dipilih
        private Building selectedBuilding;

        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }

            // Setup destroy button
            if (destroyButton != null)
            {
                destroyButton.onClick.AddListener(DestroySelectedBuilding);
            }            // Panel tidak ditampilkan saat pertama kali
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Tampilkan panel dengan informasi bangunan
        /// </summary>
        public void ShowBuildingInfo(Building building)
        {
            selectedBuilding = building;

            if (building != null)
            {
                // Update UI dengan informasi bangunan
                if (buildingNameText != null)
                    buildingNameText.text = building.GetBuildingName();

                if (buildingDescriptionText != null)
                    buildingDescriptionText.text = building.GetBuildingDescription();

                if (healthText != null)
                    healthText.text = $"Health: {building.GetHealth()} / {building.GetMaxHealth()}";

                // Tampilkan panel
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Update informasi di panel (dipanggil saat health berubah)
        /// </summary>
        public void UpdateInfo()
        {
            if (selectedBuilding != null)
            {
                if (healthText != null)
                    healthText.text = $"Health: {selectedBuilding.GetHealth()} / {selectedBuilding.GetMaxHealth()}";
            }
        }

        /// <summary>
        /// Tutup panel
        /// </summary>
        public void ClosePanel()
        {
            selectedBuilding = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hancurkan bangunan yang dipilih
        /// </summary>
        private void DestroySelectedBuilding()
        {
            if (selectedBuilding != null)
            {
                // Panggil fungsi untuk menghancurkan bangunan
                selectedBuilding.DestroyBuilding();
                ClosePanel();
            }
        }
    }
}
