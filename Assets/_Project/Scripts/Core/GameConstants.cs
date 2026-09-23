namespace MathDungeon.Core
{
    /// <summary>
    /// Single source of truth for the gameplay constants fixed by the SRS.
    /// Gameplay code must reference these instead of hard-coding numbers.
    /// </summary>
    public static class GameConstants
    {
        // --- Dungeons and challenges (SRS 1.3) ---
        public const int TotalDungeons = 3;
        public const int ChallengesPerDungeon = 4;
        public const int TotalChallenges = TotalDungeons * ChallengesPerDungeon; // 12

        public const int FirstDungeonId = 1;
        public const int LastDungeonId = TotalDungeons;

        // --- Health (SRS 1.7 FR6 Health System) ---
        /// <summary>Hearts the player starts each dungeon with, and the maximum they can hold.</summary>
        public const int MaxHearts = 3;
        public const int StartingHearts = MaxHearts;

        /// <summary>Hearts lost per incorrect challenge answer.</summary>
        public const int HeartsLostPerIncorrectAnswer = 1;

        /// <summary>Heart count at which the Game Over state is entered.</summary>
        public const int GameOverHearts = 0;

        // --- Persistence (SRS 1.7 FR1, FR10) ---
        /// <summary>Name of the single local JSON save file. No cloud save exists.</summary>
        public const string SaveFileName = "mathdungeon_save.json";

        // --- Scenes ---
        public const string MainMenuSceneName = "MainMenu";
        public const string DungeonScenePrefix = "Dungeon";

        /// <summary>Scene name for a dungeon id, e.g. 2 -> "Dungeon2".</summary>
        public static string GetDungeonSceneName(int dungeonId)
        {
            return DungeonScenePrefix + dungeonId;
        }

        public static bool IsValidDungeonId(int dungeonId)
        {
            return dungeonId >= FirstDungeonId && dungeonId <= LastDungeonId;
        }
    }
}
