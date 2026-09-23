using System;
using MathDungeon.Core;
using MathDungeon.Data;
using UnityEngine;

namespace MathDungeon.Player
{
    /// <summary>
    /// The three-heart health pool (SRS FR6 Health System). Hearts live on the
    /// saved <see cref="PlayerProfile"/> so they survive a reload; this component
    /// is the gameplay-side door onto them and raises the Game Over state when the
    /// pool empties.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealth : MonoBehaviour
    {
        /// <summary>Raised with the new heart count whenever it changes, for the HUD.</summary>
        public event Action<int> HeartsChanged;

        /// <summary>Raised once when the last heart is lost.</summary>
        public event Action Died;

        public int Hearts => Profile != null ? Profile.hearts : 0;

        public bool IsAlive => Hearts > GameConstants.GameOverHearts;

        private PlayerProfile Profile =>
            GameManager.Instance != null ? GameManager.Instance.ActiveProfile : null;

        /// <summary>
        /// Deducts a heart for an incorrect challenge answer and reports whether the
        /// player is still standing. Reaching zero enters the Game Over state, which
        /// offers retrying the zone or the main menu - never a hard restart.
        /// </summary>
        public bool LoseHeart()
        {
            PlayerProfile profile = Profile;
            if (profile == null)
            {
                Debug.LogWarning("[PlayerHealth] Heart loss requested with no active profile.", this);
                return true;
            }

            profile.hearts = Mathf.Max(
                GameConstants.GameOverHearts,
                profile.hearts - GameConstants.HeartsLostPerIncorrectAnswer);

            HeartsChanged?.Invoke(profile.hearts);

            if (profile.hearts > GameConstants.GameOverHearts)
            {
                return true;
            }

            Died?.Invoke();
            GameManager.Instance?.SetState(GameState.GameOver);
            return false;
        }

        /// <summary>
        /// Refills the pool. Used when retrying a zone and when entering a dungeon,
        /// since the player starts every dungeon with a full three hearts.
        /// </summary>
        public void RefillHearts()
        {
            PlayerProfile profile = Profile;
            if (profile == null)
            {
                return;
            }

            profile.hearts = GameConstants.MaxHearts;
            HeartsChanged?.Invoke(profile.hearts);
        }
    }
}
