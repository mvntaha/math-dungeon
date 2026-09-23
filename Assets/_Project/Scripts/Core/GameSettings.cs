using UnityEngine;
using UnityEngine.Audio;

namespace MathDungeon.Core
{
    /// <summary>
    /// Player-facing options (SRS FR12 Settings Modification). Values persist in
    /// PlayerPrefs rather than the save file, because they belong to the machine
    /// rather than to a profile - switching profile should not change your volume.
    /// </summary>
    public static class GameSettings
    {
        private const string MasterVolumeKey = "settings.volume.master";
        private const string MusicVolumeKey = "settings.volume.music";
        private const string SfxVolumeKey = "settings.volume.sfx";
        private const string QualityKey = "settings.graphics.quality";
        private const string FullscreenKey = "settings.graphics.fullscreen";

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            set
            {
                float clamped = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
                AudioListener.volume = clamped;
            }
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f);
            set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        }

        public static int QualityLevel
        {
            get => PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
            set
            {
                int clamped = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
                PlayerPrefs.SetInt(QualityKey, clamped);
                QualitySettings.SetQualityLevel(clamped, true);
            }
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            set
            {
                PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
                Screen.fullScreen = value;
            }
        }

        /// <summary>Pushes every stored setting into the engine. Call once on startup.</summary>
        public static void Apply()
        {
            AudioListener.volume = MasterVolume;
            QualitySettings.SetQualityLevel(QualityLevel, true);
            Screen.fullScreen = Fullscreen;
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
