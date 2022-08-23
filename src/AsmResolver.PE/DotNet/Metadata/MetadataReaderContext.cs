using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a context for a .NET metadata directory reader.
    /// </summary>
    public class MetadataReaderContext : IErrorListener
    {
        /// <summary>
        /// Constructs a new metadata reader context.
        /// </summary>
        /// <param name="factory">The factory object responsible for translating RVAs to references.</param>
        public MetadataReaderContext(ISegmentReferenceFactory factory)
            : this(factory, ThrowErrorListener.Instance, DefaultMetadataStreamReader.Instance)
        {
        }

        /// <summary>
        /// Constructs a new metadata reader context.
        /// </summary>
        /// <param name="referenceFactory">The factory object responsible for translating RVAs to references.</param>
        /// <param name="errorListener">The object responsible for collecting any errors during the parsing.</param>
        /// <param name="metadataStreamReader">The object responsible for reading metadata streams in the .NET data directory.</param>
        public MetadataReaderContext(
            ISegmentReferenceFactory referenceFactory,
            IErrorListener errorListener,
            IMetadataStreamReader metadataStreamReader)
        {
            ReferenceFactory = referenceFactory;
            ErrorListener = errorListener;
            MetadataStreamReader = metadataStreamReader;
        }

        /// <summary>
        /// Gets the factory responsible for translating RVAs to references.
        /// </summary>
        public ISegmentReferenceFactory ReferenceFactory
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for collecting any errors during the parsing.
        /// </summary>
        public IErrorListener ErrorListener
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for reading metadata streams in the .NET data directory.
        /// </summary>
        public IMetadataStreamReader MetadataStreamReader
        {
            get;
        }

        /// <summary>
        /// Constructs a metadata reader context from a PE reader context.
        /// </summary>
        /// <param name="context">The context to transform.</param>
        /// <returns>The constructed context.</returns>
        public static MetadataReaderContext FromReaderContext(PEReaderContext context) => new(
            context.File,
            context.Parameters.ErrorListener,
            context.Parameters.MetadataStreamReader);

        /// <inheritdoc />
        public void MarkAsFatal() => ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) => ErrorListener.RegisterException(exception);
    }
}
