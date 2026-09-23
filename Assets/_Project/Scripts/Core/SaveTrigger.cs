namespace MathDungeon.Core
{
    /// <summary>
    /// Why a save happened. Auto-saves fire on gameplay milestones (FR10 Automatic
    /// Saving); <see cref="Manual"/> is the player choosing to save (FR10 Manual Saving).
    /// </summary>
    public enum SaveTrigger
    {
        Manual = 0,
        ChallengeSolved = 1,
        DungeonCompleted = 2,
        Checkpoint = 3,
        NewProfile = 4
    }
}
