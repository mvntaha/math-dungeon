using MathDungeon.Core;
using MathDungeon.Dungeon;
using MathDungeon.Player;
using TMPro;
using UnityEngine;

namespace MathDungeon.UI
{
    /// <summary>
    /// The in-game heads-up display (SRS 1.3 In-Game HUD): health, coins, current
    /// objective and elapsed time. The time readout is informational only - the SRS
    /// forbids any countdown or time pressure.
    /// </summary>
    [DisallowMultipleComponent]
    public class HUD : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text heartsLabel;
        [SerializeField] private TMP_Text coinsLabel;
        [SerializeField] private TMP_Text objectiveLabel;
        [SerializeField] private TMP_Text timeLabel;

        [Header("Sources (found in the scene when empty)")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private DungeonTimer dungeonTimer;

        private void Awake()
        {
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (dungeonManager == null) dungeonManager = FindFirstObjectByType<DungeonManager>();
            if (dungeonTimer == null) dungeonTimer = FindFirstObjectByType<DungeonTimer>();
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.HeartsChanged += OnHeartsChanged;
            }

            if (dungeonManager != null)
            {
                dungeonManager.ProgressChanged += OnProgressChanged;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HeartsChanged -= OnHeartsChanged;
            }

            if (dungeonManager != null)
            {
                dungeonManager.ProgressChanged -= OnProgressChanged;
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void Update()
        {
            // Only the clock and coins need polling; the rest are event-driven.
            if (timeLabel != null && dungeonTimer != null)
            {
                timeLabel.text = DungeonTimer.Format(dungeonTimer.ElapsedSeconds);
            }

            UpdateCoins();
            SetVisible(ShouldShow());
        }

        /// <summary>The HUD is hidden whenever a full-screen panel owns the view.</summary>
        private bool ShouldShow()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                return true;
            }

            GameState state = manager.CurrentState;
            return state == GameState.Exploring || state == GameState.InChallenge;
        }

        private void RefreshAll()
        {
            OnHeartsChanged(playerHealth != null ? playerHealth.Hearts : GameConstants.MaxHearts);
            UpdateCoins();

            if (dungeonManager != null)
            {
                OnProgressChanged(dungeonManager.SolvedCount, GameConstants.ChallengesPerDungeon);
            }
        }

        private void OnHeartsChanged(int hearts)
        {
            if (heartsLabel == null)
            {
                return;
            }

            // Full hearts then empty ones, so the pool size stays readable.
            int full = Mathf.Clamp(hearts, 0, GameConstants.MaxHearts);
            heartsLabel.text = new string('♥', full) + new string('♡', GameConstants.MaxHearts - full);
        }

        private void OnProgressChanged(int solved, int total)
        {
            if (objectiveLabel == null)
            {
                return;
            }

            int id = dungeonManager != null ? dungeonManager.DungeonId : GameConstants.FirstDungeonId;
            objectiveLabel.text = solved >= total
                ? $"Dungeon {id}  -  head for the gate"
                : $"Dungeon {id}  -  challenges {solved}/{total}";
        }

        private void UpdateCoins()
        {
            if (coinsLabel == null)
            {
                return;
            }

            GameManager manager = GameManager.Instance;
            int coins = manager != null && manager.HasActiveProfile ? manager.ActiveProfile.coins : 0;
            coinsLabel.text = coins.ToString();
        }

        private void SetVisible(bool visible)
        {
            if (panel != null && panel.activeSelf != visible)
            {
                panel.SetActive(visible);
            }
        }
    }
}
