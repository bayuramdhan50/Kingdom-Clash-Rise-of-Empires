using UnityEngine;
using System.Collections;
using KingdomClash.Characters;
using KingdomClash.UI; // Tambahkan namespace untuk ResourcePanel

namespace KingdomClash
{
    /// <summary>
    /// Class untuk worker yang dapat mengumpulkan resource
    /// </summary>
    public class WorkerUnit : Unit
    {
        [Header("Worker Settings")]
        [SerializeField] private float gatherSpeed = 1.0f; // Pengali kecepatan mengumpulkan (1.0 = normal)
        [SerializeField] private float detectionRadius = 5.0f; // Jarak mendeteksi resource node
        [SerializeField] private int carryCapacity = 10; // Jumlah resource maksimal yang bisa dibawa
        [SerializeField] private GameObject gatheringEffect; // Effect saat mengumpulkan resource
        
        // Resource states
        private ResourceNode targetResource;
        private int carriedAmount = 0;
        private string carriedResourceType = "";
        private bool isGathering = false;
        private bool isReturning = false;
        
        // Storage reference
        private Building dropoffBuilding; // Bangunan untuk menyimpan resource
        private Vector3 dropoffPosition; // Posisi untuk mengembalikan resource
          // Float di atas kepala
        [SerializeField] private ResourceIndicator resourceIndicator;
        
