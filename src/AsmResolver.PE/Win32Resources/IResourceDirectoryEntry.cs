namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents one entry in a win32 resource directory.
    /// </summary>
    public interface IResourceDirectoryEntry
    {
        /// <summary>
        /// Gets or sets the name of the entry.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the entry.
        /// </summary>
        uint Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the entry is a sub directory or not.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Gets a value indicating the entry is a data entry or not.
        /// </summary>
        bool IsData { get; }
    }
}