using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Base class for all buildings in the game
    /// </summary>
    public class Building : MonoBehaviour
    {
        [Header("Building Properties")]
        [SerializeField] private string buildingName;
        [SerializeField] private string buildingDescription;
        [SerializeField] private int health = 100;
        [SerializeField] private int maxHealth = 100;
        
        [Header("Resource Production")]
        [SerializeField] private bool producesResources = false;
        [SerializeField] private string resourceType = ""; // "wood", "stone", "iron", "food"
        [SerializeField] private int productionAmount = 0;
        [SerializeField] private float productionInterval = 60f; // Seconds
        
        private float lastProductionTime = 0f;
        
        private void Start()
        {
            lastProductionTime = Time.time;
        }
        
        private void Update()
        {
            // Handle resource production if applicable
            if (producesResources && Time.time - lastProductionTime >= productionInterval)
            {
                ProduceResources();
                lastProductionTime = Time.time;
            }
        }
        
        /// <summary>
        /// Produce resources if this is a resource-producing building
        /// </summary>
        private void ProduceResources()
        {
            if (!producesResources || string.IsNullOrEmpty(resourceType) || productionAmount <= 0)
                return;
                
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateResource(resourceType, productionAmount);
                Debug.Log($"{buildingName} produced {productionAmount} {resourceType}");
            }
        }
        
        /// <summary>
        /// Take damage when the building is attacked
        /// </summary>
        public void TakeDamage(int amount)
        {
            health -= amount;
            
            if (health <= 0)
            {
                DestroyBuilding();
            }
        }
        
        /// <summary>
        /// Destroys the building
        /// </summary>
        private void DestroyBuilding()
        {
            // Play destruction effects here
            
            // Remove the building
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Get the building's name
        /// </summary>
        public string GetBuildingName()
        {
            return buildingName;
        }
        
        /// <summary>
        /// Get the building's current health
        /// </summary>
        public int GetHealth()
        {
            return health;
        }
        
        /// <summary>
        /// Get the building's maximum health
        /// </summary>
        public int GetMaxHealth()
        {
            return maxHealth;
        }
    }
}
