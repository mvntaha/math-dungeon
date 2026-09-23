using UnityEngine;

namespace MathDungeon.Player
{
    /// <summary>
    /// Fixed-angle third-person follow camera, framed high enough to read a
    /// dungeon room and the props around the player. It does not rotate with the
    /// character, so corridors stay legible and movement direction stays predictable.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Camera position relative to the target, in world axes.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 5.5f, -5.5f);

        [Tooltip("How high above the target's pivot the camera aims.")]
        [SerializeField] private float lookHeight = 1.2f;

        [Tooltip("Follow damping. Larger is lazier.")]
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 followVelocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                SnapToTarget();
            }
        }

        private void Start()
        {
            if (target == null)
            {
                Debug.LogWarning($"[ThirdPersonCamera] No target assigned on {name}; the camera will not follow.", this);
                return;
            }

            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position, target.position + offset, ref followVelocity, smoothTime);

            transform.LookAt(target.position + Vector3.up * lookHeight);
        }

        /// <summary>Jumps straight to the framed position, with no easing on the first frame.</summary>
        private void SnapToTarget()
        {
            followVelocity = Vector3.zero;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
