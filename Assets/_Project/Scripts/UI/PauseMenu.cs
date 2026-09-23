using MathDungeon.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// Pause and resume (SRS FR12). Pausing uses <see cref="GameState.Paused"/>,
    /// which the shared AllowsPlayerControl rule already treats as frozen, so the
    /// player stops exactly the way they do during a challenge. Resuming returns to
    /// the previous state without restarting anything.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitToMenuButton;

        [SerializeField] private SettingsMenu settingsMenu;

        private GameState stateBeforePause = GameState.Exploring;

        public bool IsPaused => GameManager.Instance != null
                                && GameManager.Instance.CurrentState == GameState.Paused;

        private void Awake()
        {
            if (settingsMenu == null)
            {
                settingsMenu = FindFirstObjectByType<SettingsMenu>();
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(QuitToMainMenu);

            SetVisible(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (IsPaused)
            {
                Resume();
            }
            else if (CanPause())
            {
                Pause();
            }
        }

        /// <summary>Pausing is only offered during ordinary play, not over a challenge or Game Over.</summary>
        private bool CanPause()
        {
            GameManager manager = GameManager.Instance;
            return manager != null && manager.CurrentState == GameState.Exploring;
        }

        public void Pause()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                return;
            }

            stateBeforePause = manager.CurrentState;
            manager.SetState(GameState.Paused);
            SetVisible(true);
        }

        public void Resume()
        {
            if (settingsMenu != null && settingsMenu.IsOpen)
            {
                settingsMenu.Close();
            }

            SetVisible(false);
            GameManager.Instance?.SetState(stateBeforePause);
        }

        private void OpenSettings()
        {
            if (settingsMenu == null)
            {
                Debug.LogWarning("[PauseMenu] No settings menu in the scene.", this);
                return;
            }

            // Hand the screen over to settings rather than stacking the two panels,
            // and take it back when settings closes.
            SetVisible(false);
            settingsMenu.Closed += OnSettingsClosed;
            settingsMenu.Open();
        }

        private void OnSettingsClosed()
        {
            settingsMenu.Closed -= OnSettingsClosed;

            // Only re-show if the player is still paused - Resume also closes settings.
            if (IsPaused)
            {
                SetVisible(true);
            }
        }

        /// <summary>
        /// Leaves for the main menu. Progress is already on disk - the save system
        /// writes on every milestone - so nothing is lost by quitting here.
        /// </summary>
        public void QuitToMainMenu()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.SaveGame(SaveTrigger.Manual);
            SetVisible(false);
            manager.LoadMainMenu();
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
