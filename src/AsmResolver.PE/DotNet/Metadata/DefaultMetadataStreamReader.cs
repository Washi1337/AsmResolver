using System;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Pdb;
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
        /// <summary>
        /// Gets a default instance
        /// </summary>
        public static DefaultMetadataStreamReader Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public IMetadataStream ReadStream(MetadataReaderContext context,
            MetadataStreamReaderFlags flags,
            MetadataStreamHeader header,
            ref BinaryStreamReader reader)
        {
            // The CLR performs a case-insensitive comparison for the names of the streams when ENC metadata is present.
            var comparisonKind = (flags & MetadataStreamReaderFlags.IsEnc) != 0
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (string.Equals(header.Name, TablesStream.CompressedStreamName, comparisonKind) ||
                string.Equals(header.Name, TablesStream.EncStreamName, comparisonKind))
            {
                bool forceLargeColumns = (flags & MetadataStreamReaderFlags.IsEnc) != 0 &&
                                         (flags & MetadataStreamReaderFlags.HasJtdStream) != 0;
                return new SerializedTableStream(context, header.Name, reader)
                    { ForceLargeColumns = forceLargeColumns };
            }
            if (string.Equals(header.Name, StringsStream.DefaultName, comparisonKind))
                return new SerializedStringsStream(header.Name, reader);
            if (string.Equals(header.Name, UserStringsStream.DefaultName, comparisonKind))
                return new SerializedUserStringsStream(header.Name, reader);
            if (string.Equals(header.Name, BlobStream.DefaultName, comparisonKind))
                return new SerializedBlobStream(header.Name, reader);
            if (string.Equals(header.Name, GuidStream.DefaultName, comparisonKind))
                return new SerializedGuidStream(header.Name, reader);
            // Always perform a case-sensitive comparison for PdbStream since it is not a stream read by the CLR
            if (header.Name == PdbStream.DefaultName)
                return new SerializedPdbStream(header.Name, reader);
            return new CustomMetadataStream(header.Name, DataSegment.FromReader(ref reader));
        }
    }
}
