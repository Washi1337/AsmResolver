using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class InterfaceImplementationTable : MetadataTable<InterfaceImplementation>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.InterfaceImpl; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<TypeDefinition>().IndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize;
        }

        protected override InterfaceImplementation ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new InterfaceImplementation(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<TypeDefinition>().IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, InterfaceImplementation member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Class.MetadataToken.Rid;
            row.Column2 = TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(member.Interface.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, InterfaceImplementation member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetTable<TypeDefinition>().IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column2);
        }
    }

    public class InterfaceImplementation : MetadataMember<MetadataRow<uint, uint>>, IHasCustomAttribute
    {
        private CustomAttributeCollection _customAttributes;

        public InterfaceImplementation(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            Class = tableStream.GetTable<TypeDefinition>()[(int)(row.Column1 - 1)];

            var interfaceToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column2);
            if (interfaceToken.Rid != 0)
                Interface = (ITypeDefOrRef)tableStream.ResolveMember(interfaceToken);
        }

        public TypeDefinition Class
        {
            get;
            set;
        }

        public ITypeDefOrRef Interface
        {
            get;
            set;
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                _customAttributes = new CustomAttributeCollection(this);
                return _customAttributes;
            }
        }
    }
}
