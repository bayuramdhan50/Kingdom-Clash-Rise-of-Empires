using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using KingdomClash.Characters;
using Random = UnityEngine.Random;

namespace KingdomClash
{
    /// <summary>
    /// Handles initializing a new game with starter resources, buildings and units
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private float terrainWidth = 1000f;
        [SerializeField] private float terrainHeight = 1000f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Player Starter Pack")]
        [SerializeField] private GameObject castlePrefab;
        [SerializeField] private GameObject workerPrefab;
        [SerializeField] private float minimumDistanceFromEdge = 100f;
        [SerializeField] private float minimumDistanceFromCenter = 200f;

        [Header("Enemy AI")]
        [SerializeField] private GameObject enemyCastlePrefab;
        [SerializeField] private GameObject enemyWorkerPrefab;
        [SerializeField] private Material enemyMaterial; // Material to distinguish enemy units/buildings
        [SerializeField] private bool disableAutomaticAIController = false; // Set to true to prevent automatic AI controller creation

        private void Start()
        {
            // Register with GameManager to be called when a new game starts
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewGameStarted += InitializeNewGame;
            }
            else
            {
                Debug.LogError("GameManager not found! GameInitializer needs GameManager to function properly.");
            }
        }

        private void OnDestroy()
        {
            // Clean up event subscription
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNewGameStarted -= InitializeNewGame;
            }
        }        /// <summary>
        /// Initialize a new game with starter buildings and units
        /// </summary>
        public void InitializeNewGame()
        {
            Debug.Log("Initializing new game with starter pack and enemy AI...");
            
            // Place player starter buildings and units
            PlacePlayerStarterPack();
            
            // Place enemy AI buildings and units
            PlaceEnemyStarterPack();
            
            // Initialize the enemy AI controller (only if automatic creation is not disabled)
            if (!disableAutomaticAIController)
            {
                InitializeEnemyAI();
            }
            else
            {
                Debug.Log("Automatic AI controller creation is disabled - using the manually created controller");
            }
        }

        /// <summary>
        /// Places initial buildings and units for the player
        /// </summary>
        private void PlacePlayerStarterPack()
        {
            // Find a suitable position for the player's castle in the bottom-left quarter of the map
            Vector3 castlePosition = GetRandomPositionInQuadrant(Quadrant.BottomLeft);
            
            // Ensure we have a valid position
            if (castlePosition != Vector3.zero)
            {            // Apply rotation for player buildings (X axis at -90 degrees)
            Quaternion playerBuildingRotation = Quaternion.Euler(-90f, 0f, 0f);
            
            // Place castle with proper rotation
            GameObject castle = Instantiate(castlePrefab, castlePosition, playerBuildingRotation);
            Building castleComponent = castle.GetComponent<Building>();
            
            // Register the building with BuildingManager
            if (BuildingManager.Instance != null && castleComponent != null)
            {
                BuildingManager.Instance.RegisterPlacedBuilding(castleComponent);
                Debug.Log($"Player castle placed at {castlePosition}");
                
                // Position camera at player's castle
                PositionCameraAtPlayerCastle(castlePosition);
            }
                  // Place worker near the castle
                Vector3 workerOffset = new Vector3(Random.Range(5f, 15f), 0, Random.Range(5f, 15f));
                Vector3 workerPosition = castlePosition + workerOffset;
                
                // Adjust worker Y position to ground level
                workerPosition.y = GetTerrainHeight(workerPosition);
                
                // Use the same rotation as the castle for worker (X axis at -90 degrees)
                GameObject worker = Instantiate(workerPrefab, workerPosition, playerBuildingRotation);
                Unit workerUnit = worker.GetComponent<Unit>();
                
                // Add worker to unit tracking if possible
                if (workerUnit != null)
                {
                    Debug.Log($"Player worker placed at {workerPosition}");
                }
            }
            else
            {
                Debug.LogError("Failed to find a valid position for player castle!");
            }
        }

        /// <summary>
        /// Places initial buildings and units for the enemy AI
        /// </summary>
        private void PlaceEnemyStarterPack()
        {
            // Find a suitable position for the enemy castle in the top-right quarter of the map
            Vector3 castlePosition = GetRandomPositionInQuadrant(Quadrant.TopRight);            // Ensure we have a valid position
            if (castlePosition != Vector3.zero)
            {
                // Enemy buildings should use default rotation (no -90 rotation needed)
                Quaternion enemyBuildingRotation = Quaternion.identity;
                
                // Place enemy castle with default rotation
                GameObject enemyCastle = Instantiate(enemyCastlePrefab ? enemyCastlePrefab : castlePrefab, castlePosition, enemyBuildingRotation);
                Building castleComponent = enemyCastle.GetComponent<Building>();
                
                // Apply enemy material if available and no specific enemy prefab
                if (enemyMaterial != null && enemyCastlePrefab == null)
                {
                    ApplyMaterialRecursively(enemyCastle, enemyMaterial);
                }
                
                // Tag the castle as an enemy building
                enemyCastle.tag = "EnemyBuilding";
                
                // Register the building with BuildingManager
                if (BuildingManager.Instance != null && castleComponent != null)
                {
                    BuildingManager.Instance.RegisterPlacedBuilding(castleComponent);
                    Debug.Log($"Enemy castle placed at {castlePosition}");
                }
                
                // Place enemy worker near the castle
                Vector3 workerOffset = new Vector3(Random.Range(5f, 15f), 0, Random.Range(5f, 15f));
                Vector3 workerPosition = castlePosition + workerOffset;                // Adjust worker Y position to ground level
                workerPosition.y = GetTerrainHeight(workerPosition);
                
                // Use default rotation for enemy units (no -90 rotation needed)
                GameObject enemyWorker = Instantiate(enemyWorkerPrefab ? enemyWorkerPrefab : workerPrefab, workerPosition, Quaternion.identity);
                Unit workerUnit = enemyWorker.GetComponent<Unit>();
                
                // Apply enemy material if available and no specific enemy prefab
                if (enemyMaterial != null && enemyWorkerPrefab == null)
                {
                    ApplyMaterialRecursively(enemyWorker, enemyMaterial);
                }
                
                // Tag the worker as an enemy unit
                enemyWorker.tag = "EnemyUnit";
                
                // Add worker to unit tracking if possible
                if (workerUnit != null)
                {
                    Debug.Log($"Enemy worker placed at {workerPosition}");
                }
            }
            else
            {
                Debug.LogError("Failed to find a valid position for enemy castle!");
            }
        }        /// <summary>
        /// Initialize the enemy AI controller
        /// </summary>
        private void InitializeEnemyAI()
        {
            // Look for an existing EnemyAIController GameObject that might have been created manually
            GameObject[] existingControllers = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "EnemyAIController").ToArray();
            
            // Log what we found
            Debug.Log($"Found {existingControllers.Length} existing EnemyAIController GameObjects");
              // If multiple controllers found, keep only one
            if (existingControllers.Length > 1)
            {
                Debug.LogWarning("Multiple EnemyAIControllers found! Keeping the manual one with prefabs assigned.");
                
                // Strategy: Keep the one with prefabs assigned (likely the manual one)
                // First, find controllers with SimpleAI component
                List<GameObject> controllersWithAI = new List<GameObject>();
                foreach (GameObject controller in existingControllers)
                {
                    SimpleAI ai = controller.GetComponent<SimpleAI>();
                    if (ai != null)
                    {
                        controllersWithAI.Add(controller);
                        // Log controller info to help debugging
                        Debug.Log($"Found AI controller: {controller.name} (ID: {controller.GetInstanceID()})");
                    }
                }
                
                // Default: Keep the first one we found with SimpleAI
                GameObject keepController = controllersWithAI.Count > 0 ? controllersWithAI[0] : existingControllers[0];
                  // Ternyata logika sebelumnya terbalik - untuk kasus ini kita harus memilih yang ID-nya lebih besar
                // yang biasanya dibuat saat runtime (karena objek manual Anda dihancurkan)
                foreach (GameObject controller in controllersWithAI)
                {
                    // Keep the controller with the HIGHER instance ID (opposite of before)
                    if (controller.GetInstanceID() > keepController.GetInstanceID())
                    {
                        keepController = controller;
                    }
                }
                
                Debug.Log($"Keeping EnemyAIController with ID: {keepController.GetInstanceID()}");
                
                // Destroy all others
                foreach (GameObject controller in existingControllers)
                {
                    if (controller != keepController)
                    {
                        Debug.Log($"Destroying duplicate EnemyAIController: {controller.GetInstanceID()}");
                        Destroy(controller);
                    }
                }
            }
            // If no controllers found, create one
            else if (existingControllers.Length == 0)
            {
                Debug.Log("No EnemyAIController found. Creating one...");
                GameObject aiControllerObject = new GameObject("EnemyAIController");
                aiControllerObject.AddComponent<SimpleAI>();
                Debug.Log("Enemy AI controller initialized");
            }
        }

        /// <summary>
        /// Map quadrants for positioning
        /// </summary>
        private enum Quadrant
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        
        /// <summary>
        /// Get a random position within a specific quadrant of the map
        /// </summary>
        private Vector3 GetRandomPositionInQuadrant(Quadrant quadrant)
        {
            // Define quadrant bounds
            float minX, maxX, minZ, maxZ;
            
            switch (quadrant)
            {
                case Quadrant.TopLeft:
                    minX = minimumDistanceFromEdge;
                    maxX = terrainWidth / 2 - minimumDistanceFromCenter;
                    minZ = terrainHeight / 2 + minimumDistanceFromCenter;
                    maxZ = terrainHeight - minimumDistanceFromEdge;
                    break;
                    
                case Quadrant.TopRight:
                    minX = terrainWidth / 2 + minimumDistanceFromCenter;
                    maxX = terrainWidth - minimumDistanceFromEdge;
                    minZ = terrainHeight / 2 + minimumDistanceFromCenter;
                    maxZ = terrainHeight - minimumDistanceFromEdge;
                    break;
                    
                case Quadrant.BottomLeft:
                    minX = minimumDistanceFromEdge;
                    maxX = terrainWidth / 2 - minimumDistanceFromCenter;
                    minZ = minimumDistanceFromEdge;
                    maxZ = terrainHeight / 2 - minimumDistanceFromCenter;
                    break;
                    
                case Quadrant.BottomRight:
                default:
                    minX = terrainWidth / 2 + minimumDistanceFromCenter;
                    maxX = terrainWidth - minimumDistanceFromEdge;
                    minZ = minimumDistanceFromEdge;
                    maxZ = terrainHeight / 2 - minimumDistanceFromCenter;
                    break;
            }
            
            // Try to find a valid position
            for (int i = 0; i < 10; i++) // Limit attempts to prevent infinite loops
            {
                float randomX = Random.Range(minX, maxX);
                float randomZ = Random.Range(minZ, maxZ);
                
                Vector3 position = new Vector3(randomX, 1000f, randomZ); // Start high
                
                // Raycast down to find terrain height
                RaycastHit hit;
                if (Physics.Raycast(position, Vector3.down, out hit, 2000f, groundLayer))
                {
                    position.y = hit.point.y;
                    return position;
                }
            }
            
            // Fallback to approximate position if raycast fails
            float y = GetTerrainHeight(new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2));
            return new Vector3((minX + maxX) / 2, y, (minZ + maxZ) / 2);
        }
        
        /// <summary>
        /// Get the height of the terrain at a specific position
        /// </summary>
        private float GetTerrainHeight(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(position.x, 1000f, position.z), Vector3.down, out hit, 2000f, groundLayer))
            {
                return hit.point.y;
            }
            
            // Fallback height if no ground is found
            return 0f;
        }
        
        /// <summary>
        /// Apply a material to all renderers in an object hierarchy
        /// </summary>
        private void ApplyMaterialRecursively(GameObject obj, Material material)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.materials = materials;
            }
        }
        
        /// <summary>
        /// Position the main camera to view the player's castle
        /// </summary>
        /// <param name="castlePosition">The position of the player's castle</param>
        private void PositionCameraAtPlayerCastle(Vector3 castlePosition)
        {
            // Find the RTS camera controller
            RTSCameraController cameraController = FindObjectOfType<RTSCameraController>();
            if (cameraController != null)
            {
                // Calculate a good camera position
                // We want to be above and slightly offset from the castle
                Vector3 cameraOffset = new Vector3(0f, 40f, -25f); // Above and behind
                Vector3 cameraPosition = castlePosition + cameraOffset;
                
                // Set camera position
                cameraController.transform.position = cameraPosition;
                
                // Point camera at castle (slightly above the base)
                Vector3 lookTarget = castlePosition + new Vector3(0f, 5f, 0f);
                cameraController.transform.LookAt(lookTarget);
                
                // If the camera controller has a zoom setting, set it to a reasonable value
                if (cameraController.cam != null)
                {
                    cameraController.cam.orthographicSize = 20f; // Adjust based on your game's scale
                }
                
                // Update any saved camera position in GameData
                if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameData() != null)
                {
                    GameData gameData = GameManager.Instance.GetCurrentGameData();
                    gameData.cameraPosition = new Vector3Data(cameraPosition);
                    gameData.cameraRotation = new QuaternionData(cameraController.transform.rotation);
                    gameData.cameraZoom = cameraController.cam != null ? cameraController.cam.orthographicSize : 20f;
                }
                
                Debug.Log($"Camera positioned at {cameraPosition}, looking at player castle");
            }
            else
            {
                Debug.LogWarning("RTSCameraController not found. Cannot position camera.");
            }
        }
    }
}
