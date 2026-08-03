namespace VRRoomCraft.Data.Save
{
    /// <summary>
    /// Contract implemented by world objects (FurnitureSlot, MaterialSlot) for state serialization.
    /// Exposes a unique identifier and save/load state handlers.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Gets the unique string key identifying this object in saved room data.
        /// </summary>
        string SaveId { get; }

        /// <summary>
        /// Gets the current serializable state object.
        /// </summary>
        object GetSaveState();

        /// <summary>
        /// Restores state from a saved object payload.
        /// </summary>
        void LoadSaveState(object state);
    }
}
