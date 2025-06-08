using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KingdomClash.Characters
{    /// <summary>
    /// Simple combat system for handling combat between player and enemy units and buildings
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 5f; // Range at which units can detect enemies
        [SerializeField] private float attackCooldown = 2f; // Time between attacks
          [Header("Unit Settings")]
        [SerializeField] private string unitTag = "PlayerUnit"; // Tag for this unit (PlayerUnit or EnemyUnit)
        [SerializeField] private string enemyTag = "EnemyUnit"; // Tag for enemy units
        [SerializeField] private string enemyBuildingTag = "EnemyBuilding"; // Tag for enemy buildings
        
        private Unit unitComponent;
        private bool canAttack = true;
        private Transform currentTarget; // Can be either unit or building
        private bool isTargetBuilding = false; // Whether current target is a building
          private void Start()
        {
            unitComponent = GetComponent<Unit>();
            
            // If this is attached to a player unit, enemy tag should be "EnemyUnit", and vice versa
            if (gameObject.CompareTag("PlayerUnit"))
            {
                enemyTag = "EnemyUnit";
                enemyBuildingTag = "EnemyBuilding";
                unitTag = "PlayerUnit";
            }
            else if (gameObject.CompareTag("EnemyUnit"))
            {
                enemyTag = "PlayerUnit";
                enemyBuildingTag = "Building";
                unitTag = "EnemyUnit";
            }
            
            // Start scanning for enemies
            StartCoroutine(ScanForEnemies());
        }
          /// <summary>
        /// Continuously scan for nearby enemies (both units and buildings)
        /// </summary>
        private IEnumerator ScanForEnemies()
        {
            while (true)
            {
                // Find all enemy units
                GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag(enemyTag);
                
                // Find all enemy buildings
                GameObject[] enemyBuildings = GameObject.FindGameObjectsWithTag(enemyBuildingTag);
                
                // Find the closest target within attack range
                float closestDistance = attackRange;
                Transform closestTarget = null;
                bool isBuilding = false;
                
                // Check enemy units first
                foreach (GameObject enemy in enemyUnits)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = enemy.transform;
                        isBuilding = false;
                    }
                }
                
                // Check enemy buildings
                foreach (GameObject building in enemyBuildings)
                {
                    float distance = Vector3.Distance(transform.position, building.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = building.transform;
                        isBuilding = true;
                    }
                }
                
                // If we found a close target, target it
                if (closestTarget != null)
                {
                    currentTarget = closestTarget;
                    isTargetBuilding = isBuilding;
                    
                    // If we can attack, do so
                    if (canAttack)
                    {
                        StartCoroutine(AttackTarget(currentTarget, isTargetBuilding));
                    }
                }
                else
                {
                    currentTarget = null;
                }
                
                // Scan every 0.5 seconds
                yield return new WaitForSeconds(0.5f);
            }
        }
          /// <summary>
        /// Attack the targeted enemy (unit or building)
        /// </summary>
        private IEnumerator AttackTarget(Transform target, bool isBuilding)
        {
            if (target == null || unitComponent == null)
                yield break;
                
            // Can't attack again until cooldown is over
            canAttack = false;
            
            // Face the target
            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // Calculate base damage
            int attackValue = unitComponent.GetAttack();
            int damage = attackValue;
            
            if (isBuilding)
            {
                // Attack a building
                Building building = target.GetComponent<Building>();
                if (building != null)
                {
                    // Buildings typically have high defense, so just apply full damage
                    // or reduce it slightly for balance
                    damage = Mathf.Max(1, (int)(attackValue * 0.8f)); // 80% damage to buildings
                    
                    // Deal damage to the building
                    building.TakeDamage(damage);
                    
                    // Log the attack for debugging
                    Debug.Log($"{gameObject.name} ({unitTag}) attacked building {target.name} for {damage} damage!");
                }
            }
            else
            {
                // Attack a unit
                Unit enemyUnit = target.GetComponent<Unit>();
                if (enemyUnit != null)
                {
                    // Calculate damage (attack value - defense value, minimum 1)
                    int defenseValue = enemyUnit.GetDefense();
                    damage = Mathf.Max(1, attackValue - defenseValue);
                    
                    // Deal damage to the enemy
                    int currentHealth = enemyUnit.GetHealth();
                    enemyUnit.SetHealth(currentHealth - damage);
                    
                    // Log the attack for debugging
                    Debug.Log($"{gameObject.name} ({unitTag}) attacked unit {target.name} for {damage} damage!");
                }
            }
            
            // Play attack animation or sound effect here
            
            // Wait for cooldown
            yield return new WaitForSeconds(attackCooldown);
            
            // Ready to attack again
            canAttack = true;
        }
        
        /// <summary>
        /// Set a specific target for this unit to attack
        /// </summary>
        /// <param name="targetTransform">The transform of the target to attack</param>
        public void SetTarget(Transform targetTransform)
        {
            if (targetTransform == null) return;
            
            // Determine if it's a building or unit
            bool targetIsBuilding = targetTransform.CompareTag(enemyBuildingTag);
            
            // Set as current target
            currentTarget = targetTransform;
            isTargetBuilding = targetIsBuilding;
            
            // Attack immediately if able
            if (canAttack)
            {
                StartCoroutine(AttackTarget(currentTarget, isTargetBuilding));
            }
            
            Debug.Log($"Unit {gameObject.name} targeting {targetTransform.name} (Building: {targetIsBuilding})");
        }
        
        /// <summary>
        /// Draw attack range gizmo (for debugging)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
