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
        public PEReadParameters()
        {
            MetadataStreamReader = new DefaultMetadataStreamReader();
            DebugDataReader = new DefaultDebugDataReader();
            ErrorListener = ThrowErrorListener.Instance;
        }

        /// <summary>
        /// Gets the object responsible for collecting any errors during the parsing.
        /// </summary>
        public IErrorListener ErrorListener
        {
            get;
            set;
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