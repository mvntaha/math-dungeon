using System;
using System.Collections.Generic;
using MathDungeon.Core;
using UnityEngine;
using UnityEngine.AI;

namespace MathDungeon.Enemy
{
    /// <summary>
    /// The single dungeon enemy (SRS 1.3): a NavMeshAgent that patrols a fixed
    /// route and chases the player on sight. Catching the player does not deal
    /// damage - it converts the encounter into a mathematics challenge, which is
    /// the whole point of the creature (SRS 1.2 AI Pursuit Encounter).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyDetection))]
    [DisallowMultipleComponent]
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");

        public enum State
        {
            Patrolling = 0,
            Chasing = 1,
            Encountering = 2
        }

        [Header("Patrol")]
        [Tooltip("Waypoints walked in order, looping. Two or more for a real route.")]
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

        [Tooltip("Seconds paused at each waypoint.")]
        [SerializeField] private float waypointPause = 1.5f;

        [SerializeField] private float patrolSpeed = 1.8f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 3.6f;

        [Tooltip("How close the enemy must get to trigger the challenge encounter.")]
        [SerializeField] private float catchDistance = 1.6f;

        [Header("Presentation")]
        [Tooltip("Receives a 'Speed' float, same convention as the player.")]
        [SerializeField] private Animator animator;

        [SerializeField] private float animatorDampTime = 0.12f;

        /// <summary>Raised when the enemy catches the player and the challenge should open.</summary>
        public event Action<EnemyAI> PlayerCaught;

        private NavMeshAgent agent;
        private EnemyDetection detection;
        private int patrolIndex;
        private float pauseTimer;
        private State state = State.Patrolling;

        public State CurrentState => state;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            detection = GetComponent<EnemyDetection>();
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f;
        }

        private void Start()
        {
            if (patrolPoints.Count == 0)
            {
                Debug.LogWarning($"[EnemyAI] {name} has no patrol points; it will idle in place.", this);
            }

            GoToCurrentPatrolPoint();
        }

        private void Update()
        {
            // While a challenge is open, or the run is over, the creature holds still.
            if (state == State.Encountering || !GameIsRunning())
            {
                HaltAgent();
                UpdateAnimator();
                return;
            }

            if (detection.CanSeePlayer(state == State.Chasing))
            {
                Chase();
            }
            else if (state == State.Chasing)
            {
                ReturnToPatrol();
            }
            else
            {
                Patrol();
            }

            UpdateAnimator();
        }

        /// <summary>The enemy only acts while the player is free to explore.</summary>
        private bool GameIsRunning()
        {
            GameManager manager = GameManager.Instance;
            return manager == null || manager.CurrentState.AllowsPlayerControl();
        }

        private void Patrol()
        {
            state = State.Patrolling;
            agent.speed = patrolSpeed;

            if (patrolPoints.Count == 0)
            {
                HaltAgent();
                return;
            }

            if (agent.pathPending || agent.remainingDistance > agent.radius + 0.2f)
            {
                agent.isStopped = false;
                return;
            }

            // Arrived: wait a beat, then move on to the next point.
            pauseTimer += Time.deltaTime;
            agent.isStopped = true;

            if (pauseTimer >= waypointPause)
            {
                pauseTimer = 0f;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
                GoToCurrentPatrolPoint();
            }
        }

        private void Chase()
        {
            state = State.Chasing;
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            pauseTimer = 0f;

            Transform target = detection.Target;
            if (target == null)
            {
                return;
            }

            agent.SetDestination(target.position);

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= catchDistance)
            {
                CatchPlayer();
            }
        }

        private void CatchPlayer()
        {
            state = State.Encountering;
            HaltAgent();
            transform.LookAt(new Vector3(detection.Target.position.x, transform.position.y, detection.Target.position.z));
            PlayerCaught?.Invoke(this);
        }

        private void ReturnToPatrol()
        {
            state = State.Patrolling;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            GoToCurrentPatrolPoint();
        }

        /// <summary>
        /// Called after an encounter ends without the enemy being dispelled, so it
        /// resumes its route instead of instantly re-triggering the challenge.
        /// </summary>
        public void ResumePatrol()
        {
            state = State.Patrolling;
            pauseTimer = 0f;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }

            GoToCurrentPatrolPoint();
        }

        /// <summary>
        /// The player answered correctly and dispelled the creature (SRS 1.2: a
        /// correct answer lets the player defeat or dispel the enemy).
        /// </summary>
        public void Dispel()
        {
            HaltAgent();
            gameObject.SetActive(false);
        }

        private void GoToCurrentPatrolPoint()
        {
            if (patrolPoints.Count == 0 || !agent.isOnNavMesh)
            {
                return;
            }

            Transform point = patrolPoints[Mathf.Clamp(patrolIndex, 0, patrolPoints.Count - 1)];
            if (point != null)
            {
                agent.SetDestination(point.position);
            }
        }

        private void HaltAgent()
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        private void UpdateAnimator()
        {
            if (animator == null)
            {
                return;
            }

            Vector3 planar = agent.velocity;
            planar.y = 0f;
            animator.SetFloat(SpeedParameter, planar.magnitude, animatorDampTime, Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, catchDistance);

            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null)
                {
                    continue;
                }

                Gizmos.DrawWireCube(patrolPoints[i].position, Vector3.one * 0.4f);
                Transform next = patrolPoints[(i + 1) % patrolPoints.Count];
                if (next != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, next.position);
                }
            }
        }
    }
}
