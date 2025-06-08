using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Merepresentasikan titik sumber daya (resource node) yang dapat dikumpulkan oleh worker
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource Properties")]
        [SerializeField] private string resourceType = "wood"; // "wood", "stone", "iron", "food"
        [SerializeField] private int totalResource = 1000; // Total resource yang bisa diambil dari node ini
        [SerializeField] private int remainingResource; // Resource yang tersisa
        
        [Header("Gathering Settings")]
        [SerializeField] private int gatherAmount = 5; // Jumlah yang diambil per pengumpulan
        [SerializeField] private float gatherCooldown = 1.5f; // Waktu antar pengumpulan
        
        [Header("Visuals")]
        [SerializeField] private GameObject fullStateObject; // Object yang menunjukkan node masih penuh
        [SerializeField] private GameObject depleteStateObject; // Object yang menunjukkan node hampir habis
        [SerializeField] private GameObject emptyStateObject; // Object yang menunjukkan node kosong
        [SerializeField] private float depletionThreshold = 0.3f; // Persentase ketika status berubah ke hampir habis
        
        private void Awake()
        {
            // Set remaining resources to total at start
            remainingResource = totalResource;
            UpdateVisuals();
        }
        
        private void Start()
        {
            // Register with possible resource manager if needed
        }
        
        /// <summary>
        /// Kumpulkan resource dari node ini
        /// </summary>
        /// <param name="amount">Jumlah yang ingin dikumpulkan (default: gatherAmount)</param>
        /// <returns>Jumlah yang berhasil dikumpulkan</returns>
        public int GatherResource(int amount = -1)
        {
            // Use default gather amount if not specified
            if (amount < 0) amount = gatherAmount;
            
            // Can't gather if empty
            if (remainingResource <= 0) return 0;
            
            // Calculate how much to actually gather (don't exceed remaining)
            int actualGatherAmount = Mathf.Min(amount, remainingResource);
            
            // Reduce remaining resource
            remainingResource -= actualGatherAmount;
            
            // Update visuals based on new amount
            UpdateVisuals();
            
            // Return how much was gathered
            return actualGatherAmount;
        }
        
        /// <summary>
        /// Dapatkan tipe resource node ini
        /// </summary>
        public string GetResourceType()
        {
            return resourceType;
        }
        
        /// <summary>
        /// Dapatkan cooldown pengumpulan
        /// </summary>
        public float GetGatherCooldown()
        {
            return gatherCooldown;
        }
        
        /// <summary>
        /// Update tampilan visual resource node berdasarkan jumlah yang tersisa
        /// </summary>
        private void UpdateVisuals()
        {
            if (remainingResource <= 0)
            {
                // Empty state
                SetVisibleState(false, false, true);
            }
            else if ((float)remainingResource / totalResource <= depletionThreshold)
            {
                // Depleted state
                SetVisibleState(false, true, false);
            }
            else
            {
                // Full state
                SetVisibleState(true, false, false);
            }
        }
        
        /// <summary>
        /// Atur status yang terlihat
        /// </summary>
        private void SetVisibleState(bool full, bool depleted, bool empty)
        {
            if (fullStateObject != null) fullStateObject.SetActive(full);
            if (depleteStateObject != null) depleteStateObject.SetActive(depleted);
            if (emptyStateObject != null) emptyStateObject.SetActive(empty);
        }
    }
}
