namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a basic implementation of a data entry that can be initialized and added to a resource
    /// directory in a PE image.
    /// </summary>
    public class ResourceData : ResourceDataBase
    {
        /// <summary>
        /// Creates a new named data entry.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(string name, IReadableSegment contents)
        {
            Name = name;
            Contents = contents;
        }

        /// <summary>
        /// Creates a new data entry defined by a numerical identifier..
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(uint id, IReadableSegment contents)
        {
            Contents = contents;
            Id = id;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetContents()
        {
            return null;
        }
        
    }
}