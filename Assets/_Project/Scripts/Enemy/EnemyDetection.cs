using UnityEngine;

namespace MathDungeon.Enemy
{
    /// <summary>
    /// Decides whether the enemy can currently see the player (SRS FR6 Enemy
    /// Detection). Kept apart from <see cref="EnemyAI"/> so the sensing rules can
    /// be tuned without touching the patrol and chase behaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyDetection : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Tag used to find the player when no target is assigned.")]
        [SerializeField] private string playerTag = "Player";

        [Header("Ranges")]
        [Tooltip("How far the enemy can notice the player when facing them.")]
        [SerializeField] private float detectionRadius = 9f;

        [Tooltip("Field of view in degrees, centred on the enemy's forward.")]
        [Range(30f, 360f)]
        [SerializeField] private float fieldOfView = 120f;

        [Tooltip("Inside this radius the player is noticed even from behind.")]
        [SerializeField] private float awarenessRadius = 3f;

        [Tooltip("Once chasing, the enemy keeps chasing out to this distance.")]
        [SerializeField] private float loseSightRadius = 13f;

        [Header("Line of sight")]
        [Tooltip("Geometry that blocks sight. Leave as the default to use walls and props.")]
        [SerializeField] private LayerMask sightBlockers = ~0;

        [Tooltip("Eye height offset, so sight lines are cast from the head rather than the feet.")]
        [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1.5f, 0f);

        private Transform target;

        public Transform Target => target;

        public float DetectionRadius => detectionRadius;

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning($"[EnemyDetection] No object tagged '{playerTag}' found; {name} will never detect anyone.", this);
            }
        }

        /// <summary>
        /// True when the player should be pursued. <paramref name="alreadyChasing"/>
        /// widens the range, so a chase does not drop the moment the player steps
        /// a little outside the detection cone.
        /// </summary>
        public bool CanSeePlayer(bool alreadyChasing)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 eye = transform.position + eyeOffset;
            Vector3 toTarget = (target.position + eyeOffset) - eye;
            float distance = toTarget.magnitude;

            float range = alreadyChasing ? loseSightRadius : detectionRadius;
            if (distance > range)
            {
                return false;
            }

            // Close enough to hear rather than see, or already committed to a chase.
            bool withinCone = alreadyChasing
                              || distance <= awarenessRadius
                              || Vector3.Angle(transform.forward, toTarget) <= fieldOfView * 0.5f;

            if (!withinCone)
            {
                return false;
            }

            return HasLineOfSight(eye, toTarget, distance);
        }

        private bool HasLineOfSight(Vector3 eye, Vector3 toTarget, float distance)
        {
            if (!Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, distance, sightBlockers, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            // Anything hit before the player counts as cover.
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawWireSphere(transform.position, awarenessRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, loseSightRadius);
        }
    }
}
