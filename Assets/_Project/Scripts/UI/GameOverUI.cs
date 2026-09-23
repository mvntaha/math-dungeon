using MathDungeon.Core;
using MathDungeon.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// The Game Over choice (SRS FR6): running out of hearts offers retrying the
    /// current challenge zone or returning to the Main Menu. It never restarts the
    /// whole game, and it never discards saved progress.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Retry")]
        [Tooltip("Where the player is placed when retrying the zone. Falls back to their current spot.")]
        [SerializeField] private Transform retrySpawnPoint;

        [SerializeField] private PlayerHealth playerHealth;

        private GameManager manager;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryZone);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            SetPanelVisible(false);
        }

        private void Start()
        {
            manager = GameManager.Instance;
            if (manager != null)
            {
                manager.StateChanged += OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.StateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(GameState state)
        {
            SetPanelVisible(state == GameState.GameOver);
        }

        /// <summary>Refills hearts and drops the player back into the zone to try again.</summary>
        public void RetryZone()
        {
            playerHealth?.RefillHearts();

            if (retrySpawnPoint != null)
            {
                MovePlayerTo(retrySpawnPoint.position);
            }

            SetPanelVisible(false);
            GameManager.Instance?.SetState(GameState.Exploring);
        }

        public void ReturnToMainMenu()
        {
            playerHealth?.RefillHearts();

            // The Main Menu scene arrives in M7; until then, say so rather than
            // dropping the player into a failed load.
            if (!Application.CanStreamedLevelBeLoaded(GameConstants.MainMenuSceneName))
            {
                Debug.LogWarning(
                    $"[GameOverUI] '{GameConstants.MainMenuSceneName}' is not in Build Settings yet (it arrives in M7); " +
                    "staying in the dungeon instead.", this);
                RetryZone();
                return;
            }

            SetPanelVisible(false);
            GameManager.Instance?.LoadMainMenu();
        }

        private static void MovePlayerTo(Vector3 position)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            // The CharacterController overrides transform writes, so disable it first.
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            player.transform.position = position;

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }

            if (visible && messageLabel != null)
            {
                messageLabel.text = "You ran out of hearts.";
            }
        }
    }
}
