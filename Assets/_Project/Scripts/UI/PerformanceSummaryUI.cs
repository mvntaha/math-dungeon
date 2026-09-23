using MathDungeon.Core;
using MathDungeon.Data;
using MathDungeon.Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathDungeon.UI
{
    /// <summary>
    /// End-of-run statistics (SRS FR9 Performance Summary, FR13 Final Statistics):
    /// solved questions, incorrect attempts, accuracy, coins, score and duration,
    /// read from the session's <see cref="PerformanceRecord"/>.
    ///
    /// It appears on <see cref="GameState.DungeonComplete"/>, which the third
    /// dungeon's gate raises.
    /// </summary>
    [DisallowMultipleComponent]
    public class PerformanceSummaryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text statsLabel;
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
            if (state == GameState.DungeonComplete)
            {
                Show();
            }
            else
            {
                SetVisible(false);
            }
        }

        public void Show()
        {
            PerformanceRecord record = SessionTracker.Instance != null
                ? SessionTracker.Instance.CompleteSession()
                : new PerformanceRecord();

            GameManager gm = GameManager.Instance;
            PlayerProfile profile = gm != null ? gm.ActiveProfile : null;

            int dungeonsCleared = 0;
            int totalSolved = 0;
            int totalSeconds = 0;
            if (profile?.dungeonProgress != null)
            {
                foreach (DungeonProgress progress in profile.dungeonProgress)
                {
                    if (progress == null)
                    {
                        continue;
                    }

                    if (progress.completed)
                    {
                        dungeonsCleared++;
                    }

                    totalSolved += progress.challengesSolved?.Count ?? 0;
                    totalSeconds += progress.elapsedTimeSeconds;
                }
            }

            bool wholeGame = dungeonsCleared >= GameConstants.TotalDungeons;

            if (titleLabel != null)
            {
                titleLabel.text = wholeGame ? "ADVENTURE COMPLETE" : "DUNGEON COMPLETE";
            }

            if (statsLabel != null)
            {
                statsLabel.text =
                    $"Challenges solved      {totalSolved} / {GameConstants.TotalChallenges}\n" +
                    $"Incorrect attempts     {record.incorrectAttempts}\n" +
                    $"Accuracy               {record.accuracyPercent:0.#}%\n" +
                    $"Coins earned           {record.coinsEarned}\n" +
                    $"Total score            {record.totalScore}\n" +
                    $"Dungeons cleared       {dungeonsCleared} / {GameConstants.TotalDungeons}\n" +
                    $"Time played            {DungeonTimer.Format(totalSeconds)}";
            }

            SetVisible(true);
        }

        public void ReturnToMainMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(GameConstants.MainMenuSceneName))
            {
                Debug.LogWarning($"[PerformanceSummaryUI] '{GameConstants.MainMenuSceneName}' is not in Build Settings.", this);
                return;
            }

            SetVisible(false);
            GameManager.Instance?.LoadMainMenu();
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
