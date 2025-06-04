using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.SceneManagement;

namespace KingdomClash
{
    /// <summary>
    /// Manages game settings including audio, graphics, etc.
    /// Implementasi sebagai singleton yang tetap ada di semua scene.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        // Singleton instance
        public static SettingsManager Instance { get; private set; }
        
        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        
        // Cached settings values (persistent between scenes)
        private float masterVolume = 0.75f;
        private float musicVolume = 0.75f;
        private float sfxVolume = 0.75f;
        private int qualityLevel = 2; // Medium quality default
        private bool isFullscreen = true;
        private int resolutionIndex = 0;
        
        // Resolution options
        private Resolution[] resolutions;

        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                // Instance sudah ada, destroy duplicate
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Pastikan SettingsManager tidak di-destroy saat ganti scene
            DontDestroyOnLoad(gameObject);
            
            // Load settings at startup
            LoadSettingsFromPrefs();
            
            // Initialize resolution array
            resolutions = Screen.resolutions;
            
            // Apply initial settings
            ApplyAllSettings();
        }
        
        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        private void LoadSettingsFromPrefs()
        {
            // Load audio settings
            if (PlayerPrefs.HasKey("MasterVolume"))
                masterVolume = Mathf.Pow(10, PlayerPrefs.GetFloat("MasterVolume") / 20);
            
            if (PlayerPrefs.HasKey("MusicVolume"))
                musicVolume = Mathf.Pow(10, PlayerPrefs.GetFloat("MusicVolume") / 20);
            
            if (PlayerPrefs.HasKey("SFXVolume"))
                sfxVolume = Mathf.Pow(10, PlayerPrefs.GetFloat("SFXVolume") / 20);

            // Load graphics settings
            if (PlayerPrefs.HasKey("QualityLevel"))
                qualityLevel = PlayerPrefs.GetInt("QualityLevel");
            
            if (PlayerPrefs.HasKey("Fullscreen"))
                isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            
            if (PlayerPrefs.HasKey("ResolutionIndex"))
                resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
            
            Debug.Log("Settings loaded from PlayerPrefs");
        }
        
        /// <summary>
        /// Apply all cached settings to the system
        /// </summary>
        private void ApplyAllSettings()
        {
            // Apply audio settings
            if (audioMixer != null)
            {
                SetMasterVolume(masterVolume);
                SetMusicVolume(musicVolume);
                SetSFXVolume(sfxVolume);
            }
            
            // Apply graphics settings
            SetGraphicsQuality(qualityLevel);
            SetFullscreen(isFullscreen);
            
            // Apply resolution if resolutions array is initialized
            if (resolutions != null && resolutions.Length > 0 && resolutionIndex < resolutions.Length)
                SetResolution(resolutionIndex);
        }
        
        /// <summary>
        /// Sync UI elements with current settings values
        /// </summary>
        /// <param name="panel">Settings panel with UI elements</param>
        public void SyncUIWithSettings(SettingsPanel panel)
        {
            if (panel == null)
            {
                return;
            }
            
            // Sync audio sliders
            if (panel.MasterVolumeSlider != null)
            {
                panel.MasterVolumeSlider.value = masterVolume;
                panel.MasterVolumeSlider.onValueChanged.RemoveAllListeners();
                panel.MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            
            if (panel.MusicVolumeSlider != null)
            {
                panel.MusicVolumeSlider.value = musicVolume;
                panel.MusicVolumeSlider.onValueChanged.RemoveAllListeners();
                panel.MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            
            if (panel.SFXVolumeSlider != null)
            {
                panel.SFXVolumeSlider.value = sfxVolume;
                panel.SFXVolumeSlider.onValueChanged.RemoveAllListeners();
                panel.SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }
            
            // Sync graphics dropdowns and toggle
            if (panel.GraphicsQualityDropdown != null)
            {
                SetupGraphicsDropdown(panel.GraphicsQualityDropdown);
                panel.GraphicsQualityDropdown.value = qualityLevel;
                panel.GraphicsQualityDropdown.onValueChanged.RemoveAllListeners();
                panel.GraphicsQualityDropdown.onValueChanged.AddListener(SetGraphicsQuality);
            }
            
            if (panel.FullscreenToggle != null)
            {
                panel.FullscreenToggle.isOn = isFullscreen;
                panel.FullscreenToggle.onValueChanged.RemoveAllListeners();
                panel.FullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
            
            if (panel.ResolutionDropdown != null)
            {
                SetupResolutionDropdown(panel.ResolutionDropdown);
                panel.ResolutionDropdown.value = resolutionIndex;
                panel.ResolutionDropdown.onValueChanged.RemoveAllListeners();
                panel.ResolutionDropdown.onValueChanged.AddListener(SetResolution);
            }
            
            // Setup close button
            if (panel.CloseButton != null)
            {
                panel.CloseButton.onClick.RemoveAllListeners();
                panel.CloseButton.onClick.AddListener(() => CloseSettingsPanel(panel));
            }
            
            // Setup save button
            if (panel.SaveButton != null)
            {
                panel.SaveButton.onClick.RemoveAllListeners();
                panel.SaveButton.onClick.AddListener(SaveSettings);
            }
        }
        
        /// <summary>
        /// Setup the resolution dropdown with available options
        /// </summary>
        private void SetupResolutionDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null || resolutions == null) return;
            
            dropdown.ClearOptions();
            
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(new TMP_Dropdown.OptionData(option));
            }
            
            dropdown.AddOptions(options);
        }
        
        /// <summary>
        /// Setup the graphics quality dropdown
        /// </summary>
        private void SetupGraphicsDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null) return;
            
            dropdown.ClearOptions();
            
            System.Collections.Generic.List<TMP_Dropdown.OptionData> qualityOptions = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            string[] qualityNames = QualitySettings.names;
            
            for (int i = 0; i < qualityNames.Length; i++)
            {
                qualityOptions.Add(new TMP_Dropdown.OptionData(qualityNames[i]));
            }
            
            dropdown.AddOptions(qualityOptions);
        }
        
        /// <summary>
        /// Set the master volume level
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
            
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
            musicVolume = volume;
            
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
            sfxVolume = volume;
            
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
            qualityLevel = qualityIndex;
            QualitySettings.SetQualityLevel(qualityIndex);
        }
        
        /// <summary>
        /// Toggle fullscreen mode
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            isFullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
        }
        
        /// <summary>
        /// Set the screen resolution
        /// </summary>
        public void SetResolution(int index)
        {
            if (resolutions != null && index < resolutions.Length)
            {
                resolutionIndex = index;
                Resolution resolution = resolutions[index];
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
                float masterVolumeLog, musicVolumeLog, sfxVolumeLog;
                audioMixer.GetFloat("MasterVolume", out masterVolumeLog);
                audioMixer.GetFloat("MusicVolume", out musicVolumeLog);
                audioMixer.GetFloat("SFXVolume", out sfxVolumeLog);
                
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeLog);
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeLog);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeLog);
            }
            
            // Save graphics settings
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Close the settings panel and save settings
        /// </summary>
        public void CloseSettingsPanel(SettingsPanel panel)
        {
            // Always save settings when closing panel
            SaveSettings();
            
            if (panel != null && panel.gameObject != null)
            {
                panel.gameObject.SetActive(false);
                
                // Notify MainMenuUI to show main menu panel again
                MainMenuUI mainMenuUI = FindObjectOfType<MainMenuUI>();
                if (mainMenuUI != null)
                {
                    mainMenuUI.ShowMainMenu();
                }
            }
        }
    }
}
