using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace KingdomClash
{
    /// <summary>
    /// Script untuk membuat BuildingManager secara otomatis saat scene dimuat
    /// </summary>
    public class BuildingManagerBootstrap : MonoBehaviour
    {
        // Singleton instance
        private static BuildingManagerBootstrap instance;
        
        // Prefab untuk BuildingManager
        [SerializeField] private GameObject buildingManagerPrefab;
        
        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from scene loaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if this is the game scene
            if (scene.name == "GameScene")
            {
                StartCoroutine(CreateBuildingManager());
            }
        }
        
        private IEnumerator CreateBuildingManager()
        {
            // Wait for a frame to ensure everything is initialized
            yield return null;
            
            // Check if BuildingManager already exists
            if (BuildingManager.Instance == null)
            {
                Debug.Log("Creating BuildingManager...");
                
                // Create BuildingManager from prefab if available
                if (buildingManagerPrefab != null)
                {
                    Instantiate(buildingManagerPrefab);
                }
                else
                {
                    // Create a new GameObject with BuildingManager component
                    GameObject buildingManagerObj = new GameObject("BuildingManager");
                    buildingManagerObj.AddComponent<BuildingManager>();
                }
                
                Debug.Log("BuildingManager created successfully");
            }
        }
    }
}
