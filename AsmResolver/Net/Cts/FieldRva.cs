using System;
using System.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata associated to a field containing information about the initial value of the field.
    /// </summary>
    public class FieldRva : MetadataMember<MetadataRow<DataSegment, uint>>
    {
        private readonly LazyValue<byte[]> _data;
        private readonly LazyValue<FieldDefinition> _field;
        private MetadataImage _image;

        public FieldRva(byte[] data)
            : base(new MetadataToken(MetadataTokenType.FieldRva))
        {
            _field = new LazyValue<FieldDefinition>();
            _data = new LazyValue<byte[]>(data);
        }

        internal FieldRva(MetadataImage image, MetadataRow<DataSegment, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            _field = new LazyValue<FieldDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Field);
                var fieldRow = table.GetRow((int) (row.Column2 - 1));
                return (FieldDefinition) table.GetMemberFromRow(image, fieldRow);
            });

            _data = new LazyValue<byte[]>(() => row.Column1.Data);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _field.IsInitialized && _field.Value != null 
            ? _field.Value.Image 
            : _image;

        /// <summary>
        /// Gets the field that is associated to this extra metadata.
        /// </summary>
        public FieldDefinition Field
        {
            get { return _field.Value; }
            internal set
            {
                _field.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the serialized initial value of the field.  
        /// </summary>
        public byte[] Data
        {
            get => _data.Value;
            set => _data.Value = value;
        }

        /// <summary>
        /// Determines the size of the data according to the type of the associated field.
        /// </summary>
        /// <returns></returns>
        public int GetDataSize()
        {
            var signature = Field.Signature;
            if (signature?.FieldType == null)
                return 0;

            if (signature.FieldType is MsCorLibTypeSignature corlibType)
            {
                switch (corlibType.ElementType)
                {
                    case ElementType.Boolean:
                    case ElementType.I1:
                    case ElementType.U1:
                        return sizeof (byte);
                    case ElementType.I2:
                    case ElementType.U2:
                        return sizeof (ushort);
                    case ElementType.I4:
                    case ElementType.U4:
                    case ElementType.R4:
                        return sizeof (uint);
                    case ElementType.I8:
                    case ElementType.U8:
                    case ElementType.R8:
                        return sizeof (ulong);
                    default:
                        return 0;
                }
            }

            if (!(signature.FieldType is TypeDefOrRefSignature typeDefOrRef))
                return 0;
            if (!(typeDefOrRef.Type is TypeDefinition definition) || definition.ClassLayout == null)
                return 0;
            return (int) definition.ClassLayout.ClassSize;
        }

        /// <summary>
        /// Attempts to deserialize the stored data. 
        /// </summary>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns>The deserialized data.</returns>
        public object InterpretData(TypeSignature type)
        {
            if (type is SzArrayTypeSignature arrayType)
                return InterpretAsArray(arrayType.BaseType);
            return InterpretData(type.ElementType);
        }

        /// <summary>
        /// Attempts to deserialize the stored data as an array of elements of a particular type.
        /// </summary>
        /// <param name="elementType">The type of each element in the array.</param>
        /// <returns>The deserialized data.</returns>
        public IEnumerable InterpretAsArray(TypeSignature elementType)
        {
            var corlibType = Image.TypeSystem.GetMscorlibType(elementType);
            if (corlibType == null)
                ThrowUnsupportedElementType(elementType);
            return InterpretAsArray(corlibType.ElementType);
        }

        /// <summary>
        /// Attempts to deserialize the stored data as an array of elements of a particular type.
        /// </summary>
        /// <param name="elementType">The type of each element in the array.</param>
        /// <returns>The deserialized data.</returns>
        public IEnumerable InterpretAsArray(ElementType elementType)
        {
            var reader = new MemoryStreamReader(Data);
            while (reader.Position < reader.Length)
                yield return ReadElement(reader, elementType);
        }

        /// <summary>
        /// Attempts to deserialize a single element of a particular type from the stored data.
        /// </summary>
        /// <param name="elementType">The type of the element to read.</param>
        /// <returns>The deserialized object.</returns>
        public object InterpretData(ElementType elementType)
        {
            var reader = new MemoryStreamReader(Data);
            return ReadElement(reader, elementType);
        }

        private static object ReadElement(IBinaryStreamReader reader, ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.I1:
                    return reader.ReadSByte();
                case ElementType.I2:
                    return reader.ReadInt16();
                case ElementType.I4:
                    return reader.ReadInt32();
                case ElementType.I8:
                    return reader.ReadInt64();
                case ElementType.U1:
                    return reader.ReadByte();
                case ElementType.U2:
                    return reader.ReadUInt16();
                case ElementType.U4:
                    return reader.ReadUInt32();
                case ElementType.U8:
                    return reader.ReadUInt64();
                case ElementType.R4:
                    return reader.ReadSingle();
                case ElementType.R8:
                    return reader.ReadDouble();
            }

            ThrowUnsupportedElementType(elementType);
            return null;
        }

        private static void ThrowUnsupportedElementType(object elementType)
        {
            throw new NotSupportedException("Invalid or unsupported element type " + elementType + ".");
        }
    }
}