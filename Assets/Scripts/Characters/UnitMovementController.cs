using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Handles unit selection and movement commands
    /// </summary>
    public class UnitMovementController : MonoBehaviour
    {
        private List<Unit> selectedUnits = new List<Unit>();
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleUnitSelection();
            HandleMovementCommand();
        }

        private void HandleUnitSelection()
        {
            // Single unit selection with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Unit unit = hit.collider.GetComponent<Unit>();
                    if (unit != null)
                    {
                        // If not holding shift, deselect all units first
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            DeselectAllUnits();
                        }
                        
                        SelectUnit(unit);
                    }
                    else if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        // If clicking empty space and not holding shift, deselect all units
                        DeselectAllUnits();
                    }
                }
            }
        }

        private void HandleMovementCommand()
        {
            // Move selected units with right click
            if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {                // Check if clicked on an enemy unit, building or castle
                if (hit.collider.CompareTag("EnemyUnit") || 
                    hit.collider.CompareTag("EnemyBuilding") || 
                    hit.collider.CompareTag("EnemyCastle"))
                {
                    Transform attackTarget = hit.collider.transform;
                    
                    // Attack the enemy or building
                    foreach (Unit unit in selectedUnits)
                    {
                        // Move to target position, slightly closer for melee attack
                        Vector3 movePosition = hit.point;
                        if (unit.GetUnitType().ToLower().Contains("infantry") || 
                            unit.GetUnitType().ToLower().Contains("cavalry"))
                        {
                            // For melee units, get closer to the target
                            Vector3 direction = (movePosition - unit.transform.position).normalized;
                            movePosition = hit.point - (direction * 2f); // Stay 2 units away from target
                        }
                        unit.MoveTo(movePosition);
                        
                        // Add combat component if not already present
                        CombatSystem combatSystem = unit.GetComponent<CombatSystem>();
                        if (combatSystem == null)
                        {
                            combatSystem = unit.gameObject.AddComponent<CombatSystem>();
                        }
                        
                        // Explicitly set the target to attack
                        combatSystem.SetTarget(attackTarget);
                    }
                    
                    Debug.Log($"Units ordered to attack {hit.collider.gameObject.name}");
                }
                    else
                    {
                        // Move all selected units to the clicked position
                        foreach (Unit unit in selectedUnits)
                        {
                            unit.MoveTo(hit.point);
                        }
                    }
                }
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }

        private void DeselectAllUnits()
        {
            foreach (Unit unit in selectedUnits)
            {
                unit.Deselect();
            }
            selectedUnits.Clear();
        }        /// <summary>
        /// AI command to move a unit to a specific location (used by SimpleAI)
        /// This method is static to allow calling it from outside without needing a reference to this controller
        /// </summary>
        /// <param name="unit">The unit to command</param>
        /// <param name="position">Target position to move to</param>
        public static void AICommandMoveTo(GameObject unitObject, Vector3 position)
        {
            // This is a simplified method for AI use
            // In a more complex implementation, you might have specialized AI movement controls
            // that differ from player controls
            
            if (unitObject != null)
            {
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.MoveTo(position);
                }
                  // Add combat component if not already present (for automatic enemy detection during movement)
                CombatSystem combatSystem = unitObject.GetComponent<CombatSystem>();
                if (combatSystem == null)
                {
                    combatSystem = unitObject.AddComponent<CombatSystem>();
                }
                
                // Check if there are attack targets at the destination
                Collider[] colliders = Physics.OverlapSphere(position, 2f);
                foreach (Collider collider in colliders)
                {
                    bool isEnemy = false;
                      // Check if this is an AI unit (enemy to player)
                    if (unitObject.CompareTag("EnemyUnit") && 
                        (collider.CompareTag("PlayerUnit") || 
                         collider.CompareTag("Building") || 
                         collider.CompareTag("PlayerCastle")))
                    {
                        isEnemy = true;
                    }
                    // Check if this is a player unit (enemy to AI)
                    else if (unitObject.CompareTag("PlayerUnit") && 
                            (collider.CompareTag("EnemyUnit") || 
                             collider.CompareTag("EnemyBuilding") || 
                             collider.CompareTag("EnemyCastle")))
                    {
                        isEnemy = true;
                    }
                    
                    // If we found an enemy target, set it
                    if (isEnemy && combatSystem != null)
                    {
                        combatSystem.SetTarget(collider.transform);
                        break;
                    }
                }
            }
        }
    }
}
