using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingdomClash
{
    /// <summary>
    /// Holds references to all UI elements in the settings panel
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Audio UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics UI")]
        [SerializeField] private TMP_Dropdown graphicsQualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;
        
        // Properties for external access
        public Slider MasterVolumeSlider => masterVolumeSlider;
        public Slider MusicVolumeSlider => musicVolumeSlider;
        public Slider SFXVolumeSlider => sfxVolumeSlider;
        public TMP_Dropdown GraphicsQualityDropdown => graphicsQualityDropdown;
        public Toggle FullscreenToggle => fullscreenToggle;
        public TMP_Dropdown ResolutionDropdown => resolutionDropdown;
        public Button CloseButton => closeButton;
        public Button SaveButton => saveButton;
        
        private void OnEnable()
        {
            // When panel is enabled, sync UI with current settings
            SyncWithSettingsManager();
        }
        
        /// <summary>
        /// Sync this panel's UI with the current settings in SettingsManager
        /// </summary>
        private void SyncWithSettingsManager()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SyncUIWithSettings(this);
                Debug.Log("Settings panel UI synced with SettingsManager");
            }
            else
            {
                Debug.LogError("Cannot sync settings panel: SettingsManager instance not found");
            }
        }
    }
}
