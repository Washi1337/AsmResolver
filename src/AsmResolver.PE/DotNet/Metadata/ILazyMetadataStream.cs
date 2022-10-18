namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a metadata stream that is initialized lazily.
    /// </summary>
    public interface ILazyMetadataStream : IMetadataStream
    {
        /// <summary>
        /// Finalizes the initialization process of the metadata stream.
        /// </summary>
        /// <param name="parentMetadata">The metadata directory that defines the stream.</param>
        void Initialize(IMetadata parentMetadata);
    }
}
