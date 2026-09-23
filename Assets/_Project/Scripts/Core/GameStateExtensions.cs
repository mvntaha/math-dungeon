namespace MathDungeon.Core
{
    /// <summary>
    /// Which states the player is allowed to act in. Keeping this in one place
    /// stops each gameplay script from inventing its own list of "frozen" states.
    /// </summary>
    public static class GameStateExtensions
    {
        /// <summary>
        /// True only while exploring. Answering a challenge, pausing, finishing a
        /// dungeon or hitting Game Over all freeze the player where they stand.
        /// </summary>
        public static bool AllowsPlayerControl(this GameState state)
        {
            return state == GameState.Exploring;
        }
    }
}
