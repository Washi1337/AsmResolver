namespace AsmResolver.PE.DotNet.Metadata.Strings
{
    /// <summary>
    /// Represents the metadata stream containing the logical strings heap of a managed executable file.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the strings stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// strings heap provides, is that any string index in the tables stream is the start address (relative to the
    /// start of the strings stream) of a UTF-8 string that is zero terminated.
    /// </remarks>
    public abstract class StringsStream : MetadataHeap
    {
        /// <summary>
        /// The default name of a strings stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#Strings";

        /// <summary>
        /// Initializes the strings stream with its default name.
        /// </summary>
        protected StringsStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the strings stream with a custom name.
        /// </summary>
        protected StringsStream(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets a string by its string index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The string, or <c>null</c> if the index was invalid.</returns>
        public abstract Utf8String? GetStringByIndex(uint index);

        /// <summary>
        /// Searches the stream for the provided string.
        /// </summary>
        /// <param name="value">The string to search for.</param>
        /// <param name="index">When the function returns <c>true</c>, contains the index at which the string was found.</param>
        /// <returns><c>true</c> if the string index was found, <c>false</c> otherwise.</returns>
        public abstract bool TryFindStringIndex(Utf8String? value, out uint index);
    }
}