        // Gunakan new alih-alih override karena tidak ada virtual void Start() di kelas dasar
        private void Start()
        {
            // Inisialisasi resource indicator jika belum ada
            if (resourceIndicator == null)
            {
                resourceIndicator = GetComponentInChildren<ResourceIndicator>();
            }
            
            // Set dropoff building - default ke townhall/command center
            FindDropoffBuilding();
            
            // Hide gathering effect if it exists
            if (gatheringEffect != null)
            {
                gatheringEffect.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Update resource indicator jika ada
            if (resourceIndicator != null)
            {
                if (carriedAmount > 0)
                {
                    resourceIndicator.ShowResource(carriedResourceType, carriedAmount);
                }
                else
                {
                    resourceIndicator.HideResource();
                }
            }
            
            // Jika sedang bergerak ke resource node dan sudah sampai, mulai gathering
            if (targetResource != null && !isGathering && !isReturning && 
                Vector3.Distance(transform.position, targetResource.transform.position) < 2.0f)
            {
                StartGathering();
            }
            
            // Jika sedang membawa resource penuh atau target habis, kembalikan ke dropoff
            if (carriedAmount >= carryCapacity && !isReturning)
            {
                ReturnToDropoff();
            }
            
            // Jika sedang kembali dan sudah sampai, dropoff resource
            if (isReturning && Vector3.Distance(transform.position, dropoffPosition) < 2.0f)
            {
                DepositResources();
            }
        }
        
        /// <summary>
        /// Perintah worker untuk mengumpulkan resource
        /// </summary>
        public void GatherResourceAt(ResourceNode resourceNode)
        {
            // Stop any current gathering
            StopAllCoroutines();
            isGathering = false;
            
            if (gatheringEffect != null)
            {
                gatheringEffect.SetActive(false);
            }
            
            // Set new target
            targetResource = resourceNode;
            isReturning = false;
            
            // Move to resource node
            if (targetResource != null)
            {
                MoveTo(targetResource.transform.position);
            }
        }
        
        /// <summary>
        /// Mulai mengumpulkan resource
        /// </summary>
        private void StartGathering()
        {
            if (targetResource == null) return;
            
            isGathering = true;
            
            // Show gathering effect if available
            if (gatheringEffect != null)
            {
                gatheringEffect.SetActive(true);
                gatheringEffect.transform.position = transform.position;
            }
            
            // Look at the resource
            Vector3 lookDirection = targetResource.transform.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                lookDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // Start gathering coroutine
            StartCoroutine(GatherResourceCoroutine());
        }
        
        /// <summary>
        /// Coroutine untuk mengumpulkan resource secara bertahap
        /// </summary>
        private IEnumerator GatherResourceCoroutine()
        {
            while (isGathering && targetResource != null && carriedAmount < carryCapacity)
            {
                // Calculate cooldown based on gather speed
                float cooldown = targetResource.GetGatherCooldown() / gatherSpeed;
                
                // Wait for cooldown
                yield return new WaitForSeconds(cooldown);
                
                // Gather resource
                int gathered = targetResource.GatherResource();
                
                if (gathered > 0)
                {
                    // Set resource type if this is first gathering
                    if (carriedAmount == 0)
                    {
                        carriedResourceType = targetResource.GetResourceType();
                    }
                    
                    // Add to carried amount
                    carriedAmount += gathered;
                    
                    Debug.Log($"Worker gathered {gathered} {carriedResourceType}. Now carrying: {carriedAmount}/{carryCapacity}");
                    
                    // Stop if full
                    if (carriedAmount >= carryCapacity)
                    {
                        isGathering = false;
                        ReturnToDropoff();
                        break;
                    }
                }
                else
                {
                    // Resource node is depleted
                    isGathering = false;
                    
                    // If we have some resources, return them
                    if (carriedAmount > 0)
                    {
                        ReturnToDropoff();
                    }
                    // Otherwise look for a new resource node
                    else
                    {
                        FindNewResourceNode();
                    }
                    break;
                }
            }
            
            // Turn off gathering effect
            if (gatheringEffect != null)
            {
                gatheringEffect.SetActive(false);
            }
        }
        
        /// <summary>
        /// Temukan bangunan untuk menyetor resource
        /// </summary>
        private void FindDropoffBuilding()
        {
            // Default Dropoff Buildings:
            // - Townhall
            // - Storage
            
            // Get all buildings
            Building[] buildings = FindObjectsOfType<Building>();
            
            foreach (Building building in buildings)
            {
                // For player workers, find player buildings
                if (gameObject.CompareTag("PlayerUnit") && 
                    (building.gameObject.CompareTag("Building") || building.gameObject.CompareTag("PlayerCastle")))
                {
                    dropoffBuilding = building;
                    dropoffPosition = building.transform.position;
                    return;
                }
                // For AI workers, find AI buildings
                else if (gameObject.CompareTag("EnemyUnit") && 
                    (building.gameObject.CompareTag("EnemyBuilding") || building.gameObject.CompareTag("EnemyCastle")))
                {
                    dropoffBuilding = building;
                    dropoffPosition = building.transform.position;
                    return;
                }
            }
            
            // If no building found, use our starting position
            dropoffPosition = transform.position;
            Debug.LogWarning("No dropoff building found for worker. Using starting position.");
        }
        
        /// <summary>
        /// Kembali ke bangunan dropoff
        /// </summary>
        private void ReturnToDropoff()
        {
            if (carriedAmount <= 0) return;
            
            // Stop gathering
            isGathering = false;
            isReturning = true;
            
            if (gatheringEffect != null)
            {
                gatheringEffect.SetActive(false);
            }
            
            // Go to dropoff
            MoveTo(dropoffPosition);
        }
        
        /// <summary>
        /// Setorkan resource yang dikumpulkan
        /// </summary>        
        private void DepositResources()
        {
            if (carriedAmount <= 0) return;

            // Add resources to global storage
            if (GameManager.Instance != null)
            {
                // Update the resource
                GameManager.Instance.UpdateResource(carriedResourceType, carriedAmount);
                
                // Update UI
                KingdomClash.UI.ResourcePanel resourcePanel = GameObject.FindObjectOfType<KingdomClash.UI.ResourcePanel>();
                if (resourcePanel != null)
                {
                    resourcePanel.UpdateResourceDisplay();
                }
            }
            
            // Reset carried amount
            carriedAmount = 0;
            carriedResourceType = "";
            
            // Update indicator
            if (resourceIndicator != null)
            {
                resourceIndicator.HideResource();
            }
            
            // Panggil fungsi untuk menampilkan status resource saat ini
            if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameData() != null)
            {
                Resources currentResources = GameManager.Instance.GetCurrentGameData().resources;
                Debug.Log($"Current resources - Wood: {currentResources.wood}, Stone: {currentResources.stone}, Iron: {currentResources.iron}, Food: {currentResources.food}");
            }
            
            isReturning = false;
            
            // Return to gathering if target still exists
            if (targetResource != null)
            {
                MoveTo(targetResource.transform.position);
            }
            else
            {
                FindNewResourceNode();
            }
        }
          /// <summary>
        /// Cari resource node baru terdekat
        /// </summary>
        private void FindNewResourceNode()
        {
            // Find all resource nodes within detection radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            
            float closestDistance = float.MaxValue;
            ResourceNode closestNode = null;
            
            // First try to find nodes within detection radius
            foreach (Collider col in colliders)
            {
                ResourceNode node = col.GetComponent<ResourceNode>();
                if (node == null)
                    node = col.GetComponentInParent<ResourceNode>();
                    
                if (node != null)
                {
                    float distance = Vector3.Distance(transform.position, node.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNode = node;
                    }
                }
            }
            
            // If nothing found in radius, fallback to FindObjectsOfType
            if (closestNode == null)
            {
                Debug.Log("No nodes found within detection radius, searching entire scene");
                ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();
                
                closestDistance = float.MaxValue;
                
                foreach (ResourceNode node in resourceNodes)
                {
                    float distance = Vector3.Distance(transform.position, node.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNode = node;
                    }
                }
            }
            
            // If found a node, go gather from it
            if (closestNode != null)
            {
                GatherResourceAt(closestNode);
            }
        }
          /// <summary>
        /// Method for handling worker death, akan dipanggil dari luar
        /// </summary>
        public void WorkerDie()
        {
            // Drop resources if carrying any
            if (carriedAmount > 0)
            {
                Debug.Log($"Worker died and dropped {carriedAmount} {carriedResourceType}");
                // Could spawn dropped resource here
            }
            
            // Worker is destroyed
            Destroy(gameObject);
        }
        
        // Check if the worker is killed by combat
        public void OnTakeDamage(int damage)
        {
            // Get current health after damage
            int currentHealth = GetHealth(); // menggunakan GetHealth dari Unit

            // If health is zero or less, worker dies
            if (currentHealth <= 0)
            {
                WorkerDie();
            }
        }
    }
}
