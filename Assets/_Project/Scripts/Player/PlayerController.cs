using MathDungeon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MathDungeon.Player
{
    /// <summary>
    /// Camera-relative WASD movement for the adventurer (SRS FR5 Movement Controls).
    /// Input is ignored whenever the game state does not allow player control, which
    /// is what freezes the player while a challenge dialog is open.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        private const string PlayerActionMap = "Player";
        private const string MoveActionName = "Move";
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");

        [Header("Input")]
        [Tooltip("Project input actions asset. The Player/Move action drives movement.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [Tooltip("Seconds to smooth a full turn toward the movement direction.")]
        [SerializeField] private float turnSmoothTime = 0.08f;
        [SerializeField] private float gravity = -20f;
        [Tooltip("Small downward push so the controller stays reported as grounded.")]
        [SerializeField] private float groundedStickForce = -2f;

        [Header("References")]
        [Tooltip("Movement is relative to this camera. Falls back to Camera.main.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Optional. Receives a 'Speed' float for a locomotion blend tree.")]
        [SerializeField] private Animator animator;

        private CharacterController controller;
        private InputAction moveAction;
        private float verticalVelocity;
        private float turnSmoothVelocity;
        private bool animatorHasSpeed;

        /// <summary>Planar speed in units per second, for the HUD or animation.</summary>
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (inputActions != null)
            {
                moveAction = inputActions.FindActionMap(PlayerActionMap, true).FindAction(MoveActionName, true);
            }
            else
            {
                Debug.LogError($"[PlayerController] No input actions asset assigned on {name}; the player cannot move.", this);
            }

            animatorHasSpeed = HasSpeedParameter();
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
        }

        private void Update()
        {
            Vector2 input = CanMove() && moveAction != null
                ? moveAction.ReadValue<Vector2>()
                : Vector2.zero;

            Vector3 motion = BuildPlanarMotion(input);
            CurrentSpeed = motion.magnitude;

            ApplyGravity();

            controller.Move((motion + Vector3.up * verticalVelocity) * Time.deltaTime);

            if (animatorHasSpeed)
            {
                animator.SetFloat(SpeedParameter, CurrentSpeed);
            }
        }

        /// <summary>
        /// Movement is allowed only while exploring. With no GameManager in the
        /// scene (a bare test scene) the player stays controllable.
        /// </summary>
        private bool CanMove()
        {
            GameManager manager = GameManager.Instance;
            return manager == null || manager.CurrentState.AllowsPlayerControl();
        }

        /// <summary>
        /// Turns stick/WASD input into world motion relative to where the camera is
        /// facing, and rotates the character to face the way it is moving.
        /// </summary>
        private Vector3 BuildPlanarMotion(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }

            Vector3 direction = (forward * input.y + right * input.x);
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            return direction * moveSpeed;
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
                return;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private bool HasSpeedParameter()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return false;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
