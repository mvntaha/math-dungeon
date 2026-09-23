using System;
using MathDungeon.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathDungeon.Core
{
    /// <summary>
    /// Persistent singleton that owns the active player profile, drives scene
    /// transitions, and decides when progress is written to disk (SRS 1.5, Logic
    /// layer). File I/O itself belongs to <see cref="SaveSystem"/>, and gameplay
    /// rules belong to their own systems.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState currentState = GameState.Booting;

        [Tooltip("Write progress to disk automatically on gameplay milestones (FR10).")]
        [SerializeField] private bool autoSaveEnabled = true;

        /// <summary>The profile being played. Null until a profile is created or loaded.</summary>
        public PlayerProfile ActiveProfile { get; private set; }

        public GameState CurrentState => currentState;

        public bool HasActiveProfile => ActiveProfile != null;

        /// <summary>True when there is a save to Continue from.</summary>
        public bool HasSavedGame => SaveSystem.HasSave;

        /// <summary>Raised after <see cref="CurrentState"/> changes.</summary>
        public event Action<GameState> StateChanged;

        /// <summary>Raised after a different profile becomes active.</summary>
        public event Action<PlayerProfile> ActiveProfileChanged;

        /// <summary>Raised after a save attempt, so the UI can show a notification (FR11).</summary>
        public event Action<SaveTrigger, bool> GameSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetActiveProfile(PlayerProfile profile)
        {
            ActiveProfile = profile;
            ActiveProfileChanged?.Invoke(profile);
        }

        public void SetState(GameState state)
        {
            if (currentState == state)
            {
                return;
            }

            currentState = state;
            StateChanged?.Invoke(state);
        }

        // --- Profile lifecycle (FR1, FR2) ---

        /// <summary>
        /// Starts a fresh adventure, discarding any previous save. The caller is
        /// responsible for confirming this with the player first (FR2 New Game).
        /// </summary>
        public PlayerProfile StartNewGame(string username, string avatarId)
        {
            SaveSystem.Delete();

            PlayerProfile profile = PlayerProfile.CreateNew(
                Guid.NewGuid().ToString("N"),
                username,
                avatarId);

            SetActiveProfile(profile);
            SaveGame(SaveTrigger.NewProfile);
            return profile;
        }

        /// <summary>
        /// Loads the most recent save and makes it the active profile.
        /// Returns false when there is nothing to continue from (FR2 Continue Game).
        /// </summary>
        public bool ContinueGame()
        {
            PlayerProfile profile;
            if (!SaveSystem.TryLoad(out profile))
            {
                Debug.LogWarning("[GameManager] Continue requested but no usable save was found.");
                return false;
            }

            SetActiveProfile(profile);
            return true;
        }

        // --- Saving (FR10) ---

        /// <summary>Writes the active profile to disk and reports the outcome.</summary>
        public bool SaveGame(SaveTrigger trigger)
        {
            if (!HasActiveProfile)
            {
                Debug.LogWarning("[GameManager] Save requested with no active profile.");
                return false;
            }

            bool saved = SaveSystem.Save(ActiveProfile);
            GameSaved?.Invoke(trigger, saved);
            return saved;
        }

        private void AutoSave(SaveTrigger trigger)
        {
            if (autoSaveEnabled)
            {
                SaveGame(trigger);
            }
        }

        // --- Milestone hooks: called by the challenge and dungeon systems ---

        /// <summary>Records a solved challenge on the active profile and auto-saves.</summary>
        public void NotifyChallengeSolved(int dungeonId, int challengeId)
        {
            DungeonProgress progress = GetProgressOrWarn(dungeonId);
            if (progress == null)
            {
                return;
            }

            progress.MarkChallengeSolved(challengeId);
            AutoSave(SaveTrigger.ChallengeSolved);
        }

        /// <summary>
        /// Records that a dungeon is finished, unlocks the next one in the linear
        /// chain, and auto-saves.
        /// </summary>
        public void NotifyDungeonCompleted(int dungeonId)
        {
            DungeonProgress progress = GetProgressOrWarn(dungeonId);
            if (progress == null)
            {
                return;
            }

            progress.completed = true;
            UnlockDungeon(dungeonId + 1);
            AutoSave(SaveTrigger.DungeonCompleted);
        }

        /// <summary>
        /// Grants access to a dungeon. Dungeons unlock linearly, so this is only
        /// ever called with the dungeon after the one just completed; ids past the
        /// last dungeon are ignored, which is what finishing the game looks like.
        /// </summary>
        public bool UnlockDungeon(int dungeonId)
        {
            if (!HasActiveProfile || !GameConstants.IsValidDungeonId(dungeonId))
            {
                return false;
            }

            if (ActiveProfile.IsDungeonUnlocked(dungeonId))
            {
                return false;
            }

            ActiveProfile.unlockedDungeons.Add(dungeonId);
            return true;
        }

        /// <summary>Records the most recent checkpoint (e.g. "d2_c3") and auto-saves (FR3).</summary>
        public void NotifyCheckpointReached(string checkpointId)
        {
            if (!HasActiveProfile)
            {
                Debug.LogWarning("[GameManager] Checkpoint reached with no active profile.");
                return;
            }

            ActiveProfile.lastCheckpoint = checkpointId;
            AutoSave(SaveTrigger.Checkpoint);
        }

        private DungeonProgress GetProgressOrWarn(int dungeonId)
        {
            if (!HasActiveProfile)
            {
                Debug.LogWarning("[GameManager] Progress update requested with no active profile.");
                return null;
            }

            DungeonProgress progress = ActiveProfile.GetProgress(dungeonId);
            if (progress == null)
            {
                Debug.LogError($"[GameManager] No progress entry for dungeon {dungeonId}.");
            }

            return progress;
        }

        // --- Scene transitions ---

        public void LoadMainMenu()
        {
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(GameConstants.MainMenuSceneName);
        }

        /// <summary>
        /// Loads a dungeon scene. Dungeons unlock linearly, so the caller is
        /// expected to have checked <see cref="PlayerProfile.IsDungeonUnlocked"/>.
        /// </summary>
        public void LoadDungeon(int dungeonId)
        {
            if (!GameConstants.IsValidDungeonId(dungeonId))
            {
                Debug.LogError($"[GameManager] Dungeon id {dungeonId} is outside 1-{GameConstants.LastDungeonId}.");
                return;
            }

            SetState(GameState.Exploring);
            SceneManager.LoadScene(GameConstants.GetDungeonSceneName(dungeonId));
        }
    }
}
