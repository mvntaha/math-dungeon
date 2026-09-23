using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// A milestone inside a dungeon (SRS FR3 Checkpoint System: resume from
    /// important milestones instead of restarting the entire dungeon).
    ///
    /// Note what this does and does not promise. SRS 1.8 Progress Preservation
    /// requires completed levels, rewards and unlocked content to survive - not the
    /// player's exact coordinates. So the saved value is a checkpoint identifier,
    /// and the position is only used to place the player when they resume or retry.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("Dungeon this checkpoint belongs to.")]
        [SerializeField] private int dungeonId = GameConstants.FirstDungeonId;

        [Tooltip("Order within the dungeon, used to build the saved id, e.g. 3 -> 'd1_c3'.")]
        [SerializeField] private int checkpointIndex = 1;

        [Tooltip("Where the player is placed when resuming here. Defaults to this object.")]
        [SerializeField] private Transform spawnPoint;

        private bool recorded;

        /// <summary>The identifier stored in <c>PlayerProfile.lastCheckpoint</c>, e.g. "d1_c3".</summary>
        public string CheckpointId => $"d{dungeonId}_c{checkpointIndex}";

        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (recorded || !other.CompareTag("Player"))
            {
                return;
            }

            recorded = true;
            GameManager.Instance?.NotifyCheckpointReached(CheckpointId);
            Debug.Log($"[Checkpoint] Reached {CheckpointId}.", this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.6f, 0.6f);
            Gizmos.DrawWireCube(SpawnPosition + Vector3.up, new Vector3(1f, 2f, 1f));
        }
    }
}
