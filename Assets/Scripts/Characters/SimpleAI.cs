using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KingdomClash.Characters;

namespace KingdomClash
{
    /// <summary>
    /// Simple AI controller for enemy actions in the game
    /// Handles resource gathering, building construction, and unit training
    /// </summary>
    public class SimpleAI : MonoBehaviour
    {
        // AI states
        public enum AIState
        {
            Gathering,  // Gathering resources
            Building,   // Building structures
            Training,   // Training units
            Combat,     // Fighting enemies
            Idle        // Units patrolling or idle
        }
        
        [Header("AI Settings")]
        [SerializeField] private float decisionInterval = 20f; // Time between AI decisions in seconds
        [SerializeField] private float patrolRadius = 30f; // Radius for idle unit patrolling
        
        [Header("Resource Management")]
        [SerializeField] private int minResourceAmount = 200; // Minimum resources to keep
        
        [Header("Building Prefabs")]
        [SerializeField] private GameObject farmPrefab;
        [SerializeField] private GameObject lumbermillPrefab;
        [SerializeField] private GameObject barracksPrefab;
        [SerializeField] private GameObject stablePrefab;
        [SerializeField] private GameObject archeryPrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject wallCornerPrefab;
        [SerializeField] private GameObject towerPrefab;
        
        [Header("Unit Prefabs")]
        [SerializeField] private GameObject workerPrefab;
        [SerializeField] private GameObject infantryPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject cavalryPrefab;

        // Internal tracking
        private GameObject enemyCastle;
        private List<GameObject> enemyBuildings = new List<GameObject>();
        private List<GameObject> enemyUnits = new List<GameObject>();
        
        // AI Resources
        private Resources aiResources;
        
        // Current AI state
        private AIState currentState = AIState.Gathering;
        
        // Coroutine handle
        private Coroutine aiRoutine;

        private void Start()
        {
            // Initialize AI resources
            aiResources = new Resources
            {
                wood = 500,
                stone = 300,
                iron = 200,
                food = 600
            };
            
            // Find enemy buildings and units
            FindEnemyEntities();
            
            // Start AI behavior routine
            aiRoutine = StartCoroutine(AIDecisionLoop());
        }

        private void OnDestroy()
        {
            if (aiRoutine != null)
                StopCoroutine(aiRoutine);
        }

        /// <summary>
        /// Main AI decision-making loop
        /// </summary>
        private IEnumerator AIDecisionLoop()
        {
            while (true)
            {
                // Find enemy entities
                FindEnemyEntities();
                
                // Determine what to do next
                DetermineAIState();
                
                // Execute the current state
                switch (currentState)
                {
                    case AIState.Gathering:
                        HandleResourceGathering();
                        break;
                    case AIState.Building:
                        HandleBuilding();
                        break;
                    case AIState.Training:
                        HandleTraining();
                        break;
                    case AIState.Combat:
                        HandleCombat();
                        break;
                    case AIState.Idle:
                        HandleIdleUnits();
                        break;
                }
                
                // Wait before making next decision
                yield return new WaitForSeconds(decisionInterval);
            }
        }

        /// <summary>
        /// Find all enemy buildings and units in the scene
        /// </summary>
        private void FindEnemyEntities()
        {
            // Find enemy castle
            GameObject[] castles = GameObject.FindGameObjectsWithTag("EnemyBuilding");
            foreach (GameObject building in castles)
            {
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null && 
                    buildingComponent.GetBuildingName().ToLower().Contains("castle"))
                {
                    enemyCastle = building;
                    break;
                }
            }
            
            // Update enemy buildings list
            enemyBuildings.Clear();
            GameObject[] buildings = GameObject.FindGameObjectsWithTag("EnemyBuilding");
            foreach (GameObject building in buildings)
            {
                enemyBuildings.Add(building);
            }
            
            // Update enemy units list
            enemyUnits.Clear();
            GameObject[] units = GameObject.FindGameObjectsWithTag("EnemyUnit");
            foreach (GameObject unit in units)
            {
                enemyUnits.Add(unit);
            }
        }

