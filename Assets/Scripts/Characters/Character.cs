using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Base class for kingdom commanders that provide passive bonuses and special abilities
    /// These commanders represent the strategic advisors to the player (who is the actual ruler)
    /// </summary>
    [System.Serializable]
    public class Character
    {
        public string characterName;
        public string title;
        public string description;
        public CharacterType characterType;
        
        // Kingdom-wide bonuses provided by this commander
        public KingdomBonuses kingdomBonuses;
        
        // Special ability
        public Ability specialAbility;
        
        // Character portrait/sprite
        [UnityEngine.SerializeField] private Sprite characterSprite;
        
        // Properties to access private fields
        public string CharacterName => characterName;
        public string Title => title;
        public string Description => description;
        public CharacterType CharacterType => characterType;
        public Sprite CharacterSprite => characterSprite;
          // Constructor
        public Character(string name, string title, string desc, CharacterType type, 
                         KingdomBonuses bonuses, Ability ability, Sprite sprite = null)
        {
            characterName = name;
            this.title = title;
            description = desc;
            characterType = type;
            kingdomBonuses = bonuses;
            specialAbility = ability;
            characterSprite = sprite;
        }
    }
    
    /// <summary>
    /// Represents the passive bonuses a commander provides to your kingdom
    /// </summary>
    [System.Serializable]
    public class KingdomBonuses
    {
        // Resource production bonuses
        public float woodProductionBonus;
        public float foodProductionBonus;
        public float stoneProductionBonus;
        public float ironProductionBonus;
        
        // Military bonuses
        public float troopAttackBonus;
        public float troopDefenseBonus;
        public float troopTrainingSpeedBonus;
        
        // Construction bonuses
        public float buildingSpeedBonus;
        public float buildingHealthBonus;
        
        // Research bonuses
        public float researchSpeedBonus;
        
        public KingdomBonuses(float woodBonus, float foodBonus, float stoneBonus, float ironBonus,
                             float attackBonus, float defenseBonus, float trainingBonus,
                             float buildSpeedBonus, float buildHealthBonus, float researchBonus)
        {
            woodProductionBonus = woodBonus;
            foodProductionBonus = foodBonus;
            stoneProductionBonus = stoneBonus;
            ironProductionBonus = ironBonus;
            
            troopAttackBonus = attackBonus;
            troopDefenseBonus = defenseBonus;
            troopTrainingSpeedBonus = trainingBonus;
            
            buildingSpeedBonus = buildSpeedBonus;
            buildingHealthBonus = buildHealthBonus;
            
            researchSpeedBonus = researchBonus;
        }
    }
    
    /// <summary>
    /// Enum for character types
    /// </summary>
    public enum CharacterType
    {
        Arvandir, // Tactical specialist
        Lysara,   // Defensive specialist
        Dravok    // Offensive specialist
    }
}
