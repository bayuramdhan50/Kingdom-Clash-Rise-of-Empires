using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{    /// <summary>
    /// War Cry battle tactic taught by Dravok's military expertise
    /// Boosts your troops' morale and intimidates enemy forces
    /// </summary>
    [System.Serializable]
    public class WarCry : Ability
    {
        public float warCryRadius = 25f;
        public float alliedAttackBoostPercent = 30f;
        public float alliedSpeedBoostPercent = 15f;
        public float enemySlowPercent = 20f;
        public float enemyDefenseReductionPercent = 15f;
        
        private float remainingActiveDuration = 0f;
        private List<GameObject> affectedAllies = new List<GameObject>();
        private List<GameObject> affectedEnemies = new List<GameObject>();
        
        public WarCry() : base(
            "War Cry", 
            "Under Dravok's inspiration, your troops raise a mighty battle cry that bolsters their courage while striking fear into enemy hearts. Allied units gain attack and speed bonuses while enemies suffer reduced coordination and defense.",
            120f, // 2 minutes cooldown
            15f)  // 15 seconds active duration
        {
        }
        
        public override bool Activate(Transform target = null)
        {
            if (base.Activate(target))
            {
                remainingActiveDuration = activeTime;
                Debug.Log($"War Cry activated! Allies empowered and enemies weakened for {activeTime} seconds.");
                
                // Apply effects to units in radius
                ApplyWarCryEffects(target);
                
                return true;
            }
            return false;
        }
        
        public override void UpdateAbility(float deltaTime)
        {
            base.UpdateAbility(deltaTime);
            
            if (isActive)
            {
                remainingActiveDuration -= deltaTime;
                
                if (remainingActiveDuration <= 0)
                {
                    // Remove effects when duration expires
                    RemoveWarCryEffects();
                    isActive = false;
                    Debug.Log("War Cry effects ended.");
                }
            }
        }
        
        private void ApplyWarCryEffects(Transform center)
        {
            if (center == null) return;
            
            // Clear previous lists
            affectedAllies.Clear();
            affectedEnemies.Clear();
            
            Debug.Log($"Applying War Cry effects in {warCryRadius} radius");
            Debug.Log($"Allied units get +{alliedAttackBoostPercent}% attack, +{alliedSpeedBoostPercent}% speed");
            Debug.Log($"Enemy units get -{enemySlowPercent}% speed, -{enemyDefenseReductionPercent}% defense");
            
            // Example code for finding and affecting units
            /*
            Collider[] hitColliders = Physics.OverlapSphere(center.position, warCryRadius);
            foreach (var hitCollider in hitColliders)
            {
                Unit unit = hitCollider.GetComponent<Unit>();
                if (unit != null)
                {
                    if (unit.IsPlayerUnit())
                    {
                        // Boost allied units
                        unit.ApplyAttackBoost(alliedAttackBoostPercent / 100f);
                        unit.ApplySpeedBoost(alliedSpeedBoostPercent / 100f);
                        affectedAllies.Add(hitCollider.gameObject);
                        
                        // Visual effect for allies
                        GameObject effect = Instantiate(allyBoostEffectPrefab, unit.transform);
                        Destroy(effect, activeTime);
                    }
                    else
                    {
                        // Weaken enemy units
                        unit.ApplySpeedReduction(enemySlowPercent / 100f);
                        unit.ApplyDefenseReduction(enemyDefenseReductionPercent / 100f);
                        affectedEnemies.Add(hitCollider.gameObject);
                        
                        // Visual effect for intimidated enemies
                        GameObject effect = Instantiate(enemyDebuffEffectPrefab, unit.transform);
                        Destroy(effect, activeTime);
                    }
                }
            }
            */
            
            // Play war cry sound effect
            // AudioSource.PlayClipAtPoint(warCrySound, center.position, 1.0f);
        }
        
        private void RemoveWarCryEffects()
        {
            // Remove effects from all affected units
            Debug.Log($"Removing War Cry effects from {affectedAllies.Count} allies and {affectedEnemies.Count} enemies");
            
            // Example code:
            /*
            foreach (var ally in affectedAllies)
            {
                if (ally != null)
                {
                    Unit unit = ally.GetComponent<Unit>();
                    if (unit != null)
                    {
                        unit.RemoveAttackBoost();
                        unit.RemoveSpeedBoost();
                    }
                }
            }
            
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null)
                {
                    Unit unit = enemy.GetComponent<Unit>();
                    if (unit != null)
                    {
                        unit.RemoveSpeedReduction();
                        unit.RemoveDefenseReduction();
                    }
                }
            }
            */
            
            affectedAllies.Clear();
            affectedEnemies.Clear();
        }
    }
}
