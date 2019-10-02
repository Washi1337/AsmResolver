namespace AsmResolver.PE.DotNet.Metadata
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
    public abstract class StringsStream : IMetadataStream
    {
        public const string DefaultName = "#Strings";

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        } = DefaultName;

        /// <inheritdoc />
        public abstract bool CanRead
        {
            get;
        }

        /// <inheritdoc />
        public abstract IBinaryStreamReader CreateReader();

        /// <summary>
        /// Gets a string by its string index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The string.</returns>
        public abstract string GetStringByIndex(int index);
    }
}