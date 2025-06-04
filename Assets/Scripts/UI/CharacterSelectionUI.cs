using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using KingdomClash.Characters;

namespace KingdomClash
{
    /// <summary>
    /// Handles the Character Selection UI interactions and logic
    /// </summary>
    public class CharacterSelectionUI : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterUISlot
        {
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI typeText;
            public Button detailButton;
            public Button slotButton;
            public Image characterImage;
            public GameObject selectionIndicator; // Indikator ketika slot dipilih (misalnya border yang menyala)
        }

        [Header("Character UI Slots")]
        [SerializeField] private CharacterUISlot[] characterSlots;

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button startButton; // Tombol Start untuk memulai game dengan karakter yang dipilih
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameScene";
        
        [Header("Character Detail Panel")]
        [SerializeField] private GameObject characterDetailPanel;
        [SerializeField] private Button closeDetailButton;
        [SerializeField] private TextMeshProUGUI detailCharacterNameText;
        [SerializeField] private TextMeshProUGUI detailCharacterTitleText;
        [SerializeField] private TextMeshProUGUI detailCharacterDescriptionText;
        [SerializeField] private TextMeshProUGUI kingdomBonusesText;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private Image detailCharacterImage;

        // Reference to character manager to get character data
        private CharacterManager characterManager;
        private int selectedCharacterIndex = -1; // Indeks karakter yang dipilih, -1 berarti belum ada yang dipilih

