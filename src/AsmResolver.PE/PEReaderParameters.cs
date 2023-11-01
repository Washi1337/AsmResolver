using System;
using AsmResolver.IO;
using AsmResolver.PE.Certificates;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.ReadyToRun;

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
            CertificateReader = new DefaultCertificateReader();
            ReadyToRunSectionReader = new DefaultReadyToRunSectionReader();
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

        /// <summary>
        /// Gets or sets the object responsible for reading certificates (such as authenticode signatures) in the
        /// security data directory of the input PE file.
        /// </summary>
        public ICertificateReader CertificateReader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the service to use for reading any additional files from the disk while reading the portable executable.
        /// </summary>
        public IFileService FileService
        {
            get;
            set;
        } = UncachedFileService.Instance;

        /// <summary>
        /// Gets or sets the object to use for reading ReadyToRun metadata sections from the disk while reading the
        /// portable executable.
        /// </summary>
        public IReadyToRunSectionReader ReadyToRunSectionReader
        {
            get;
            set;
        }
    }
}
