using System;
using AsmResolver.DotNet.Builder.Metadata.Blob;
using AsmResolver.DotNet.Builder.Metadata.Guid;
using AsmResolver.DotNet.Builder.Metadata.Strings;
using AsmResolver.DotNet.Builder.Metadata.Tables;
using AsmResolver.DotNet.Builder.Metadata.UserStrings;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

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
            : this(KnownRuntimeVersions.Clr40)
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
        } = new();

        /// <inheritdoc />
        public StringsStreamBuffer StringsStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public UserStringsStreamBuffer UserStringsStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public GuidStreamBuffer GuidStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public TablesStreamBuffer TablesStream
        {
            get;
        } = new();

        /// <inheritdoc />
        public IMetadata CreateMetadata()
        {
            // Create metadata directory.
            var result = new PE.DotNet.Metadata.Metadata
            {
                VersionString = _versionString,
            };

            // Create and add streams.
            var tablesStream = AddIfNotEmpty<TablesStream>(result, TablesStream);
            var stringsStream =  AddIfNotEmpty<StringsStream>(result, StringsStream);
            AddIfNotEmpty<UserStringsStream>(result, UserStringsStream);
            var guidStream = AddIfNotEmpty<GuidStream>(result, GuidStream);
            var blobStream = AddIfNotEmpty<BlobStream>(result, BlobStream);

            // Update index sizes.
            tablesStream.StringIndexSize = stringsStream?.IndexSize ?? IndexSize.Short;
            tablesStream.GuidIndexSize = guidStream?.IndexSize ?? IndexSize.Short;
            tablesStream.BlobIndexSize = blobStream?.IndexSize ?? IndexSize.Short;

            return result;
        }

        private static TStream AddIfNotEmpty<TStream>(IMetadata metadata, IMetadataStreamBuffer streamBuffer)
            where TStream : class, IMetadataStream
        {
            if (!streamBuffer.IsEmpty)
            {
                var stream = streamBuffer.CreateStream();
                metadata.Streams.Add(stream);
                return (TStream) stream;
            }

            return null;
        }

    }
}
