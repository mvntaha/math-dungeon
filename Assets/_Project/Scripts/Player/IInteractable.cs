namespace MathDungeon.Player
{
    /// <summary>
    /// Anything the player can activate with the interact key: challenge terminals,
    /// doors, switches, chests (SRS FR5 Object Interaction).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Short verb phrase for the HUD prompt, e.g. "Examine terminal".</summary>
        string InteractionPrompt { get; }

        /// <summary>False when the object is present but not currently usable (already solved, locked).</summary>
        bool CanInteract { get; }

        void Interact(PlayerInteraction interactor);
    }
}