    private void Start()
        {
            // Get reference to CharacterManager
            characterManager = CharacterManager.Instance;
            
            if (characterManager == null)
            {
                Debug.LogError("CharacterManager instance not found!");
            }
            
            InitializeButtons();
            PopulateCharacterSlots();
            
            // Sembunyikan panel detail karakter saat awal
            if (characterDetailPanel != null)
            {
                characterDetailPanel.SetActive(false);
            }
        }/// <summary>
        /// Initialize all button click listeners
        /// </summary>
        private void InitializeButtons()
        {
            // Back button setup
            if (backButton != null)
                backButton.onClick.AddListener(ReturnToMainMenu);
              // Start button setup
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartGameWithSelectedCharacter);
                // Tombol start awalnya dinonaktifkan sampai karakter dipilih
                startButton.interactable = false;
            }
            
            // Close detail panel button setup
            if (closeDetailButton != null)
            {
                closeDetailButton.onClick.AddListener(CloseCharacterDetailPanel);
            }

            // Setup detail and slot buttons for each character slot
            for (int i = 0; i < characterSlots.Length; i++)
            {
                if (characterSlots[i].detailButton != null)
                {
                    int index = i; // Store the index for lambda capture
                    characterSlots[i].detailButton.onClick.AddListener(() => ShowCharacterDetails(index));
                }

                if (characterSlots[i].slotButton != null)
                {
                    int index = i; // Store the index for lambda capture
                    characterSlots[i].slotButton.onClick.AddListener(() => SelectCharacterSlot(index));
                }
                
                // Nonaktifkan semua indikator pemilihan di awal
                if (characterSlots[i].selectionIndicator != null)
                {
                    characterSlots[i].selectionIndicator.SetActive(false);
                }
            }
        }        /// <summary>
        /// Populates the character slots with data from CharacterManager
        /// </summary>
        private void PopulateCharacterSlots()
        {
            if (characterManager == null)
                return;

            var characters = characterManager.GetAllCharacters();
            
            // Make sure we don't exceed the available character slots
            int count = Mathf.Min(characters.Count, characterSlots.Length);
            
            for (int i = 0; i < count; i++)
            {
                Character character = characters[i];
                
                if (characterSlots[i].nameText != null)
                    characterSlots[i].nameText.text = character.CharacterName;
                    
                if (characterSlots[i].typeText != null)
                    characterSlots[i].typeText.text = character.Title; // Menggunakan title sebagai tipe yang ditampilkan
                    
                if (characterSlots[i].characterImage != null && character.CharacterSprite != null)
                    characterSlots[i].characterImage.sprite = character.CharacterSprite;
            }
        }        /// <summary>
        /// Menampilkan informasi detail tentang karakter yang dipilih
        /// </summary>
        /// <param name="characterIndex">Indeks karakter dalam array characterSlots</param>
        public void ShowCharacterDetails(int characterIndex)
        {
            if (characterManager == null || characterDetailPanel == null)
                return;
                
            var characters = characterManager.GetAllCharacters();
            
            if (characterIndex >= 0 && characterIndex < characters.Count)
            {
                Character character = characters[characterIndex];
                
                // Isi konten panel dengan detail karakter
                if (detailCharacterNameText != null)
                    detailCharacterNameText.text = character.CharacterName;
                    
                if (detailCharacterTitleText != null)
                    detailCharacterTitleText.text = character.Title;
                    
                if (detailCharacterDescriptionText != null)
                    detailCharacterDescriptionText.text = character.Description;
                
                if (detailCharacterImage != null && character.CharacterSprite != null)
                    detailCharacterImage.sprite = character.CharacterSprite;
                
                // Isi informasi Kingdom Bonuses
                if (kingdomBonusesText != null && character.kingdomBonuses != null)
                {
                    kingdomBonusesText.text = FormatKingdomBonuses(character.kingdomBonuses);
                }
                
                // Isi informasi Ability
                if (abilityNameText != null && character.specialAbility != null)
                {
                    abilityNameText.text = character.specialAbility.GetName();
                }
                
                if (abilityDescriptionText != null && character.specialAbility != null)
                {
                    abilityDescriptionText.text = character.specialAbility.GetDescription();
                }
                
                // Tampilkan panel detail
                characterDetailPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Memilih slot karakter dan menampilkan indikator pemilihan
        /// </summary>
        /// <param name="characterIndex">Indeks karakter yang dipilih</param>
        public void SelectCharacterSlot(int characterIndex)
        {
            if (characterManager == null)
                return;
                
            var characters = characterManager.GetAllCharacters();
            
            if (characterIndex >= 0 && characterIndex < characters.Count)
            {
                // Reset indikator pemilihan untuk semua slot
                for (int i = 0; i < characterSlots.Length; i++)
                {
                    if (characterSlots[i].selectionIndicator != null)
                    {
                        characterSlots[i].selectionIndicator.SetActive(i == characterIndex);
                    }
                }
                
                // Simpan indeks karakter yang dipilih
                selectedCharacterIndex = characterIndex;
                
                // Aktifkan tombol start karena sudah ada karakter yang dipilih
                if (startButton != null)
                {
                    startButton.interactable = true;
                }
                
                Debug.Log($"Karakter dipilih: {characters[characterIndex].CharacterName}");
            }
        }
        
        /// <summary>
        /// Memulai game dengan karakter yang dipilih
        /// </summary>
        public void StartGameWithSelectedCharacter()
        {
            if (characterManager == null || selectedCharacterIndex < 0)
                return;
                
            var characters = characterManager.GetAllCharacters();
            
            if (selectedCharacterIndex < characters.Count)
            {
                // Simpan karakter yang dipilih ke CharacterManager
                characterManager.SetSelectedCharacter(characters[selectedCharacterIndex]);
                
                Debug.Log($"Memulai game dengan karakter: {characters[selectedCharacterIndex].CharacterName}");
                
                // Load scene game
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogError("Indeks karakter yang dipilih tidak valid");
            }
        }        /// <summary>
        /// Metode lama untuk memilih karakter (dipertahankan untuk kompatibilitas)
        /// </summary>
        /// <param name="characterIndex">Indeks karakter dalam array characterSlots</param>
        public void SelectCharacter(int characterIndex)
        {
            // Hanya memanggil metode baru
            SelectCharacterSlot(characterIndex);
        }

        /// <summary>
        /// Returns to the main menu scene
        /// </summary>
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Menutup panel detail karakter
        /// </summary>
        public void CloseCharacterDetailPanel()
        {
            if (characterDetailPanel != null)
            {
                characterDetailPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Format kingdom bonuses menjadi teks yang mudah dibaca
        /// </summary>
        private string FormatKingdomBonuses(KingdomBonuses bonuses)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Resource production bonuses
            if (bonuses.woodProductionBonus > 0)
                sb.AppendLine($"• Produksi Kayu: +{bonuses.woodProductionBonus * 100}%");
                
            if (bonuses.foodProductionBonus > 0)
                sb.AppendLine($"• Produksi Makanan: +{bonuses.foodProductionBonus * 100}%");
                
            if (bonuses.stoneProductionBonus > 0)
                sb.AppendLine($"• Produksi Batu: +{bonuses.stoneProductionBonus * 100}%");
                
            if (bonuses.ironProductionBonus > 0)
                sb.AppendLine($"• Produksi Besi: +{bonuses.ironProductionBonus * 100}%");
            
            // Military bonuses
            if (bonuses.troopAttackBonus > 0)
                sb.AppendLine($"• Serangan Pasukan: +{bonuses.troopAttackBonus * 100}%");
                
            if (bonuses.troopDefenseBonus > 0)
                sb.AppendLine($"• Pertahanan Pasukan: +{bonuses.troopDefenseBonus * 100}%");
                
            if (bonuses.troopTrainingSpeedBonus > 0)
                sb.AppendLine($"• Kecepatan Pelatihan: +{bonuses.troopTrainingSpeedBonus * 100}%");
            
            // Construction bonuses
            if (bonuses.buildingSpeedBonus > 0)
                sb.AppendLine($"• Kecepatan Konstruksi: +{bonuses.buildingSpeedBonus * 100}%");
                
            if (bonuses.buildingHealthBonus > 0)
                sb.AppendLine($"• Ketahanan Bangunan: +{bonuses.buildingHealthBonus * 100}%");
            
            // Research bonuses
            if (bonuses.researchSpeedBonus > 0)
                sb.AppendLine($"• Kecepatan Riset: +{bonuses.researchSpeedBonus * 100}%");
                
            return sb.ToString();
        }
    }
}
