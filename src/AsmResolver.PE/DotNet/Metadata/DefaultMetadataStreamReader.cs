using System;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a default implementation for the <see cref="DefaultMetadataStreamReader"/> interface, which is able
    /// to read all metadata streams defined by the ECMA-335, and creates instances of <see cref="CustomMetadataStream"/>
    /// when an unknown metadata stream was read.
    /// </summary>
    public class DefaultMetadataStreamReader : IMetadataStreamReader
    {
        /// <inheritdoc />
        public IMetadataStream? ReadStream(PEReaderContext context, MetadataStreamHeader header,
            ref BinaryStreamReader reader)
        {
            switch (header.Name)
            {
                case TablesStream.CompressedStreamName:
                case TablesStream.EncStreamName:
                    return new SerializedTableStream(context, header.Name, reader);

                case StringsStream.DefaultName:
                    return new SerializedStringsStream(header.Name, reader);

                case UserStringsStream.DefaultName:
                    return new SerializedUserStringsStream(header.Name, reader);

                case BlobStream.DefaultName:
                    return new SerializedBlobStream(header.Name, reader);

                case GuidStream.DefaultName:
                    return new SerializedGuidStream(header.Name, reader);

                default:
                    return new CustomMetadataStream(header.Name, DataSegment.FromReader(ref reader));
            }
        }
    }
}
