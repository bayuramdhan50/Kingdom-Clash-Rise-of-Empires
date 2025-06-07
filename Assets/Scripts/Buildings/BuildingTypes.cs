using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Example building types that can be created in the game
    /// </summary>
    public class BuildingTypes : MonoBehaviour
    {
        [System.Serializable]
        public class BuildingType
        {
            public string buildingName;
            public string description;
            public int woodCost;
            public int stoneCost;
            public int ironCost;
            public GameObject prefab;
            public Sprite icon;
        }
        
        // Singleton instance
        public static BuildingTypes Instance { get; private set; }
        
        [Header("Building Types")]
        [SerializeField] public BuildingType[] availableBuildings;
        
        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        /// <summary>
        /// Get a building type by name
        /// </summary>
        /// <param name="buildingName">Name of the building</param>
        /// <returns>The building type or null if not found</returns>
        public BuildingType GetBuildingTypeByName(string buildingName)
        {
            if (availableBuildings == null)
                return null;
                
            foreach (BuildingType type in availableBuildings)
            {
                if (type.buildingName == buildingName)
                    return type;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all available building types
        /// </summary>
        /// <returns>Array of all available building types</returns>
        public BuildingType[] GetAllBuildingTypes()
        {
            return availableBuildings;
        }
    }
}
