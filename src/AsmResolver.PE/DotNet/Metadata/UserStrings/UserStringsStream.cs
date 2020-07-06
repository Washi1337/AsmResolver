namespace AsmResolver.PE.DotNet.Metadata.UserStrings
{
    /// <summary>
    /// Represents the metadata streams containing the user strings referenced by CIL method bodies.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the user strings stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// strings heap provides, is that any string index in a CIL method body is the start address (relative to the start
    /// of the #US stream) of a unicode string, prefixed by a length, and suffixed by one extra terminator byte.
    /// </remarks>
    public abstract class UserStringsStream : MetadataHeap
    {
        /// <summary>
        /// The default name of a user-strings stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#US";

        /// <summary>
        /// Initializes the user-strings stream with its default name.
        /// </summary>
        protected UserStringsStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the user-strings stream with a custom name.
        /// </summary>
        protected UserStringsStream(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets a string by its string index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The string, or <c>null</c> if the index was invalid.</returns>
        public abstract string GetStringByIndex(uint index);
    }
}