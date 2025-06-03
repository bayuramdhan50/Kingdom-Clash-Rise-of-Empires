using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro; // Tambahkan TextMeshPro namespace

namespace KingdomClash
{
    /// <summary>
    /// Manages game settings including audio, graphics, etc.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown graphicsQualityDropdown; // Menggunakan TMP_Dropdown untuk TextMeshPro
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown; // Menggunakan TMP_Dropdown untuk TextMeshPro

        [Header("UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveSettingsButton;

        // Resolution options
        private Resolution[] resolutions;

        private void Start()
        {
            InitializeSettings();
            SetupEventListeners();
        }        /// <summary>
        /// Initialize UI elements with current settings
        /// </summary>
        private void InitializeSettings()
        {
            // Inisialisasi resolutions array terlebih dahulu
            resolutions = Screen.resolutions;
            
            // Setup resolution dropdown
            SetupResolutionDropdown();

            // Setup graphics quality dropdown
            SetupGraphicsDropdown();
            
            // Load saved settings if they exist
            LoadSettings();

            // Initialize fullscreen toggle
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
            }
        }

        /// <summary>
        /// Setup event listeners for UI elements
        /// </summary>
        private void SetupEventListeners()
        {
            // Audio sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

            // Graphics settings
            if (graphicsQualityDropdown != null)
                graphicsQualityDropdown.onValueChanged.AddListener(SetGraphicsQuality);
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.AddListener(SetResolution);

            // Buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSettingsPanel);
            
            if (saveSettingsButton != null)
                saveSettingsButton.onClick.AddListener(SaveSettings);
        }        /// <summary>
        /// Setup the resolution dropdown with available options
        /// </summary>
        private void SetupResolutionDropdown()
        {
            if (resolutionDropdown == null) return;
            
            // resolutions sudah diinisialisasi di InitializeSettings()
            resolutionDropdown.ClearOptions();

            int currentResolutionIndex = 0;
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(new TMP_Dropdown.OptionData(option));

                if (resolutions[i].width == Screen.currentResolution.width && 
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Setup the graphics quality dropdown
        /// </summary>
        private void SetupGraphicsDropdown()
        {
            if (graphicsQualityDropdown == null) return;

            graphicsQualityDropdown.ClearOptions();
            
            System.Collections.Generic.List<TMP_Dropdown.OptionData> qualityOptions = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            string[] qualityNames = QualitySettings.names;
            
            for (int i = 0; i < qualityNames.Length; i++)
            {
                qualityOptions.Add(new TMP_Dropdown.OptionData(qualityNames[i]));
            }

            graphicsQualityDropdown.AddOptions(qualityOptions);
            graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();
            graphicsQualityDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Set the master volume level
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
            }
        }

        /// <summary>
        /// Set the music volume level
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
            }
        }

        /// <summary>
        /// Set the SFX volume level
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
            }
        }

        /// <summary>
        /// Set the graphics quality level
        /// </summary>
        public void SetGraphicsQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        /// <summary>
        /// Toggle fullscreen mode
        /// </summary>
        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }        /// <summary>
        /// Set the screen resolution
        /// </summary>
        public void SetResolution(int resolutionIndex)
        {
            if (resolutions != null && resolutionIndex < resolutions.Length)
            {
                Resolution resolution = resolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            }
        }

        /// <summary>
        /// Save current settings to PlayerPrefs
        /// </summary>
        public void SaveSettings()
        {
            // Save audio settings
            if (audioMixer != null)
            {
                float masterVolume, musicVolume, sfxVolume;
                audioMixer.GetFloat("MasterVolume", out masterVolume);
                audioMixer.GetFloat("MusicVolume", out musicVolume);
                audioMixer.GetFloat("SFXVolume", out sfxVolume);
                
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            }

            // Save graphics settings
            PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
            
            if (resolutionDropdown != null)
            {
                PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
            }

            PlayerPrefs.Save();
            Debug.Log("Settings saved successfully");
        }

        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        private void LoadSettings()
        {
            // Load audio settings
            if (audioMixer != null)
            {
                if (PlayerPrefs.HasKey("MasterVolume"))
                {
                    float masterVolume = PlayerPrefs.GetFloat("MasterVolume");
                    audioMixer.SetFloat("MasterVolume", masterVolume);
                    if (masterVolumeSlider != null)
                        masterVolumeSlider.value = Mathf.Pow(10, masterVolume / 20);
                }

                if (PlayerPrefs.HasKey("MusicVolume"))
                {
                    float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
                    audioMixer.SetFloat("MusicVolume", musicVolume);
                    if (musicVolumeSlider != null)
                        musicVolumeSlider.value = Mathf.Pow(10, musicVolume / 20);
                }

                if (PlayerPrefs.HasKey("SFXVolume"))
                {
                    float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
                    audioMixer.SetFloat("SFXVolume", sfxVolume);
                    if (sfxVolumeSlider != null)
                        sfxVolumeSlider.value = Mathf.Pow(10, sfxVolume / 20);
                }
            }

            // Load graphics settings
            if (PlayerPrefs.HasKey("QualityLevel"))
            {
                int qualityLevel = PlayerPrefs.GetInt("QualityLevel");
                QualitySettings.SetQualityLevel(qualityLevel);
                if (graphicsQualityDropdown != null)
                    graphicsQualityDropdown.value = qualityLevel;
            }

            if (PlayerPrefs.HasKey("Fullscreen"))
            {
                bool isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
                Screen.fullScreen = isFullscreen;
                if (fullscreenToggle != null)
                    fullscreenToggle.isOn = isFullscreen;
            }            
            if (PlayerPrefs.HasKey("ResolutionIndex") && resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
            {
                int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
                if (resolutionIndex < resolutions.Length)
                {
                    resolutionDropdown.value = resolutionIndex;
                    Resolution resolution = resolutions[resolutionIndex];
                    Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                }
            }

            Debug.Log("Settings loaded successfully");
        }

        /// <summary>
        /// Close the settings panel
        /// </summary>
        public void CloseSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
    }
}
