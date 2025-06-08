using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Castle building class - extends Building class
    /// Represents the main castle/headquarters of a player or enemy
    /// </summary>
    public class CastleBuilding : Building
    {
        [Header("Castle Settings")]
        [SerializeField] private bool isPlayerCastle = true;
          private void Awake()
        {
            // Set appropriate tag based on owner
            if (isPlayerCastle)
            {
                gameObject.tag = "PlayerCastle";
                Debug.Log("Player Castle initialized");
            }
            else
            {
                gameObject.tag = "EnemyCastle";
                Debug.Log("Enemy Castle initialized");
            }
            
            // Castles have more health than normal buildings
            SetMaxHealth(10);
            SetHealth(10);
        }
        
        /// <summary>
        /// Gets whether this is a player castle or enemy castle
        /// </summary>
        public bool IsPlayerCastle()
        {
            return isPlayerCastle;
        }
          /// <summary>
        /// Handle castle destruction - this triggers game over
        /// </summary>
        private void OnDestroy()
        {
            // No base.OnDestroy() call needed as it doesn't exist in Building class
            
            // Notify GameConditionManager about castle destruction
            if (GameConditionManager.Instance != null)
            {
                if (isPlayerCastle)
                {
                    // Player castle was destroyed, player lost
                    GameConditionManager.Instance.EndGame(false);
                }
                else
                {
                    // Enemy castle was destroyed, player won
                    GameConditionManager.Instance.EndGame(true);
                }
            }
        }
    }
}
