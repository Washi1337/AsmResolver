using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class FieldRvaTable : MetadataTable<FieldRva>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldRva; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   (uint)TableStream.GetTable<FieldDefinition>().IndexSize;
        }

        protected override FieldRva ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new FieldRva(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.GetTable<FieldDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, FieldRva member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Rva;
            row.Column2 = member.Field.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, FieldRva member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable<FieldDefinition>().IndexSize, row.Column2);
        }
    }

    public class FieldRva : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<byte[]> _data;
        private readonly LazyValue<FieldDefinition> _field;

        public FieldRva(FieldDefinition field, byte[] data)
            : base(null, new MetadataToken(MetadataTokenType.FieldRva), new MetadataRow<uint, uint>())
        {
            _field = new LazyValue<FieldDefinition>(field);
            _data = new LazyValue<byte[]>(data);
        }

        internal FieldRva(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            Rva = row.Column1;

            _field = new LazyValue<FieldDefinition>(() => 
                header.GetStream<TableStream>().GetTable<FieldDefinition>()[(int)(row.Column2 - 1)]);

            _data = new LazyValue<byte[]>(() =>
            {
                var assembly = Header.NetDirectory.Assembly;
                var reader = assembly.ReadingContext.Reader.CreateSubReader(assembly.RvaToFileOffset(Rva), GetDataSize());
                return reader.ReadBytes((int)reader.Length);
            });
        }

        public uint Rva
        {
            get;
            set;
        }

        public FieldDefinition Field
        {
            get { return _field.Value; }
            set { _field.Value = value; }
        }

        public byte[] Data
        {
            get { return _data.Value; }
            set { _data.Value = value; }
        }

        public int GetDataSize()
        {
            var signature = Field.Signature;
            if (signature == null || signature.FieldType == null)
                return 0;

            var corlibType = signature.FieldType as MsCorLibTypeSignature;
            if (corlibType != null)
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
                        return sizeof (uint);
                    case ElementType.I8:
                    case ElementType.U8:
                        return sizeof (ulong);
                    case ElementType.I:
                    case ElementType.U:
                        // TODO;
                    default:
                        return 0;
                }
            }

            var typeDefOrRef = signature.FieldType as TypeDefOrRefSignature;
            if (typeDefOrRef == null)
                return 0;
            var definition = typeDefOrRef.Type as TypeDefinition;
            if (definition == null || definition.ClassLayout == null)
                return 0;
            return (int)definition.ClassLayout.ClassSize;
        }

        public object InterpretData(TypeSignature type)
        {
            var arrayType = type as SzArrayTypeSignature;
            if (arrayType != null)
                return InterpretAsArray(arrayType.BaseType.ElementType);
            return InterpretData(type.ElementType);
        }

        public IEnumerable InterpretAsArray(TypeSignature elementType)
        {
            return InterpretAsArray(elementType.ElementType);
        }

        public IEnumerable InterpretAsArray(ElementType elementType)
        {
            var reader = new MemoryStreamReader(Data);
            while (reader.Position < reader.Length)
                yield return ReadElement(reader, elementType);
        }

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
            }

            throw new NotSupportedException("Invalid or unsupported element type " + elementType + ".");
        }
    }
}
