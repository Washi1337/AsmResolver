using System;
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
        public IMetadataStream ReadStream(PEReaderContext context, MetadataStreamHeader header,
            IBinaryStreamReader reader)
        {
            switch (header.Name)
            {
                case TablesStream.CompressedStreamName:
                case TablesStream.EncStreamName:
                    return new SerializedTableStream(context, header.Name,DataSegment.FromReader(reader));
                    
                case StringsStream.DefaultName:
                    return new SerializedStringsStream(header.Name, DataSegment.FromReader(reader));

                case UserStringsStream.DefaultName:
                    return new SerializedUserStringsStream(header.Name, DataSegment.FromReader(reader));

                case BlobStream.DefaultName:
                    return new SerializedBlobStream(header.Name, DataSegment.FromReader(reader));

                case GuidStream.DefaultName:
                    return new SerializedGuidStream(header.Name, DataSegment.FromReader(reader));

                default:
                    return new CustomMetadataStream(header.Name, DataSegment.FromReader(reader));
            }
        }
    }
}