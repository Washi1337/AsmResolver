using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.File;

namespace AsmResolver.PE
{
    /// <summary>
    /// Provides parameters for the reading process of a PE image. 
    /// </summary>
    public class PEReadParameters
    {
        /// <summary>
        /// Initializes the PE reader parameters.
        /// </summary>
        /// <param name="resolver">
        /// The object responsible for resolving references within the original PE file.
        /// This is usually just the underlying <see cref="PEFile"/> instance.
        /// </param>
        public PEReadParameters(ISegmentReferenceResolver resolver)
        {
            ReferenceResolver = resolver;
            MetadataStreamReader = new DefaultMetadataStreamReader(resolver);
            DebugDataReader = new DefaultDebugDataReader(resolver);
        }

        /// <summary>
        /// Gets the object responsible for resolving references to segments within the raw PE file.
        /// </summary>
        public ISegmentReferenceResolver ReferenceResolver
        {
            get;
        }

        /// <summary>
        /// Gets or sets the object responsible for reading metadata streams in the .NET data directory.
        /// </summary>
        public IMetadataStreamReader MetadataStreamReader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the object responsible for reading debug data streams in the debug data directory.
        /// </summary>
        public IDebugDataReader DebugDataReader
        {
            get;
            set;
        }
    }
}