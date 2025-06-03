using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Manages character creation and selection
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        // Singleton instance
        public static CharacterManager Instance { get; private set; }
        
        // Character prefabs/data references
        [SerializeField] private Sprite arvendirPortrait;
        [SerializeField] private Sprite lysaraPortrait;
        [SerializeField] private Sprite dravokPortrait;
        
        // Currently selected character
        private Character selectedCharacter;
        
        // List of available characters
        private List<Character> availableCharacters = new List<Character>();
        
        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize the character list
            InitializeCharacters();
        }
          /// <summary>
        /// Create commander instances with their abilities and kingdom bonuses
        /// </summary>
        private void InitializeCharacters()
        {            // Create Arvandir - The tactical specialist
            Character arvandir = new Character(
                "Arvandir",
                "Master Strategist",
                "Your trusted advisor in matters of stealth and strategy. Under his guidance, your troops excel in surprise attacks and guerrilla warfare tactics.",
                CharacterType.Arvandir,
                new KingdomBonuses(
                    0.05f,  // Wood production bonus: +5%
                    0.1f,   // Food production bonus: +10%
                    0.05f,  // Stone production bonus: +5%
                    0.05f,  // Iron production bonus: +5%
                    0.15f,  // Troop attack bonus: +15%
                    0.05f,  // Troop defense bonus: +5%
                    0.1f,   // Troop training speed bonus: +10%
                    0.0f,   // Building speed bonus: +0%
                    0.0f,   // Building health bonus: +0%
                    0.15f   // Research speed bonus: +15%
                ),
                new TacticalAmbush(),
                arvendirPortrait
            );
              // Create Lysara - The defensive specialist
            Character lysara = new Character(
                "Lysara",
                "Master Architect",
                "Your chief engineer and defensive advisor. With her architectural genius, your kingdom's walls and buildings will withstand any siege.",
                CharacterType.Lysara,
                new KingdomBonuses(
                    0.1f,   // Wood production bonus: +10%
                    0.0f,   // Food production bonus: +0%
                    0.2f,   // Stone production bonus: +20%
                    0.1f,   // Iron production bonus: +10%
                    0.0f,   // Troop attack bonus: +0%
                    0.2f,   // Troop defense bonus: +20%
                    0.05f,  // Troop training speed bonus: +5%
                    0.15f,  // Building speed bonus: +15%
                    0.25f,  // Building health bonus: +25%
                    0.05f   // Research speed bonus: +5%
                ),
                new FortifiedWalls(),
                lysaraPortrait
            );
              // Create Dravok - The offensive specialist
            Character dravok = new Character(
                "Dravok",
                "Battle Commander",
                "Your most fearsome general. With his leadership on the battlefield, your troops fight with unmatched ferocity and courage.",
                CharacterType.Dravok,
                new KingdomBonuses(
                    0.0f,   // Wood production bonus: +0%
                    0.15f,  // Food production bonus: +15%
                    0.05f,  // Stone production bonus: +5%
                    0.2f,   // Iron production bonus: +20%
                    0.25f,  // Troop attack bonus: +25%
                    0.1f,   // Troop defense bonus: +10%
                    0.15f,  // Troop training speed bonus: +15%
                    0.05f,  // Building speed bonus: +5%
                    0.0f,   // Building health bonus: +0%
                    0.05f   // Research speed bonus: +5%
                ),
                new WarCry(),
                dravokPortrait
            );
            
            // Add characters to the available list
            availableCharacters.Add(arvandir);
            availableCharacters.Add(lysara);
            availableCharacters.Add(dravok);
        }
        
        /// <summary>
        /// Get a list of all available characters
        /// </summary>
        public List<Character> GetAvailableCharacters()
        {
            return availableCharacters;
        }
        
        /// <summary>
        /// Select a character by type
        /// </summary>
        public Character SelectCharacter(CharacterType type)
        {
            selectedCharacter = availableCharacters.Find(c => c.characterType == type);
            if (selectedCharacter != null)
            {
                Debug.Log($"Selected character: {selectedCharacter.characterName}");
            }
            return selectedCharacter;
        }
        
        /// <summary>
        /// Get the currently selected character
        /// </summary>
        public Character GetSelectedCharacter()
        {
            return selectedCharacter;
        }
        
        /// <summary>
        /// Get character portrait by type
        /// </summary>
        public Sprite GetCharacterPortrait(CharacterType type)
        {
            switch (type)
            {
                case CharacterType.Arvandir:
                    return arvendirPortrait;
                case CharacterType.Lysara:
                    return lysaraPortrait;
                case CharacterType.Dravok:
                    return dravokPortrait;
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Get character by type
        /// </summary>
        public Character GetCharacter(CharacterType type)
        {
            return availableCharacters.Find(c => c.characterType == type);
        }
        
        /// <summary>
        /// Get all available characters
        /// </summary>
        public List<Character> GetAllCharacters()
        {
            return availableCharacters;
        }
        
        /// <summary>
        /// Set the selected character directly
        /// </summary>
        public void SetSelectedCharacter(Character character)
        {
            selectedCharacter = character;
            Debug.Log($"Selected character: {selectedCharacter.CharacterName}");
        }
    }
}
