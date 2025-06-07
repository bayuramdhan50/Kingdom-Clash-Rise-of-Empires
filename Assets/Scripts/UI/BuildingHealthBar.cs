using UnityEngine;
using UnityEngine.UI;

namespace KingdomClash.UI
{
    /// <summary>
    /// Health bar yang ditampilkan di atas bangunan
    /// </summary>
    public class BuildingHealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Canvas canvas;
        
        [Header("Options")]
        [SerializeField] private bool alwaysVisible = false;
        [SerializeField] private float hideDelay = 3f;
        
        // Referensi ke bangunan
        private Building targetBuilding;
        private Camera mainCamera;
        private float hideTimer;
        
        private void Start()
        {
            mainCamera = Camera.main;
            
            // Pastikan canvas selalu menghadap ke kamera
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
            }
            
            // Sembunyikan health bar jika tidak perlu selalu ditampilkan
            if (!alwaysVisible)
            {
                SetVisible(false);
            }
            
            // Inisialisasi timer
            hideTimer = hideDelay;
        }
        
        private void LateUpdate()
        {
            // Pastikan health bar selalu menghadap kamera
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                 mainCamera.transform.rotation * Vector3.up);
            }
            
            // Update fill health bar
            UpdateHealthBar();
            
            // Timer untuk menyembunyikan health bar
            if (!alwaysVisible && hideTimer > 0)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    SetVisible(false);
                }
            }
        }
        
        /// <summary>
        /// Set bangunan target untuk health bar
        /// </summary>
        public void SetTargetBuilding(Building building)
        {
            targetBuilding = building;
            UpdateHealthBar();
            
            // Tampilkan health bar dan reset timer
            if (!alwaysVisible)
            {
                SetVisible(true);
                hideTimer = hideDelay;
            }
        }
        
        /// <summary>
        /// Update tampilan health bar berdasarkan health bangunan
        /// </summary>
        private void UpdateHealthBar()
        {
            if (targetBuilding != null && healthFill != null)
            {
                float healthPercent = (float)targetBuilding.GetHealth() / targetBuilding.GetMaxHealth();
                healthFill.fillAmount = healthPercent;
                
                // Ubah warna health bar berdasarkan persentase health
                if (healthPercent > 0.6f)
                {
                    healthFill.color = Color.green; // Health > 60%
                }
                else if (healthPercent > 0.3f)
                {
                    healthFill.color = Color.yellow; // Health 30-60%
                }
                else
                {
                    healthFill.color = Color.red; // Health < 30%
                }
            }
        }
        
        /// <summary>
        /// Tampilkan atau sembunyikan health bar
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Tampilkan health bar untuk beberapa waktu
        /// </summary>
        public void ShowTemporary()
        {
            if (!alwaysVisible)
            {
                SetVisible(true);
                hideTimer = hideDelay;
            }
        }
    }
}
