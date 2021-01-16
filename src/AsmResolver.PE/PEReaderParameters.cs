using System;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.PE
{
    /// <summary>
    /// Provides parameters for the reading process of a PE image. 
    /// </summary>
    public class PEReaderParameters
    {
        /// <summary>
        /// Initializes the PE reader parameters.
        /// </summary>
        public PEReaderParameters()
            : this(ThrowErrorListener.Instance)
        {
        }

        /// <summary>
        /// Initializes the PE reader parameters.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        public PEReaderParameters(IErrorListener errorListener)
        {
            MetadataStreamReader = new DefaultMetadataStreamReader();
            DebugDataReader = new DefaultDebugDataReader();
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
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