using MathDungeon.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// Audio, graphics and control options (SRS FR2 Settings Access, FR12 Settings
    /// Modification). Reachable from the main menu and from the pause menu, so the
    /// same panel serves both.
    ///
    /// Controls are shown rather than rebindable: the SRS asks for control
    /// preferences to be visible and adjustable, and live rebinding is a large
    /// feature the bare requirements do not call for.
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValue;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Controls")]
        [SerializeField] private TMP_Text controlsLabel;

        [SerializeField] private Button closeButton;

        /// <summary>Raised when the panel closes, so whatever opened it can come back.</summary>
        public event System.Action Closed;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            GameSettings.Apply();
            BuildQualityOptions();
            LoadCurrentValues();
            WireListeners();
            SetVisible(false);
        }

        private void BuildQualityOptions()
        {
            if (qualityDropdown == null)
            {
                return;
            }

            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        }

        private void LoadCurrentValues()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
            if (qualityDropdown != null) qualityDropdown.SetValueWithoutNotify(GameSettings.QualityLevel);
            if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);

            UpdateMasterLabel();

            if (controlsLabel != null)
            {
                controlsLabel.text =
                    "Move    W A S D\n" +
                    "Interact    E\n" +
                    "Pause    Esc\n" +
                    "Submit answer    Enter";
            }
        }

        private void WireListeners()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(v => { GameSettings.MasterVolume = v; UpdateMasterLabel(); });
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(v => GameSettings.SfxVolume = v);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(v => GameSettings.QualityLevel = v);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(v => GameSettings.Fullscreen = v);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        private void UpdateMasterLabel()
        {
            if (masterVolumeValue != null)
            {
                masterVolumeValue.text = Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%";
            }
        }

        public void Open()
        {
            LoadCurrentValues();
            SetVisible(true);
        }

        public void Close()
        {
            GameSettings.Save();
            SetVisible(false);
            Closed?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }
    }
}
