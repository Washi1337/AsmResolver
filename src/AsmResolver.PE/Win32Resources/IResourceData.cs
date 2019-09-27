namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single data entry in a Win32 resource directory.
    /// </summary>
    public interface IResourceData : IResourceDirectoryEntry
    {
        /// <summary>
        /// Gets or sets the raw contents of the data entry.
        /// </summary>
        IReadableSegment Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the code page that is used to decode code point values within the resource data. 
        /// </summary>
        /// <remarks>
        /// Typically, the code page would be the Unicode code page.
        /// </remarks>
        uint CodePage
        {
            get;
            set;
        }
    }
}