using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{    /// <summary>
    /// Tactical Ambush strategy provided by Arvandir's advice
    /// Allows your troops to employ stealth tactics and gain damage boost in a specific area
    /// </summary>
    [System.Serializable]
    public class TacticalAmbush : Ability
    {
        public float ambushRadius = 15f;
        public float damageMultiplier = 1.5f;
        public float stealthDuration = 10f;
        
        private float remainingActiveDuration = 0f;
        
        public TacticalAmbush() : base(
            "Tactical Ambush", 
            "On Arvandir's strategic advice, your troops employ advanced stealth tactics in the targeted area, becoming concealed from enemy sight and gaining increased damage for their first attack.",
            180f, // 3 minutes cooldown
            10f)  // 10 seconds active duration
        {
        }
        
        public override bool Activate(Transform target = null)
        {
            if (base.Activate(target))
            {
                // Implementation for making units invisible and boosting damage
                remainingActiveDuration = activeTime;
                Debug.Log($"Tactical Ambush activated! Units are now concealed for {activeTime} seconds.");
                
                // Here we would find all friendly units in the ambush radius and apply effects
                ApplyAmbushEffects(target);
                
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
                    // Remove stealth effects when duration expires
                    RemoveAmbushEffects();
                    isActive = false;
                    Debug.Log("Tactical Ambush effects ended.");
                }
            }
        }
        
        private void ApplyAmbushEffects(Transform center)
        {
            if (center == null) return;
            
            // In an actual implementation, we would use Physics.OverlapSphere or similar
            // to find all units in the radius and apply stealth + damage boost
            Debug.Log($"Applying stealth and {damageMultiplier}x damage boost to units in {ambushRadius} radius");
            
            // Example code for finding units (would depend on how your units are tagged/layered)
            /*
            Collider[] hitColliders = Physics.OverlapSphere(center.position, ambushRadius);
            foreach (var hitCollider in hitColliders)
            {
                Unit unit = hitCollider.GetComponent<Unit>();
                if (unit != null && unit.IsPlayerUnit())
                {
                    unit.ApplyStealth(stealthDuration);
                    unit.ApplyDamageBoost(damageMultiplier);
                }
            }
            */
        }
        
        private void RemoveAmbushEffects()
        {
            // Code to remove effects when ability duration expires
            // Would iterate through affected units and clear buffs
        }
    }
}