        /// <summary>
        /// Determine the current AI state based on resources, buildings, and units
        /// </summary>
        private void DetermineAIState()
        {
            // Count units by type
            int workerCount = 0;
            int militaryUnitCount = 0;
            
            foreach (GameObject unit in enemyUnits)
            {
                Unit unitComponent = unit.GetComponent<Unit>();
                if (unitComponent != null)
                {
                    string unitType = unitComponent.GetUnitType().ToLower();
                    if (unitType.Contains("worker"))
                        workerCount++;
                    else
                        militaryUnitCount++;
                }
            }
            
            // Check for military buildings
            int barracksCount = 0;
            int archeryCount = 0;
            int stableCount = 0;
            int farmCount = 0;
            int lumbermillCount = 0;
            
            foreach (GameObject building in enemyBuildings)
            {
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    string buildingType = buildingComponent.GetBuildingName().ToLower();
                    if (buildingType.Contains("barrack"))
                        barracksCount++;
                    else if (buildingType.Contains("archery"))
                        archeryCount++;
                    else if (buildingType.Contains("stable"))
                        stableCount++;
                    else if (buildingType.Contains("farm"))
                        farmCount++;
                    else if (buildingType.Contains("lumber"))
                        lumbermillCount++;
                }
            }

            // Check resources
            bool lowResources = aiResources.wood < minResourceAmount || 
                               aiResources.stone < minResourceAmount || 
                               aiResources.food < minResourceAmount || 
                               aiResources.iron < minResourceAmount;

            // Decision making
            
            // Priority 1: Ensure we have workers
            if (workerCount < 3)
            {
                currentState = AIState.Training;
                return;
            }
            
            // Priority 2: If resources are low, gather more
            if (lowResources)
            {
                currentState = AIState.Gathering;
                return;
            }
            
            // Priority 3: Build military buildings if we don't have them
            if (barracksCount < 1 || archeryCount < 1 || stableCount < 1)
            {
                currentState = AIState.Building;
                return;
            }
            
            // Priority 4: Ensure we have a decent military force
            if (militaryUnitCount < 10)
            {
                currentState = AIState.Training;
                return;
            }
            
            // Priority 5: Look for enemies to attack
            GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
            if (playerUnits.Length > 0 && militaryUnitCount >= 5)
            {
                currentState = AIState.Combat;
                return;
            }
            
