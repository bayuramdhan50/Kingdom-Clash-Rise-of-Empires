using UnityEngine;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Script to attach to unit prefabs for handling unit-specific behavior
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [Header("Unit Stats")]
        [SerializeField] private string unitType = "infantry"; // Must match the type in UnitData
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int attack = 10;
        [SerializeField] private int defense = 10;
        
        private int currentHealth;
        
        private void Awake()
        {
            // Initialize health to max on spawn
            currentHealth = maxHealth;
        }
        
        /// <summary>
        /// Sets the max health for this unit
        /// </summary>
        public void SetMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            
            // Adjust current health if needed
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
        
        /// <summary>
        /// Sets the current health for this unit
        /// </summary>
        public void SetHealth(int value)
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            
            // Check if unit is dead
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Sets the attack value for this unit
        /// </summary>
        public void SetAttack(int value)
        {
            attack = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Sets the defense value for this unit
        /// </summary>
        public void SetDefense(int value)
        {
            defense = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Sets the unit type
        /// </summary>
        public void SetUnitType(string type)
        {
            unitType = type;
        }
        
        /// <summary>
        /// Gets the unit type
        /// </summary>
        public string GetUnitType()
        {
            return unitType;
        }
        
        /// <summary>
        /// Gets the current health
        /// </summary>
        public int GetHealth()
        {
            return currentHealth;
        }
        
        /// <summary>
        /// Gets the max health
        /// </summary>
        public int GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// Gets the attack value
        /// </summary>
        public int GetAttack()
        {
            return attack;
        }
        
        /// <summary>
        /// Gets the defense value
        /// </summary>
        public int GetDefense()
        {
            return defense;
        }
        
        /// <summary>
        /// Handles unit death
        /// </summary>
        private void Die()
        {
            Debug.Log($"Unit {unitType} has died");
            // Implement death behavior here (e.g., play animation, destroy after delay)
            Destroy(gameObject);
        }
    }
}
