using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.PE.Platforms;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IFieldRvaDataReader"/> interface.
    /// </summary>
    public class FieldRvaDataReader : IFieldRvaDataReader
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="FieldRvaDataReader"/> class.
        /// </summary>
        public static FieldRvaDataReader Instance { get; } = new();

        /// <inheritdoc />
        public ISegment? ResolveFieldData(
            IErrorListener listener,
            Platform platform,
            DotNetDirectory directory,
            in FieldRvaRow fieldRvaRow)
        {
            if (fieldRvaRow.Data.IsBounded)
                return fieldRvaRow.Data.GetSegment();

            var metadata = directory.Metadata;
            if (metadata is null)
            {
                listener.BadImage(".NET directory does not contain a metadata directory.");
                return null;
            }

            if (!metadata.TryGetStream<TablesStream>(out var tablesStream))
            {
                listener.BadImage("Metadata does not contain a tables stream.");
                return null;
            }

            var table = tablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);
            if (fieldRvaRow.Field > table.Count)
            {
                listener.BadImage("FieldRva row has an invalid Field column value.");
                return null;
            }

            var field = table.GetByRid(fieldRvaRow.Field);
            int valueSize = DetermineFieldSize(listener, platform, directory, field);

            if (fieldRvaRow.Data.CanRead)
            {
                var reader = fieldRvaRow.Data.CreateReader();
                return DataSegment.FromReader(ref reader, valueSize);
            }

            if (fieldRvaRow.Data is PESegmentReference {IsValidAddress: true})
            {
                // We are reading from a virtual segment that is resized at runtime, assume zeroes.
                var segment = new ZeroesSegment((uint) valueSize);
                segment.UpdateOffsets(new RelocationParameters(fieldRvaRow.Data.Offset, fieldRvaRow.Data.Rva));
                return segment;
            }

            listener.NotSupported("FieldRva row has an invalid or unsupported data column.");
            return null;
        }

        private int DetermineFieldSize(IErrorListener listener, Platform platform, DotNetDirectory directory, in FieldDefinitionRow field)
        {
            if (!directory.Metadata!.TryGetStream<BlobStream>(out var blobStream)
                || !blobStream.TryGetBlobReaderByIndex(field.Signature, out var reader))
            {
                return 0;
            }

            reader.ReadByte(); // calling convention attributes.

            while (true)
            {
                switch ((ElementType)reader.ReadByte())
                {
                    case ElementType.Boolean:
                        return sizeof(bool);

                    case ElementType.Char:
                        return sizeof(char);

                    case ElementType.I1:
                        return sizeof(sbyte);

                    case ElementType.U1:
                        return sizeof(byte);

                    case ElementType.I2:
                        return sizeof(short);

                    case ElementType.U2:
                        return sizeof(ushort);

                    case ElementType.I4:
                        return sizeof(int);

                    case ElementType.U4:
                        return sizeof(uint);

                    case ElementType.I8:
                        return sizeof(long);

                    case ElementType.U8:
                        return sizeof(ulong);

                    case ElementType.R4:
                        return sizeof(float);

                    case ElementType.R8:
                        return sizeof(double);

                    case ElementType.ValueType:
                        return GetCustomTypeSize(directory.Metadata, ref reader);

                    case ElementType.I:
                    case ElementType.U:
                    case ElementType.Ptr:
                    case ElementType.FnPtr:
                        return directory.Flags.IsLoadedAs32Bit(platform) ? sizeof(uint) : sizeof(ulong);

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                        if (!reader.TryReadCompressedUInt32(out _))
                            return listener.BadImageAndReturn<int>("Invalid field signature.");
                        break;
                }
            }
        }

        private int GetCustomTypeSize(MetadataDirectory metadataDirectory, ref BinaryStreamReader reader)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex)
                || !metadataDirectory.TryGetStream<TablesStream>(out var tablesStream))
            {
                return 0;
            }

            var typeToken = tablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(codedIndex);

            if (typeToken.Table == TableIndex.TypeDef)
            {
                var classLayoutTable = tablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);
                if (classLayoutTable.TryGetRowByKey(2, typeToken.Rid, out var row))
                    return (int) row.ClassSize;
            }

            return 0;
        }
    }
}
