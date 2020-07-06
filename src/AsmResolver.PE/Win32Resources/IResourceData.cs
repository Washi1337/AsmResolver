namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single data entry in a Win32 resource directory.
    /// </summary>
    public interface IResourceData : IResourceEntry
    {
        /// <summary>
        /// Gets or sets the raw contents of the data entry.
        /// </summary>
        ISegment Contents
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="Contents"/> is readable using a binary stream reader.
        /// </summary>
        bool CanRead
        {
            get;
        }

        /// <summary>
        /// Creates a new binary stream reader that reads the raw contents of the resource file.
        /// </summary>
        /// <returns>The reader.</returns>
        IBinaryStreamReader CreateReader();
    }
}