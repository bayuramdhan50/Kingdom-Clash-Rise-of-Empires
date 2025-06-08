using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash
{
    /// <summary>
    /// Panel untuk memuat dan me-spawn unit dari save data
    /// </summary>
    public class UnitLoaderPanel : MonoBehaviour
    {
        // Dictionary untuk menyimpan prefab unit
        private Dictionary<string, GameObject> unitPrefabs = new Dictionary<string, GameObject>();
        
        // Caching unit prefabs locally if not available from TrainingManager
        [SerializeField] private GameObject infantryPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject cavalryPrefab;
        
        private void Awake()
        {
            // Load prefab references from TrainingManager
            UI.TrainingManager trainingManager = UI.TrainingManager.Instance;
            if (trainingManager != null)
            {
                // Load prefab references
                unitPrefabs = trainingManager.GetUnitPrefabs();
                  if (unitPrefabs == null || unitPrefabs.Count == 0)
                {
                    LoadPrefabsFromResources();
                }
            }
            else
            {
                LoadPrefabsFromResources();
            }
        }
        
        /// <summary>
        /// Load unit prefabs from Resources folder as a fallback
        /// </summary>
        private void LoadPrefabsFromResources()
        {
            // Initialize prefab dictionary
            unitPrefabs = new Dictionary<string, GameObject>();
            
            // Try loading from serialized fields first
            if (infantryPrefab != null) unitPrefabs.Add("infantry", infantryPrefab);
            if (archerPrefab != null) unitPrefabs.Add("archer", archerPrefab);
            if (cavalryPrefab != null) unitPrefabs.Add("cavalry", cavalryPrefab);
              // Try loading from Resources as fallback - Uncomment if you have a Resources folder with Units
            /*
            if (!unitPrefabs.ContainsKey("infantry"))
            {
                Object infantryObj = Resources.LoadAssetAtPath("Assets/Prefabs/Units/Infantry.prefab", typeof(GameObject));
                if (infantryObj != null) unitPrefabs.Add("infantry", infantryObj as GameObject);
            }
            
            if (!unitPrefabs.ContainsKey("archer"))
            {
                Object archerObj = Resources.LoadAssetAtPath("Assets/Prefabs/Units/Archer.prefab", typeof(GameObject));
                if (archerObj != null) unitPrefabs.Add("archer", archerObj as GameObject);
            }
            
            if (!unitPrefabs.ContainsKey("cavalry"))
            {
                Object cavalryObj = Resources.LoadAssetAtPath("Assets/Prefabs/Units/Cavalry.prefab", typeof(GameObject));            if (cavalryObj != null) unitPrefabs.Add("cavalry", cavalryObj as GameObject);
            }
            */
        }
        
        /// <summary>
        /// Loads and spawns units from saved data
        /// </summary>
        /// <param name="savedUnits">List of saved unit data</param>
        /// <returns>Number of units successfully spawned</returns>        
        public int LoadSavedUnits(List<UnitData> savedUnits)
        {
            if (savedUnits == null || savedUnits.Count == 0)
            {
                return 0;
            }
            
            int successCount = 0;
            
            foreach (UnitData unitData in savedUnits)
            {
                if (unitData == null)
                    continue;
                    
                // Spawn the unit from prefab
                bool success = SpawnUnitFromData(unitData);
                
                if (success)
                    successCount++;
            }
            
            return successCount;
        }
        
        /// <summary>
        /// Spawns a unit from saved data
        /// </summary>
        private bool SpawnUnitFromData(UnitData unitData)
        {
            if (unitData == null)
                return false;
            
            try
            {
                // Get unit type (convert to lowercase for consistency)
                string unitType = unitData.unitType.ToLower();
                
                // Get spawn position from saved data
                Vector3 spawnPos = unitData.position.ToVector3();
                
                // Variable to hold the unit game object
                GameObject unitObj = null;

                // Try to instantiate from prefab
                if (unitPrefabs.ContainsKey(unitType))
                {
                    GameObject prefab = unitPrefabs[unitType];
                    if (prefab != null)
                    {
                        unitObj = Instantiate(prefab, spawnPos, Quaternion.identity);
                    }
                }

                if (unitObj != null)
                {                    // Update unit stats
                    Characters.Unit unitComponent = unitObj.GetComponent<Characters.Unit>();
                    if (unitComponent != null)
                    {
                        unitComponent.SetHealth(unitData.health);
                        unitComponent.SetAttack(unitData.attack);
                        unitComponent.SetDefense(unitData.defense);
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Creates a placeholder unit when prefabs are not available
        /// </summary>
    }
}
