using System;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IFieldRvaDataReader"/> interface.
    /// </summary>
    public class FieldRvaDataReader : IFieldRvaDataReader
    {
        private readonly TablesStream _tablesStream;
        private readonly BlobStream _blobStream;

        /// <summary>
        /// Creates a new instance of the <see cref="FieldRvaDataReader"/> class.
        /// </summary>
        /// <param name="metadata">The metadata directory containing the Field and Field RVA table.</param>
        public FieldRvaDataReader(IMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            _tablesStream = metadata.GetStream<TablesStream>();
            _blobStream = metadata.GetStream<BlobStream>();
        }

        /// <inheritdoc />
        public ISegment ResolveFieldData(FieldRvaRow fieldRvaRow)
        {
            if (fieldRvaRow.Data is null)
                return null;
            
            if (fieldRvaRow.Data.IsBounded)
                return fieldRvaRow.Data.GetSegment();

            if (fieldRvaRow.Data.CanRead)
            {
                var table = _tablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);

                if (fieldRvaRow.Field > table.Count)
                    throw new ArgumentException("Invalid Field column value.");

                var field = table.GetByRid(fieldRvaRow.Field);
                int valueSize = DetermineFieldSize(field);
                
                var reader = fieldRvaRow.Data.CreateReader();
                return DataSegment.FromReader(reader, valueSize);
            }

            throw new NotSupportedException("Invalid or unsupported data column.");
        }

        private int DetermineFieldSize(in FieldDefinitionRow field)
        {
            var reader = _blobStream.GetBlobReaderByIndex(field.Signature);
            
            reader.ReadByte(); // calling convention attributes.
            var elementType = (ElementType) reader.ReadByte();
            return elementType switch
            {
                ElementType.Boolean => sizeof(bool),
                ElementType.Char => sizeof(char),
                ElementType.I1 => sizeof(sbyte),
                ElementType.U1 => sizeof(byte),
                ElementType.I2 => sizeof(short),
                ElementType.U2 => sizeof(ushort),
                ElementType.I4 => sizeof(int),
                ElementType.U4 => sizeof(uint),
                ElementType.I8 => sizeof(long),
                ElementType.U8 => sizeof(ulong),
                ElementType.R4 => sizeof(float),
                ElementType.R8 => sizeof(double),
                ElementType.ValueType => GetCustomTypeSize(reader),
                ElementType.Class => GetCustomTypeSize(reader),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private int GetCustomTypeSize(IBinaryStreamReader reader)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return 0;

            var typeToken = _tablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(codedIndex);
            
            if (typeToken.Table == TableIndex.TypeDef)
            {
                var classLayoutTable = _tablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);
                if (classLayoutTable.TryGetRowByKey(2, typeToken.Rid, out var row))
                    return (int) row.ClassSize;
            }

            return 0;
        }
    }
}