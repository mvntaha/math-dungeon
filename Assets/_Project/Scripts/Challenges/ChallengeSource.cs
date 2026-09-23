namespace MathDungeon.Challenges
{
    /// <summary>
    /// What put the player in front of a challenge. Both routes cost a heart on a
    /// wrong answer; the source decides what happens in the world afterwards.
    /// </summary>
    public enum ChallengeSource
    {
        /// <summary>A terminal, gate or chest the player chose to interact with.</summary>
        Terminal = 0,

        /// <summary>An enemy that caught the player (SRS 1.2 AI Pursuit Encounter).</summary>
        Enemy = 1
    }
}
