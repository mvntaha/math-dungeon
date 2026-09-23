using System;
using MathDungeon.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathDungeon.Core
{
    /// <summary>
    /// Persistent singleton that owns the active player profile and drives scene
    /// transitions (SRS 1.5, Logic layer). It holds state only - saving and loading
    /// live in the persistence layer, and gameplay rules live in their own systems.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState currentState = GameState.Booting;

        /// <summary>The profile being played. Null until a profile is created or loaded.</summary>
        public PlayerProfile ActiveProfile { get; private set; }

        public GameState CurrentState => currentState;

        public bool HasActiveProfile => ActiveProfile != null;

        /// <summary>Raised after <see cref="CurrentState"/> changes.</summary>
        public event Action<GameState> StateChanged;

        /// <summary>Raised after a different profile becomes active.</summary>
        public event Action<PlayerProfile> ActiveProfileChanged;

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
