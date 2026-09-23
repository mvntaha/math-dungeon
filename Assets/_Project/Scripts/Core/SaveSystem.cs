using System;
using System.IO;
using MathDungeon.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MathDungeon.Core
{
    /// <summary>
    /// Persistence layer (SRS 1.5): reads and writes the single local JSON save file.
    /// Nothing here touches gameplay rules, and there is no cloud or network path.
    ///
    /// Writes are atomic (temp file then replace, keeping a .bak) so an unexpected
    /// shutdown cannot leave a half-written save, and loads are validated before use
    /// (Data Integrity / Progress Preservation, SRS 1.8).
    /// </summary>
    public static class SaveSystem
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);

        private static string BackupPath => SavePath + ".bak";
        private static string TempPath => SavePath + ".tmp";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        /// <summary>True when a save exists to Continue from (FR10 Saved Game Loading).</summary>
        public static bool HasSave => File.Exists(SavePath) || File.Exists(BackupPath);

        /// <summary>Writes the profile to disk. Returns false and logs if the write failed.</summary>
        public static bool Save(PlayerProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[SaveSystem] Refusing to save a null profile.");
                return false;
            }

            try
            {
                string json = JsonConvert.SerializeObject(profile, SerializerSettings);
                File.WriteAllText(TempPath, json);

                if (File.Exists(SavePath))
                {
                    // Replace keeps the previous save as the backup, so a failure
                    // part-way through still leaves one readable file behind.
                    File.Replace(TempPath, SavePath, BackupPath);
                }
                else
                {
                    File.Move(TempPath, SavePath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to write save file at {SavePath}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the saved profile, falling back to the backup if the main file is
        /// missing or corrupt. Returns false when no usable save could be read.
        /// </summary>
        public static bool TryLoad(out PlayerProfile profile)
        {
            if (TryLoadFrom(SavePath, out profile))
            {
                return true;
            }

            if (TryLoadFrom(BackupPath, out profile))
            {
                Debug.LogWarning("[SaveSystem] Main save file was unreadable; recovered from the backup.");
                return true;
            }

            profile = null;
            return false;
        }

        /// <summary>Removes the save file and its backup (New Game, once confirmed - FR2).</summary>
        public static bool Delete()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete save file: {e.Message}");
                return false;
            }
        }

        private static bool TryLoadFrom(string path, out PlayerProfile profile)
        {
            profile = null;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                profile = JsonConvert.DeserializeObject<PlayerProfile>(json, SerializerSettings);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not parse save file at {path}: {e.Message}");
                return false;
            }

            if (profile == null || string.IsNullOrEmpty(profile.profileId))
            {
                Debug.LogWarning($"[SaveSystem] Save file at {path} is empty or missing a profileId.");
                profile = null;
                return false;
            }

            RepairLoadedProfile(profile);
            return true;
        }

        /// <summary>
        /// Brings a loaded profile back inside its documented bounds so partial or
        /// hand-edited data cannot put the game into an impossible state.
        /// </summary>
        private static void RepairLoadedProfile(PlayerProfile profile)
        {
            profile.hearts = Mathf.Clamp(profile.hearts, GameConstants.GameOverHearts, GameConstants.MaxHearts);
            profile.coins = Mathf.Max(0, profile.coins);

            if (profile.unlockedDungeons == null)
            {
                profile.unlockedDungeons = new System.Collections.Generic.List<int>();
            }

            profile.unlockedDungeons.RemoveAll(id => !GameConstants.IsValidDungeonId(id));

            // The first dungeon is always available, otherwise the save is unplayable.
            if (!profile.unlockedDungeons.Contains(GameConstants.FirstDungeonId))
            {
                profile.unlockedDungeons.Add(GameConstants.FirstDungeonId);
            }

            if (profile.dungeonProgress == null)
            {
                profile.dungeonProgress = new System.Collections.Generic.List<DungeonProgress>();
            }

            profile.dungeonProgress.RemoveAll(p => p == null || !GameConstants.IsValidDungeonId(p.dungeonId));

            for (int dungeonId = GameConstants.FirstDungeonId; dungeonId <= GameConstants.LastDungeonId; dungeonId++)
            {
                DungeonProgress progress = profile.GetProgress(dungeonId);
                if (progress == null)
                {
                    profile.dungeonProgress.Add(new DungeonProgress(dungeonId));
                    continue;
                }

                if (progress.challengesSolved == null)
                {
                    progress.challengesSolved = new System.Collections.Generic.List<int>();
                }

                if (progress.attemptsPerChallenge == null)
                {
                    progress.attemptsPerChallenge = new System.Collections.Generic.Dictionary<int, int>();
                }

                progress.elapsedTimeSeconds = Mathf.Max(0, progress.elapsedTimeSeconds);
            }
        }
    }
}
