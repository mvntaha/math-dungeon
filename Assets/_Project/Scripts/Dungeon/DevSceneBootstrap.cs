using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// TEST-SCENE ONLY - DELETE WHEN M7 LANDS.
    ///
    /// A dungeon scene normally receives its <see cref="PlayerProfile"/> from the
    /// Main Menu, which calls <see cref="GameManager.ContinueGame"/> or
    /// <see cref="GameManager.StartNewGame"/> before loading the dungeon. Entering
    /// a dungeon scene directly from the Editor skips that, leaving no active
    /// profile, so solved challenges are silently dropped and nothing auto-saves.
    ///
    /// This component stands in for that flow so the scene can be played on its
    /// own. The Main Menu Continue / New Game buttons in Milestone 7 replace it
    /// entirely, and this script and its GameObject should then be removed.
    ///
    /// It refuses to run in the Main Menu scene, so it cannot hijack the real
    /// flow if it is ever copied into the wrong scene by accident.
    /// </summary>
    [DisallowMultipleComponent]
    public class DevSceneBootstrap : MonoBehaviour
    {
        [Tooltip("Username given to the throwaway profile when no save exists.")]
        [SerializeField] private string fallbackUsername = "DevTester";

        [Tooltip("Avatar id given to the throwaway profile when no save exists.")]
        [SerializeField] private string fallbackAvatarId = "avatar_01";

        private void Start()
        {
            string sceneName = gameObject.scene.name;
            if (sceneName == GameConstants.MainMenuSceneName)
            {
                Debug.LogWarning(
                    $"[DevSceneBootstrap] Found in {sceneName}. This is a test-scene helper and must not run " +
                    "in the Main Menu, which owns the real Continue / New Game flow. Disabling it.", this);
                enabled = false;
                return;
            }

            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[DevSceneBootstrap] No GameManager in the scene; cannot set up a profile.", this);
                return;
            }

            if (manager.HasActiveProfile)
            {
                // Arrived here through the real flow, which already did this.
                return;
            }

            if (manager.ContinueGame())
            {
                Debug.Log($"[DevSceneBootstrap] Test scene: continued the existing save " +
                          $"({manager.ActiveProfile.username}).", this);
            }
            else
            {
                manager.StartNewGame(fallbackUsername, fallbackAvatarId);
                Debug.Log($"[DevSceneBootstrap] Test scene: no save found, started a new profile " +
                          $"({fallbackUsername}).", this);
            }

            // A dungeon scene played directly starts in exploration.
            manager.SetState(GameState.Exploring);
        }
    }
}
