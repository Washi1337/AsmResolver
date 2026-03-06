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

            var streams = metadata.GetImpliedStreamSelection();

            // Validate we have at least a tables stream (to find fielddefs and classlayouts)
            // and blob stream (to parse the field signature).
            if (streams.TablesStream is null)
            {
                listener.BadImage("Metadata does not contain a tables stream.");
                return null;
            }

            if (streams.BlobStream is null)
            {
                listener.BadImage("Metadata does not contain a blob stream.");
                return null;
            }

            // Is the referenced field a valid index?
            var table = streams.TablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);
            if (fieldRvaRow.Field > table.Count)
            {
                listener.BadImage("FieldRva row has an invalid Field column value.");
                return null;
            }

            // Determine the size of the field.
            var field = table.GetByRid(fieldRvaRow.Field);
            int valueSize = DetermineFieldSize(
                listener,
                platform,
                directory.Flags,
                in streams,
                in field
            );

            if (fieldRvaRow.Data.CanRead)
            {
                var reader = fieldRvaRow.Data.CreateReader();
                return DataSegment.FromReader(ref reader, valueSize);
            }

            if (fieldRvaRow.Data is PESegmentReference {IsValidAddress: true})
            {
                // We are reading from a virtual segment that is resized at runtime, assume initialized data (zeroes).
                var segment = new ZeroesSegment((uint) valueSize);
                segment.UpdateOffsets(new RelocationParameters(fieldRvaRow.Data.Offset, fieldRvaRow.Data.Rva));
                return segment;
            }

            listener.NotSupported("FieldRva row has an invalid or unsupported data column.");
            return null;
        }

        private int DetermineFieldSize(
            IErrorListener listener,
            Platform platform,
            DotNetDirectoryFlags directoryFlags,
            in MetadataStreamSelection streams,
            in FieldDefinitionRow field)
        {
            // Follow field signature index.
            if (!streams.BlobStream!.TryGetBlobReaderByIndex(field.Signature, out var reader))
                return listener.BadImageAndReturn<int>($"Invalid field signature index 0x{field.Signature:X8}.");

            // Check if we're actually a field signature.
            if (!reader.CanRead(sizeof(byte)))
                return listener.BadImageAndReturn<int>("Expected a field signature header.");
            byte attributes = reader.ReadByte();
            if ((attributes & 0xF) != 0x06)
                return listener.BadImageAndReturn<int>("Expected a field signature header.");

            // Minimal field type parser tuned to all supported value types.
            while (true)
            {
                if (!reader.CanRead(sizeof(byte)))
                    return listener.BadImageAndReturn<int>($"Expected an element type at blob signature index {reader.RelativeOffset}.");

                var elementType = (ElementType)reader.ReadByte();
                switch (elementType)
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
                        return GetCustomTypeSize(listener, streams.TablesStream!, ref reader);

                    case ElementType.I:
                    case ElementType.U:
                    case ElementType.Ptr:
                    case ElementType.FnPtr:
                        return directoryFlags.IsLoadedAs32Bit(platform) ? sizeof(uint) : sizeof(ulong);

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                        if (!reader.TryReadCompressedUInt32(out _))
                            return listener.BadImageAndReturn<int>("Invalid field signature.");
                        break;

                    default:
                        return listener.BadImageAndReturn<int>($"Unsupported field element type {elementType}.");
                }
            }
        }

        private static int GetCustomTypeSize(IErrorListener listener, TablesStream tablesStream, ref BinaryStreamReader reader)
        {
            // Read and decode the TypeDefOrRef index.
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return listener.BadImageAndReturn<int>($"Expected a TypeDefOrRef coded index at blob signature offset {reader.RelativeOffset}.");

            var typeToken = tablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(codedIndex);

            // Type needs to be a definition in the current module.
            if (typeToken.Table != TableIndex.TypeDef)
                return listener.BadImageAndReturn<int>($"Decoded TypeDefOrRef token {typeToken} at blob signature offset {reader.RelativeOffset} does not reference a type definition.");

            // Find a class layout that is associated to the type.
            var classLayoutTable = tablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);
            if (!classLayoutTable.TryGetRowByKey(2, typeToken.Rid, out var row))
                return listener.BadImageAndReturn<int>($"Field type {typeToken} does not have a class layout attached to it.");

            // Get the size.
            return (int) row.ClassSize;
        }
    }
}
