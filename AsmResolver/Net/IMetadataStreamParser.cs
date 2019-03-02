namespace AsmResolver.Net
{
    /// <summary>
    /// Provides members for parsing a metadata stream from a reading context.
    /// </summary>
    public interface IMetadataStreamParser
    {
        /// <summary>
        /// Reads a single metadata stream.
        /// </summary>
        /// <param name="streamName">The name of the stream to read.</param>
        /// <param name="context">The reading context that points to the beginning of the metadata stream.</param>
        /// <returns>The stream that was read.</returns>
        MetadataStream ReadStream(string streamName, ReadingContext context);
    }
}