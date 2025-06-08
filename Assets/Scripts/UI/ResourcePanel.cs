using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KingdomClash.UI
{
    /// <summary>
    /// Manages the resource display panel within the HUD
    /// </summary>
    public class ResourcePanel : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceDisplay
        {
            public Image resourceIcon;
            public TextMeshProUGUI resourceText;
        }

        [Header("Wood Resource Display")]
        [SerializeField] private ResourceDisplay woodDisplay;

        [Header("Stone Resource Display")]
        [SerializeField] private ResourceDisplay stoneDisplay;

        [Header("Iron Resource Display")]
        [SerializeField] private ResourceDisplay ironDisplay;

        [Header("Food Resource Display")]
        [SerializeField] private ResourceDisplay foodDisplay;

        /// <summary>
        /// Updates all resource displays with the provided resource data
        /// </summary>
        /// <param name="resources">The current resources data</param>
        public void UpdateResourceDisplay(Resources resources)
        {
            if (resources == null) return;

            // Update wood resource
            if (woodDisplay != null && woodDisplay.resourceText != null)
            {
                woodDisplay.resourceText.text = resources.wood.ToString();
            }
            
            // Update stone resource
            if (stoneDisplay != null && stoneDisplay.resourceText != null)
            {
                stoneDisplay.resourceText.text = resources.stone.ToString();
            }
            
            // Update iron resource
            if (ironDisplay != null && ironDisplay.resourceText != null)
            {
                ironDisplay.resourceText.text = resources.iron.ToString();
            }
            
            // Update food resource
            if (foodDisplay != null && foodDisplay.resourceText != null)
            {
                foodDisplay.resourceText.text = resources.food.ToString();
            }
        }
        
        /// <summary>
        /// Set default values for the resource display
        /// </summary>
        public void SetDefaultValues()
        {
            if (woodDisplay != null && woodDisplay.resourceText != null)
            {
                woodDisplay.resourceText.text = "0";
            }
            
            if (stoneDisplay != null && stoneDisplay.resourceText != null)
            {
                stoneDisplay.resourceText.text = "0";
            }
            
            if (ironDisplay != null && ironDisplay.resourceText != null)
            {
                ironDisplay.resourceText.text = "0";
            }
            
            if (foodDisplay != null && foodDisplay.resourceText != null)
            {
                foodDisplay.resourceText.text = "0";
            }
        }
        
        /// <summary>
        /// Updates all resource displays using the current game data
        /// </summary>
        public void UpdateResourceDisplay()
        {
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentGameData() == null)
            {
                Debug.LogWarning("Cannot update resource display: GameManager or GameData is null");
                return;
            }
            
            Resources resources = GameManager.Instance.GetCurrentGameData().resources;
            
            UpdateResourceDisplay(resources);
        }
        
        // Singleton instance untuk memudahkan akses
        public static ResourcePanel Instance { get; private set; }
        
        private void Awake()
        {
            // Setup Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            // Update resource display at start
            UpdateResourceDisplay();
            
            // Subscribe to the resource update event - ini akan diimplementasikan nanti jika diperlukan
        }
        
        private void Update()
        {
            // Auto-update setiap beberapa detik
            if (Time.frameCount % 60 == 0) // Update setiap ~1 detik pada 60 FPS
            {
                UpdateResourceDisplay();
            }
        }
    }
}
