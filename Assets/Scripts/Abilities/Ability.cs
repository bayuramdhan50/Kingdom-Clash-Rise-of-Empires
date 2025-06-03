using UnityEngine;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Base class for all abilities in the game
    /// </summary>
    [System.Serializable]
    public abstract class Ability
    {
        public string abilityName;
        public string description;
        public float cooldownTime;
        public float activeTime;
        
        protected float currentCooldown = 0f;
        protected bool isActive = false;
        
        public Ability(string name, string description, float cooldown, float activeTime)
        {
            this.abilityName = name;
            this.description = description;
            this.cooldownTime = cooldown;
            this.activeTime = activeTime;
        }
        
        /// <summary>
        /// Activates the ability if not on cooldown
        /// </summary>
        /// <returns>True if activated, false otherwise</returns>
        public virtual bool Activate(Transform target = null)
        {
            if (currentCooldown <= 0f)
            {
                isActive = true;
                currentCooldown = cooldownTime;
                Debug.Log($"Activated ability: {abilityName}");
                return true;
            }
            
            Debug.Log($"Ability {abilityName} is on cooldown: {currentCooldown}s remaining");
            return false;
        }
        
        /// <summary>
        /// Updates the ability state (cooldowns, effects, etc.)
        /// </summary>
        public virtual void UpdateAbility(float deltaTime)
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= deltaTime;
            }
        }
        
        /// <summary>
        /// Checks if the ability is ready to use
        /// </summary>
        public bool IsReady()
        {
            return currentCooldown <= 0f;
        }
        
        /// <summary>
        /// Gets the remaining cooldown as percentage (0-1)
        /// </summary>
        public float GetCooldownPercent()
        {
            return Mathf.Clamp01(currentCooldown / cooldownTime);
        }
        
        /// <summary>
        /// Gets the ability name
        /// </summary>
        public string GetName()
        {
            return abilityName;
        }
        
        /// <summary>
        /// Gets the ability description
        /// </summary>
        public string GetDescription()
        {
            return description;
        }
    }
}
