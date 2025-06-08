using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KingdomClash
{
    /// <summary>
    /// Komponen untuk menampilkan resource yang dibawa oleh worker
    /// </summary>
    public class ResourceIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject indicatorObject; // Root object untuk indicator UI
        [SerializeField] private TextMeshProUGUI resourceText; // Text untuk jumlah resource
        [SerializeField] private Image resourceIcon; // Ikon resource
        
        [Header("Resource Icons")]
        [SerializeField] private Sprite woodIcon;
        [SerializeField] private Sprite stoneIcon;
        [SerializeField] private Sprite ironIcon;
        [SerializeField] private Sprite foodIcon;
        
        [Header("Display Settings")]
        [SerializeField] private float showDuration = 3f; // Berapa lama indicator ditampilkan
        [SerializeField] private bool alwaysShown = true; // Apakah selalu ditampilkan saat membawa resource
        
        private float hideTimer = 0f;
        
        private void Awake()
        {
            // Hide at start
            if (indicatorObject != null)
            {
                indicatorObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Tampilkan indicator dengan resource type dan jumlah tertentu
        /// </summary>
        public void ShowResource(string resourceType, int amount)
        {
            if (indicatorObject == null || resourceText == null)
                return;
                
            // Set resource text
            resourceText.text = amount.ToString();
            
            // Set appropriate icon
            if (resourceIcon != null)
            {
                switch (resourceType.ToLower())
                {
                    case "wood":
                        resourceIcon.sprite = woodIcon;
                        break;
                    case "stone":
                        resourceIcon.sprite = stoneIcon;
                        break;
                    case "iron":
                        resourceIcon.sprite = ironIcon;
                        break;
                    case "food":
                        resourceIcon.sprite = foodIcon;
                        break;
                }
            }
            
            // Show indicator
            indicatorObject.SetActive(true);
            
            // Reset timer if not always shown
            if (!alwaysShown)
            {
                hideTimer = showDuration;
            }
        }
        
        /// <summary>
        /// Sembunyikan indicator resource
        /// </summary>
        public void HideResource()
        {
            if (indicatorObject != null)
            {
                indicatorObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Handle auto-hiding
            if (!alwaysShown && hideTimer > 0)
            {
                hideTimer -= Time.deltaTime;
                
                if (hideTimer <= 0)
                {
                    HideResource();
                }
            }
        }
    }
}
