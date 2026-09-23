using MathDungeon.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// Shown when the third dungeon's gate is passed and the run is finished
    /// (SRS FR13 Game Completion). This is the minimal end-of-run acknowledgement;
    /// the full statistics screen is the Performance Summary in Milestone 7.
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonCompleteUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Button mainMenuButton;

        private GameManager manager;

        private void Awake()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            SetVisible(false);
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
            SetVisible(state == GameState.DungeonComplete);
        }

        private void SetVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }

            if (!visible || messageLabel == null)
            {
                return;
            }

            GameManager gm = GameManager.Instance;
            if (gm != null && gm.HasActiveProfile)
            {
                int solved = gm.ActiveProfile.dungeonProgress != null ? CountAllSolved(gm) : 0;
                messageLabel.text =
                    $"You solved {solved} of {GameConstants.TotalChallenges} challenges\n" +
                    $"and cleared all {GameConstants.TotalDungeons} dungeons.";
            }
            else
            {
                messageLabel.text = "All dungeons cleared.";
            }
        }

        private static int CountAllSolved(GameManager gm)
        {
            int total = 0;
            foreach (var progress in gm.ActiveProfile.dungeonProgress)
            {
                if (progress?.challengesSolved != null)
                {
                    total += progress.challengesSolved.Count;
                }
            }

            return total;
        }

        public void ReturnToMainMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(GameConstants.MainMenuSceneName))
            {
                Debug.LogWarning(
                    $"[DungeonCompleteUI] '{GameConstants.MainMenuSceneName}' is not in Build Settings yet (M7); staying put.", this);
                return;
            }

            GameManager.Instance?.LoadMainMenu();
        }
    }
}
