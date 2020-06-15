using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources;

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
            Win32ResourceDataReader = new DefaultResourceDataReader();
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
        /// Gets or sets the object responsible for reading and interpreting Win32 resource data entries.
        /// </summary>
        public IWin32ResourceDataReader Win32ResourceDataReader
        {
            get;
            set;
        }
    }
}