            // Default: Let units idle/patrol
            currentState = AIState.Idle;
        }

        /// <summary>
        /// Handle resource gathering tasks
        /// </summary>
        private void HandleResourceGathering()
        {
            // Find all workers
            List<GameObject> workers = new List<GameObject>();
            
            foreach (GameObject unit in enemyUnits)
            {
                Unit unitComponent = unit.GetComponent<Unit>();
                if (unitComponent != null && unitComponent.GetUnitType().ToLower().Contains("worker"))
                {
                    workers.Add(unit);
                }
            }
            
            if (workers.Count == 0)
                return;
                
            // Find resource nodes
            GameObject[] resourceNodes = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> validResourceNodes = new List<GameObject>();
            
            foreach (GameObject node in resourceNodes)
            {
                // Check for resource components/tags
                if (node.CompareTag("ResourceNode") || 
                    node.name.ToLower().Contains("tree") ||
                    node.name.ToLower().Contains("ore") ||
                    node.name.ToLower().Contains("stone") ||
                    node.name.ToLower().Contains("forest") ||
                    node.name.ToLower().Contains("mine"))
                {
                    validResourceNodes.Add(node);
                }
            }
            
            if (validResourceNodes.Count == 0)
                return;
            
            // Assign workers to resource nodes
            int workerIndex = 0;
            foreach (GameObject worker in workers)
            {
                if (validResourceNodes.Count == 0)
                    break;
                
                // Pick a resource node based on worker index
                GameObject targetNode = validResourceNodes[workerIndex % validResourceNodes.Count];
                
                // Use UnitMovementController or NavMesh to move the worker
                MoveUnitToTarget(worker, targetNode.transform.position);
                
                // Simulate resource gathering
                int gatherAmount = Random.Range(30, 50);
                aiResources.wood += gatherAmount;
                aiResources.stone += gatherAmount;
                aiResources.food += gatherAmount;
                aiResources.iron += gatherAmount;
                
                workerIndex++;
            }
        }

        /// <summary>
        /// Handle building construction
        /// </summary>
        private void HandleBuilding()
        {
            // Check resources first
            if (aiResources.wood < 150 || aiResources.stone < 100)
            {
                currentState = AIState.Gathering;
                return;
            }
            
            // Count building types
            int farmCount = 0;
            int lumbermillCount = 0;
            int barracksCount = 0;
            int archeryCount = 0;
            int stableCount = 0;
            int wallCount = 0;
            int wallCornerCount = 0;
            int towerCount = 0;
            
            foreach (GameObject building in enemyBuildings)
            {
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    string buildingType = buildingComponent.GetBuildingName().ToLower();
                    if (buildingType.Contains("farm"))
                        farmCount++;
                    else if (buildingType.Contains("lumber"))
                        lumbermillCount++;
                    else if (buildingType.Contains("barrack"))
                        barracksCount++;
                    else if (buildingType.Contains("archery"))
                        archeryCount++;
                    else if (buildingType.Contains("stable"))
                        stableCount++;
                    else if (buildingType.Contains("wall") && !buildingType.Contains("corner"))
                        wallCount++;
                    else if (buildingType.Contains("corner"))
                        wallCornerCount++;
                    else if (buildingType.Contains("tower"))
                        towerCount++;
                }
            }
            
            // Decide what to build based on priorities
            GameObject buildingToPlace = null;
            string buildingTypeName = "";
            
            // Priority 1: Essential production buildings (limit to 1 each)
            if (barracksCount == 0 && barracksPrefab != null)
            {
                buildingToPlace = barracksPrefab;
                buildingTypeName = "barracks";
            }
            else if (archeryCount == 0 && archeryPrefab != null)
            {
                buildingToPlace = archeryPrefab;
                buildingTypeName = "archery range";
            }
            else if (stableCount == 0 && stablePrefab != null)
            {
                buildingToPlace = stablePrefab;
                buildingTypeName = "stable";
            }
            // Priority 2: Resource buildings
            else if (farmCount < 2 && farmPrefab != null)
            {
                buildingToPlace = farmPrefab;
                buildingTypeName = "farm";
            }
            else if (lumbermillCount < 2 && lumbermillPrefab != null)
            {
                buildingToPlace = lumbermillPrefab;
                buildingTypeName = "lumbermill";
            }
            // Priority 3: Defenses
            else if (wallCount < 6 && wallPrefab != null)
            {
                buildingToPlace = wallPrefab;
                buildingTypeName = "wall";
            }
            else if (wallCornerCount < 4 && wallCornerPrefab != null)
            {
                buildingToPlace = wallCornerPrefab;
                buildingTypeName = "wall corner";
            }
            else if (towerCount < 2 && towerPrefab != null)
            {
                buildingToPlace = towerPrefab;
                buildingTypeName = "tower";
            }
            
            // Place the building
            if (buildingToPlace != null && enemyCastle != null)
            {
                // Find a position near the castle
                Vector3 castlePos = enemyCastle.transform.position;
                Vector3 offsetDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Vector3 buildingPos = castlePos + offsetDirection * Random.Range(10f, 30f);
                
                // Adjust height to terrain if needed
                RaycastHit hit;
                if (Physics.Raycast(buildingPos + Vector3.up * 100f, Vector3.down, out hit, 200f))
                {
                    buildingPos.y = hit.point.y;
                }
                  // Create the building
                GameObject newBuilding = Instantiate(buildingToPlace, buildingPos, Quaternion.identity);
                newBuilding.tag = "EnemyBuilding";
                
                // Register with BuildingManager if needed
                Building buildingComponent = newBuilding.GetComponent<Building>();
                if (BuildingManager.Instance != null && buildingComponent != null)
                {
                    BuildingManager.Instance.RegisterPlacedBuilding(buildingComponent);
                    
                    // Set building name for identification
                    if (!string.IsNullOrEmpty(buildingTypeName))
                    {
                        buildingComponent.name = $"Enemy {buildingTypeName}";
                    }
                }
                
                // Add to our tracking and deduct resources
                enemyBuildings.Add(newBuilding);
                aiResources.wood -= 150;
                aiResources.stone -= 100;
                
                Debug.Log($"AI built a {buildingTypeName}");
            }
        }

        /// <summary>
        /// Handle training units
        /// </summary>
        private void HandleTraining()
        {
            // Check resources first
            if (aiResources.food < 100 || aiResources.iron < 50)
            {
                currentState = AIState.Gathering;
                return;
            }
            
            // Count unit types
            int workerCount = 0;
            int infantryCount = 0;
            int archerCount = 0;
            int cavalryCount = 0;
            
            foreach (GameObject unit in enemyUnits)
            {
                Unit unitComponent = unit.GetComponent<Unit>();
                if (unitComponent != null)
                {
                    string unitType = unitComponent.GetUnitType().ToLower();
                    if (unitType.Contains("worker"))
                        workerCount++;
                    else if (unitType.Contains("infantry"))
                        infantryCount++;
                    else if (unitType.Contains("archer"))
                        archerCount++;
                    else if (unitType.Contains("cavalry"))
                        cavalryCount++;
                }
            }
            
            // Check for training buildings
            bool hasBarracks = false;
            bool hasArchery = false;
            bool hasStable = false;
            
            foreach (GameObject building in enemyBuildings)
            {
                Building buildingComponent = building.GetComponent<Building>();
                if (buildingComponent != null)
                {
                    string buildingType = buildingComponent.GetBuildingName().ToLower();
                    if (buildingType.Contains("barrack"))
                        hasBarracks = true;
                    else if (buildingType.Contains("archery"))
                        hasArchery = true;
                    else if (buildingType.Contains("stable"))
                        hasStable = true;
                }
            }
            
            // Decide what to train
            GameObject unitToTrain = null;
            string unitTypeName = "";
            
            // Priority 1: Always keep some workers
            if (workerCount < 3 && workerPrefab != null)
            {
                unitToTrain = workerPrefab;
                unitTypeName = "worker";
            }
            // Priority 2: Balanced military force
            else if (hasBarracks && infantryCount < 4 && infantryPrefab != null)
            {
                unitToTrain = infantryPrefab;
                unitTypeName = "infantry";
            }
            else if (hasArchery && archerCount < 3 && archerPrefab != null)
            {
                unitToTrain = archerPrefab;
                unitTypeName = "archer";
            }
            else if (hasStable && cavalryCount < 3 && cavalryPrefab != null)
            {
                unitToTrain = cavalryPrefab;
                unitTypeName = "cavalry";
            }
            
            // Train the unit
            if (unitToTrain != null && enemyCastle != null)
            {
                // Find position near castle to spawn unit
                Vector3 castlePos = enemyCastle.transform.position;
                Vector3 offsetDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Vector3 unitPos = castlePos + offsetDirection * Random.Range(5f, 10f);
                
                // Adjust height to terrain if needed
                RaycastHit hit;
                if (Physics.Raycast(unitPos + Vector3.up * 100f, Vector3.down, out hit, 200f))
                {
                    unitPos.y = hit.point.y;
                }
                  // Create the unit
                GameObject newUnit = Instantiate(unitToTrain, unitPos, Quaternion.identity);
                newUnit.tag = "EnemyUnit";
                
                // Set proper unit name
                Unit unitComponent = newUnit.GetComponent<Unit>();
                if (unitComponent != null && !string.IsNullOrEmpty(unitTypeName))
                {
                    newUnit.name = $"Enemy {unitTypeName}";
                }
                
                // Add to tracking and deduct resources
                enemyUnits.Add(newUnit);
                aiResources.food -= 100;
                aiResources.iron -= 50;
                
                Debug.Log($"AI trained a new {unitTypeName}");
            }
        }

        /// <summary>
        /// Handle combat situations when enemy encounters player units/buildings
        /// </summary>
        private void HandleCombat()
        {
            // Find all military units (non-workers)
            List<GameObject> militaryUnits = new List<GameObject>();
            
            foreach (GameObject unit in enemyUnits)
            {
                Unit unitComponent = unit.GetComponent<Unit>();
                if (unitComponent != null)
                {
                    string unitType = unitComponent.GetUnitType().ToLower();
                    if (!unitType.Contains("worker"))
                    {
                        militaryUnits.Add(unit);
                    }
                }
            }
            
            if (militaryUnits.Count < 3) // Require minimum units to attack
            {
                currentState = AIState.Training;
                return;
            }
            
            // First look for player units to attack
            GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
            if (playerUnits.Length > 0)
            {
                // Simple strategy: Attack the closest player unit
                GameObject target = GetClosestObject(enemyCastle.transform.position, playerUnits);
                if (target != null)
                {
                    // Send all military units to attack
                    foreach (GameObject unit in militaryUnits)
                    {
                        MoveUnitToTarget(unit, target.transform.position);
                    }
                    return;
                }
            }
            
            // If no player units found, attack buildings
            GameObject[] playerBuildings = GameObject.FindGameObjectsWithTag("Building");
            if (playerBuildings.Length > 0)
            {
                // Simple strategy: Attack a random player building
                GameObject target = playerBuildings[Random.Range(0, playerBuildings.Length)];
                
                // Send all military units to attack
                foreach (GameObject unit in militaryUnits)
                {
                    MoveUnitToTarget(unit, target.transform.position);
                }
            }
            else
            {
                // No targets found, go back to idle state
                currentState = AIState.Idle;
            }
        }
        
        /// <summary>
        /// Handle idle unit behavior - make them patrol around
        /// </summary>
        private void HandleIdleUnits()
        {
            // Find all military units (non-workers)
            List<GameObject> militaryUnits = new List<GameObject>();
            
            foreach (GameObject unit in enemyUnits)
            {
                Unit unitComponent = unit.GetComponent<Unit>();
                if (unitComponent != null && !unitComponent.GetUnitType().ToLower().Contains("worker"))
                {
                    militaryUnits.Add(unit);
                }
            }
            
            if (militaryUnits.Count == 0 || enemyCastle == null)
                return;
            
            // For each military unit, occasionally give them a new patrol point
            foreach (GameObject unit in militaryUnits)
            {
                // Only move some units (50% chance) to create a more natural idle behavior
                if (Random.value > 0.5f)
                {
                    // Create a random patrol point around the castle
                    Vector3 castlePos = enemyCastle.transform.position;
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    Vector3 patrolPoint = castlePos + randomDir * Random.Range(10f, patrolRadius);
                    
                    // Adjust height to terrain
                    RaycastHit hit;
                    if (Physics.Raycast(patrolPoint + Vector3.up * 100f, Vector3.down, out hit, 200f))
                    {
                        patrolPoint.y = hit.point.y;
                    }
                    
                    // Move the unit
                    MoveUnitToTarget(unit, patrolPoint);
                }
            }
            
            // Periodically check for enemies
            GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
            if (playerUnits.Length > 0)
            {
                // If player units are nearby, switch to combat
                GameObject closestUnit = GetClosestObject(enemyCastle.transform.position, playerUnits);
                if (closestUnit != null && 
                    Vector3.Distance(enemyCastle.transform.position, closestUnit.transform.position) < patrolRadius * 1.5f)
                {
                    currentState = AIState.Combat;
                }
            }
        }
        
        /// <summary>
        /// Helper method to move a unit to a target position
        /// </summary>
        private void MoveUnitToTarget(GameObject unit, Vector3 targetPosition)
        {
            if (unit == null)
                return;
                
            // Try to use UnitMovementController for movement
            UnitMovementController.AICommandMoveTo(unit, targetPosition);
            
            // Fallback to NavMeshAgent if needed
            UnityEngine.AI.NavMeshAgent agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(targetPosition);
            }
              // Make sure unit has a combat component for combat encounters
            CombatSystem combatSystem = unit.GetComponent<CombatSystem>();
            if (combatSystem == null)
            {
                combatSystem = unit.AddComponent<CombatSystem>();
            }
            
            // If target is a unit or building, set it as explicit attack target
            GameObject targetObject = null;
              // Try to find target by position
            Collider[] colliders = Physics.OverlapSphere(targetPosition, 2f);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("PlayerUnit") || collider.CompareTag("Building"))
                {
                    targetObject = collider.gameObject;
                    break;
                }
            }
            
            if (targetObject != null && combatSystem != null)
            {
                combatSystem.SetTarget(targetObject.transform);
            }
        }
        
        /// <summary>
        /// Helper method to find the closest object from a position
        /// </summary>
        private GameObject GetClosestObject(Vector3 fromPosition, GameObject[] objects)
        {
            if (objects.Length == 0)
                return null;
                
            GameObject closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (GameObject obj in objects)
            {
                if (obj == null)
                    continue;
                    
                float distance = Vector3.Distance(fromPosition, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj;
                }
            }
            
            return closest;
        }
    }
}
