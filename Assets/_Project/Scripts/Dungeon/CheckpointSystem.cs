using System.Collections.Generic;
using MathDungeon.Core;
using UnityEngine;

namespace MathDungeon.Dungeon
{
    /// <summary>
    /// Decides where the player starts in this dungeon: at the saved checkpoint if
    /// there is one for this dungeon, otherwise at the entrance (SRS FR3).
    /// Also supplies the respawn point used when retrying after a Game Over, so
    /// "retry this zone" means the last milestone rather than the dungeon start.
    /// </summary>
    [DisallowMultipleComponent]
    public class CheckpointSystem : MonoBehaviour
    {
        [SerializeField] private int dungeonId = GameConstants.FirstDungeonId;

        [Tooltip("Where the player starts with no saved checkpoint in this dungeon.")]
        [SerializeField] private Transform entrancePoint;

        [Tooltip("All checkpoints in this dungeon.")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        [Tooltip("Place the player at the resume point when the scene loads.")]
        [SerializeField] private bool placePlayerOnStart = true;

        /// <summary>Where a retry or a resume should put the player right now.</summary>
        public Vector3 ResumePosition
        {
            get
            {
                Checkpoint saved = FindSavedCheckpoint();
                if (saved != null)
                {
                    return saved.SpawnPosition;
                }

                return entrancePoint != null ? entrancePoint.position : transform.position;
            }
        }

        private void Start()
        {
            if (placePlayerOnStart)
            {
                PlacePlayerAtResumePoint();
            }
        }

        /// <summary>Moves the player to <see cref="ResumePosition"/>.</summary>
        public void PlacePlayerAtResumePoint()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            // The CharacterController overrides transform writes, so disable it first.
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            player.transform.position = ResumePosition;

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private Checkpoint FindSavedCheckpoint()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null || !manager.HasActiveProfile)
            {
                return null;
            }

            string saved = manager.ActiveProfile.lastCheckpoint;
            if (string.IsNullOrEmpty(saved))
            {
                return null;
            }

            for (int i = 0; i < checkpoints.Count; i++)
            {
                // Only this dungeon's checkpoints match, so a checkpoint saved in
                // another dungeon never drags the player to the wrong place.
                if (checkpoints[i] != null && checkpoints[i].CheckpointId == saved)
                {
                    return checkpoints[i];
                }
            }

            return null;
        }
    }
}
