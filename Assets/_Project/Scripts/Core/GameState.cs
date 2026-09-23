namespace MathDungeon.Core
{
    /// <summary>
    /// High-level application state owned by <see cref="GameManager"/>
    /// (SRS 1.5, Logic layer: "Controls application state, scene transitions,
    /// and global session conditions").
    /// </summary>
    public enum GameState
    {
        Booting = 0,
        MainMenu = 1,
        Exploring = 2,
        InChallenge = 3,
        Paused = 4,
        DungeonComplete = 5,
        GameOver = 6
    }
}
