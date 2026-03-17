namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for persisting meta world save state.
    /// Implementations handle the actual storage backend (PlayerPrefs, file, cloud).
    /// </summary>
    public interface IMetaSaveService
    {
        /// <summary>Save the given meta state.</summary>
        void Save(MetaSaveData data);

        /// <summary>
        /// Load the persisted meta state.
        /// Returns a fresh <see cref="MetaSaveData"/> if no save exists.
        /// </summary>
        MetaSaveData Load();

        /// <summary>Delete all persisted meta state.</summary>
        void Delete();
    }
}
