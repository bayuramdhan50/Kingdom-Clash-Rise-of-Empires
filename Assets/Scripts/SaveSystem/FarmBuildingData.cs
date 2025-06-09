using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Stores data for Farm Buildings that can be upgraded
    /// </summary>
    [System.Serializable]
    public class FarmBuildingData : BuildingData
    {
        // Level information
        public int currentLevel;
        public int maxLevel;
        
        // Resources stored in farm
        public int storedResources;
        public int maxStoredResources;
        
        // Default constructor
        public FarmBuildingData() : base() { }
        
        /// <summary>
        /// Constructor to create data from an existing farm building
        /// </summary>
        /// <param name="farmBuilding">The farm building</param>
        /// <param name="prefabName">Name of the prefab for instantiation</param>
        public FarmBuildingData(FarmBuilding farmBuilding, string prefabName) : base(farmBuilding, prefabName)
        {
            this.currentLevel = farmBuilding.GetCurrentLevel();
            this.maxLevel = farmBuilding.GetMaxLevel();
            this.storedResources = farmBuilding.GetStoredResources();
            this.maxStoredResources = farmBuilding.GetMaxStoredResources();
        }
    }
}
