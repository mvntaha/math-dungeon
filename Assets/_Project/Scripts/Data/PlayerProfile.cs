using System;
using System.Collections.Generic;
using MathDungeon.Core;

namespace MathDungeon.Data
{
    /// <summary>
    /// The local player profile (SRS 1.9 PlayerProfile). This is the root object
    /// written to the single local JSON save file - there is no cloud copy.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public string profileId;
        public string username;
        public string avatarId;

        /// <summary>Profile creation timestamp, ISO 8601 (e.g. "2026-08-01").</summary>
        public string createdAt;

        /// <summary>Current health, 0-<see cref="GameConstants.MaxHearts"/>.</summary>
        public int hearts;

        public int coins;
        public List<int> unlockedDungeons = new List<int>();
        public List<DungeonProgress> dungeonProgress = new List<DungeonProgress>();

        /// <summary>Identifier of the most recent saved checkpoint, e.g. "d2_c3".</summary>
        public string lastCheckpoint;

        public PlayerProfile() { }

        /// <summary>
        /// Creates a profile with the default starting state: full hearts, no coins,
        /// only the first dungeon unlocked, and an empty progress entry per dungeon.
        /// </summary>
        public static PlayerProfile CreateNew(string profileId, string username, string avatarId)
        {
            PlayerProfile profile = new PlayerProfile
            {
                profileId = profileId,
                username = username,
                avatarId = avatarId,
                createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                hearts = GameConstants.MaxHearts,
                coins = 0,
                lastCheckpoint = string.Empty
            };

            profile.unlockedDungeons.Add(GameConstants.FirstDungeonId);

            for (int dungeonId = GameConstants.FirstDungeonId; dungeonId <= GameConstants.LastDungeonId; dungeonId++)
            {
                profile.dungeonProgress.Add(new DungeonProgress(dungeonId));
            }

            return profile;
        }

        public bool IsDungeonUnlocked(int dungeonId)
        {
            return unlockedDungeons.Contains(dungeonId);
        }

        public DungeonProgress GetProgress(int dungeonId)
        {
            for (int i = 0; i < dungeonProgress.Count; i++)
            {
                if (dungeonProgress[i].dungeonId == dungeonId)
                {
                    return dungeonProgress[i];
                }
            }

            return null;
        }
    }
}
