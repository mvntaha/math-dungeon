using MathDungeon.Core;
using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// Accumulates time spent in this dungeon into
    /// <see cref="DungeonProgress.elapsedTimeSeconds"/> (SRS FR9 Completion Time
    /// Tracking).
    ///
    /// This is a tracked metric only. The SRS is explicit that there is no time
    /// limit, countdown or time-based penalty, and players may take as long as they
    /// need - so nothing here may ever gate or punish. It only counts while the
    /// player is actually playing, so a pause or an open challenge does not inflate
    /// the figure.
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonTimer : MonoBehaviour
    {
        [SerializeField] private int dungeonId = GameConstants.FirstDungeonId;

        [Tooltip("Seconds between writes into the profile, to avoid touching it every frame.")]
        [SerializeField] private float flushInterval = 1f;

        private float unflushed;
        private float flushTimer;

        private DungeonProgress Progress =>
            GameManager.Instance != null && GameManager.Instance.HasActiveProfile
                ? GameManager.Instance.ActiveProfile.GetProgress(dungeonId)
                : null;

        /// <summary>Total seconds recorded for this dungeon, including the unflushed part.</summary>
        public int ElapsedSeconds
        {
            get
            {
                DungeonProgress progress = Progress;
                return progress != null ? progress.elapsedTimeSeconds + Mathf.FloorToInt(unflushed) : 0;
            }
        }

        private void Update()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                return;
            }

            // Count exploring and challenge-solving time, but not paused, finished
            // or Game Over time.
            GameState state = manager.CurrentState;
            if (state != GameState.Exploring && state != GameState.InChallenge)
            {
                return;
            }

            unflushed += Time.unscaledDeltaTime;
            flushTimer += Time.unscaledDeltaTime;

            if (flushTimer >= flushInterval)
            {
                flushTimer = 0f;
                Flush();
            }
        }

        private void OnDisable()
        {
            Flush();
        }

        private void Flush()
        {
            DungeonProgress progress = Progress;
            if (progress == null || unflushed < 1f)
            {
                return;
            }

            int whole = Mathf.FloorToInt(unflushed);
            progress.elapsedTimeSeconds += whole;
            unflushed -= whole;
        }

        /// <summary>Formats a duration as m:ss, or h:mm:ss past an hour.</summary>
        public static string Format(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            return hours > 0
                ? $"{hours}:{minutes:00}:{seconds:00}"
                : $"{minutes}:{seconds:00}";
        }
    }
}
