using MathDungeon.Core;
using MathDungeon.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// The central navigation hub (SRS FR2 Main Menu Navigation): New Game,
    /// Continue, Settings and Exit. This is what finally owns profile creation and
    /// loading, replacing the test-scene bootstrap used up to Milestone 6.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TMP_Text saveInfoLabel;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("New game")]
        [SerializeField] private GameObject newGamePanel;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button confirmNewGameButton;
        [SerializeField] private Button cancelNewGameButton;
        [SerializeField] private TMP_Text overwriteWarning;

        [SerializeField] private SettingsMenu settingsMenu;

        private const string DefaultUsername = "Adventurer";

        private void Awake()
        {
            if (settingsMenu == null)
            {
                settingsMenu = FindFirstObjectByType<SettingsMenu>();
            }

            if (newGameButton != null) newGameButton.onClick.AddListener(OpenNewGamePanel);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(() => settingsMenu?.Open());
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);
            if (confirmNewGameButton != null) confirmNewGameButton.onClick.AddListener(StartNewGame);
            if (cancelNewGameButton != null) cancelNewGameButton.onClick.AddListener(CloseNewGamePanel);

            GameSettings.Apply();
        }

        private void Start()
        {
            GameManager.Instance?.SetState(GameState.MainMenu);
            RefreshSaveState();
            SetActive(newGamePanel, false);
        }

        /// <summary>
        /// Continue is only offered when there is something to continue from
        /// (SRS FR2 Continue Game), so it is disabled rather than failing on click.
        /// </summary>
        private void RefreshSaveState()
        {
            bool hasSave = SaveSystem.HasSave;

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (saveInfoLabel == null)
            {
                return;
            }

            if (!hasSave)
            {
                saveInfoLabel.text = "No saved adventure yet.";
                return;
            }

            if (SaveSystem.TryLoad(out PlayerProfile profile))
            {
                int solved = 0;
                if (profile.dungeonProgress != null)
                {
                    foreach (DungeonProgress progress in profile.dungeonProgress)
                    {
                        solved += progress?.challengesSolved?.Count ?? 0;
                    }
                }

                saveInfoLabel.text =
                    $"Saved: {profile.username}  -  {solved}/{GameConstants.TotalChallenges} challenges, " +
                    $"{profile.unlockedDungeons.Count}/{GameConstants.TotalDungeons} dungeons unlocked";
            }
            else
            {
                saveInfoLabel.text = "Saved adventure could not be read.";
            }
        }

        private void OpenNewGamePanel()
        {
            SetActive(newGamePanel, true);

            if (usernameInput != null)
            {
                usernameInput.text = string.Empty;
                usernameInput.ActivateInputField();
            }

            // Starting fresh discards the existing save, so say so before doing it.
            if (overwriteWarning != null)
            {
                overwriteWarning.gameObject.SetActive(SaveSystem.HasSave);
                overwriteWarning.text = "This will erase your saved adventure.";
            }
        }

        private void CloseNewGamePanel()
        {
            SetActive(newGamePanel, false);
        }

        private void StartNewGame()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[MainMenuUI] No GameManager; cannot start a game.", this);
                return;
            }

            string username = usernameInput != null && !string.IsNullOrWhiteSpace(usernameInput.text)
                ? usernameInput.text.Trim()
                : DefaultUsername;

            manager.StartNewGame(username, "avatar_01");
            SessionTracker.Instance?.BeginSession();
            manager.LoadDungeon(GameConstants.FirstDungeonId);
        }

        private void ContinueGame()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null || !manager.ContinueGame())
            {
                RefreshSaveState();
                return;
            }

            SessionTracker.Instance?.BeginSession();

            // Resume in the furthest dungeon they have unlocked but not finished,
            // falling back to the last one they unlocked.
            manager.LoadDungeon(ResumeDungeonId(manager.ActiveProfile));
        }

        private static int ResumeDungeonId(PlayerProfile profile)
        {
            int resume = GameConstants.FirstDungeonId;

            for (int id = GameConstants.FirstDungeonId; id <= GameConstants.LastDungeonId; id++)
            {
                if (!profile.IsDungeonUnlocked(id))
                {
                    continue;
                }

                resume = id;
                DungeonProgress progress = profile.GetProgress(id);
                if (progress == null || !progress.completed)
                {
                    return id;
                }
            }

            return resume;
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
