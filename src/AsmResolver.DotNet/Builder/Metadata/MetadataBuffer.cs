using System;
using AsmResolver.DotNet.Builder.Metadata.Blob;
using AsmResolver.DotNet.Builder.Metadata.Guid;
using AsmResolver.DotNet.Builder.Metadata.Strings;
using AsmResolver.DotNet.Builder.Metadata.Tables;
using AsmResolver.DotNet.Builder.Metadata.UserStrings;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Provides a default implementation for <see cref="IMetadataBuffer" /> that produces compressed metadata (#~).
    /// </summary>
    public class MetadataBuffer : IMetadataBuffer
    {
        private readonly string _versionString;

        /// <summary>
        /// Creates a new metadata directory buffer that targets runtime version v4.0.30319. 
        /// </summary>
        public MetadataBuffer()
            : this("v4.0.30319")
        {
        }
        
        /// <summary>
        /// Creates a new metadata directory buffer.
        /// </summary>
        /// <param name="versionString">The runtime version string to use.</param>
        public MetadataBuffer(string versionString)
        {
            _versionString = versionString ?? throw new ArgumentNullException(nameof(versionString));
        }
        
        /// <inheritdoc />
        public BlobStreamBuffer BlobStream
        {
            get;
        } = new BlobStreamBuffer();

        /// <inheritdoc />
        public StringsStreamBuffer StringsStream
        {
            get;
        } = new StringsStreamBuffer();

        /// <inheritdoc />
        public UserStringsStreamBuffer UserStringsStream
        {
            get;
        } = new UserStringsStreamBuffer();

        /// <inheritdoc />
        public GuidStreamBuffer GuidStream
        {
            get;
        } = new GuidStreamBuffer();

        /// <inheritdoc />
        public TablesStreamBuffer TablesStream
        {
            get;
        } = new TablesStreamBuffer();

        /// <inheritdoc />
        public IMetadata CreateMetadata()
        {
            // Create streams.
            var tablesStream = TablesStream.CreateStream();
            var stringsStream = StringsStream.CreateStream();
            var userStringsStream = UserStringsStream.CreateStream();
            var guidStream = GuidStream.CreateStream();
            var blobStream = BlobStream.CreateStream();

            // Update index sizes.
            tablesStream.StringIndexSize = stringsStream.IndexSize;
            tablesStream.GuidIndexSize = guidStream.IndexSize;
            tablesStream.BlobIndexSize = blobStream.IndexSize;
            
            // Create metadata directory.
            return new PE.DotNet.Metadata.Metadata
            {
                VersionString = _versionString,
                Streams =
                {
                    tablesStream,
                    stringsStream,
                    userStringsStream,
                    guidStream,
                    blobStream
                }
            };
        }
    }
}