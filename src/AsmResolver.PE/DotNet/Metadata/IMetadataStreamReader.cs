using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides members for reading a stream stored in the metadata directory of a .NET image.
    /// </summary>
    public interface IMetadataStreamReader
    {
        /// <summary>
        /// Reads the contents of a metadata stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="header">The header of the metadata stream.</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The read metadata stream.</returns>
        IMetadataStream ReadStream(MetadataReaderContext context, MetadataStreamHeader header, ref BinaryStreamReader reader);
    }
}
