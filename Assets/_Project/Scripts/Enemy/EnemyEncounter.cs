using MathDungeon.Challenges;
using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Enemy
{
    /// <summary>
    /// Turns a catch into a mathematics challenge and applies the outcome to the
    /// creature (SRS 1.2: the encounter becomes a mathematical decision rather than
    /// a combat sequence). Keeping this apart from <see cref="EnemyAI"/> leaves the
    /// AI script about movement only.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    [DisallowMultipleComponent]
    public class EnemyEncounter : MonoBehaviour
    {
        [Tooltip("Which dungeon's challenges this enemy draws from.")]
        [SerializeField] private int dungeonId = GameConstants.FirstDungeonId;

        [Tooltip("Found in the scene when left empty.")]
        [SerializeField] private ChallengeManager challengeManager;

        private EnemyAI enemyAI;
        private bool encounterInProgress;

        private void Awake()
        {
            enemyAI = GetComponent<EnemyAI>();
            if (challengeManager == null)
            {
                challengeManager = FindFirstObjectByType<ChallengeManager>();
            }
        }

        private void OnEnable()
        {
            enemyAI.PlayerCaught += OnPlayerCaught;
            if (challengeManager != null)
            {
                challengeManager.ChallengeClosed += OnChallengeClosed;
            }
        }

        private void OnDisable()
        {
            enemyAI.PlayerCaught -= OnPlayerCaught;
            if (challengeManager != null)
            {
                challengeManager.ChallengeClosed -= OnChallengeClosed;
            }
        }

        private void OnPlayerCaught(EnemyAI _)
        {
            if (encounterInProgress || challengeManager == null)
            {
                return;
            }

            if (challengeManager.BeginEnemyEncounter(dungeonId))
            {
                encounterInProgress = true;
            }
            else
            {
                // Nothing to ask, so do not leave the creature frozen mid-chase.
                enemyAI.ResumePatrol();
            }
        }

        private void OnChallengeClosed(ChallengeSource source, bool solved)
        {
            if (!encounterInProgress || source != ChallengeSource.Enemy)
            {
                return;
            }

            encounterInProgress = false;

            if (solved)
            {
                enemyAI.Dispel();
            }
            else
            {
                enemyAI.ResumePatrol();
            }
        }
    }
}
