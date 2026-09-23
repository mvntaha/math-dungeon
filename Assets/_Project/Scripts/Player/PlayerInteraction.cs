using System;
using MathDungeon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MathDungeon.Player
{
    /// <summary>
    /// Finds the interactable the player is closest to and facing, and activates it
    /// on the interact key (SRS FR5 Object Interaction, FR4 challenge terminals).
    /// Like movement, interaction is ignored outside the exploring state.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInteraction : MonoBehaviour
    {
        private const string PlayerActionMap = "Player";
        private const string InteractActionName = "Interact";

        [Header("Input")]
        [Tooltip("Project input actions asset. The Player/Interact action is bound to E.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Detection")]
        [SerializeField] private float interactRange = 2.5f;
        [Tooltip("Which layers can hold interactables.")]
        [SerializeField] private LayerMask interactLayers = ~0;
        [Tooltip("How wide the arc in front of the player is, in degrees.")]
        [Range(30f, 360f)]
        [SerializeField] private float interactArcDegrees = 160f;
        [Tooltip("Origin of the detection sphere, relative to the player.")]
        [SerializeField] private Vector3 detectionOffset = new Vector3(0f, 1f, 0f);

        private readonly Collider[] hits = new Collider[16];
        private InputAction interactAction;
        private IInteractable currentTarget;

        /// <summary>The interactable that would be activated right now, or null.</summary>
        public IInteractable CurrentTarget => currentTarget;

        /// <summary>Raised when the focused interactable changes, for the HUD prompt (M7).</summary>
        public event Action<IInteractable> TargetChanged;

        private void Awake()
        {
            if (inputActions != null)
            {
                interactAction = inputActions.FindActionMap(PlayerActionMap, true).FindAction(InteractActionName, true);
            }
            else
            {
                Debug.LogError($"[PlayerInteraction] No input actions asset assigned on {name}; interaction is disabled.", this);
            }
        }

        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
                interactAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
            }
        }

        private void Update()
        {
            SetTarget(CanInteract() ? FindBestTarget() : null);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!CanInteract() || currentTarget == null || !currentTarget.CanInteract)
            {
                return;
            }

            currentTarget.Interact(this);
        }

        private bool CanInteract()
        {
            GameManager manager = GameManager.Instance;
            return manager == null || manager.CurrentState.AllowsPlayerControl();
        }

        /// <summary>
        /// Picks the nearest usable interactable inside the arc the player is facing,
        /// so standing between two terminals gives a predictable choice.
        /// </summary>
        private IInteractable FindBestTarget()
        {
            Vector3 origin = transform.TransformPoint(detectionOffset);
            int count = Physics.OverlapSphereNonAlloc(
                origin, interactRange, hits, interactLayers, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestDistance = float.MaxValue;
            float halfArc = interactArcDegrees * 0.5f;

            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = hits[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract)
                {
                    continue;
                }

                Vector3 toTarget = hits[i].transform.position - origin;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.0001f &&
                    Vector3.Angle(transform.forward, toTarget) > halfArc)
                {
                    continue;
                }

                float distance = toTarget.sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private void SetTarget(IInteractable target)
        {
            if (ReferenceEquals(target, currentTarget))
            {
                return;
            }

            currentTarget = target;
            TargetChanged?.Invoke(target);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(detectionOffset), interactRange);
        }
    }
}
