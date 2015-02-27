using System.Collections.Generic;
using System.Data;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ParameterDefinitionTable : MetadataTable<ParameterDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Param; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +                  // Attributes
                   sizeof (ushort) +                  // Sequence
                   (uint)TableStream.StringIndexSize; // Name
        }

        protected override ParameterDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            var row = new MetadataRow<ushort, ushort, uint>
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadUInt16(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize)
            };
            return new ParameterDefinition(Header, token, row);
        }

        protected override void UpdateMember(NetBuildingContext context, ParameterDefinition member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Attributes;
            row.Column2 = (ushort)member.Sequence;
            row.Column3 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
        }

        protected override void WriteMember(WritingContext context, ParameterDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
        }
    }

    public class ParameterDefinition : MetadataMember<MetadataRow<ushort, ushort,uint>>, IHasConstant, IHasCustomAttribute, IHasFieldMarshal, ICollectionItem
    {
        private CustomAttributeCollection _customAttributes;

        public ParameterDefinition(MetadataHeader header, MetadataToken token, MetadataRow<ushort, ushort, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();

            Attributes = (ParameterAttributes)row.Column1;
            Sequence = row.Column2;
            Name = stringStream.GetStringByOffset(row.Column3);
        }

        public ParameterAttributes Attributes
        {
            get;
            set;
        }

        public int Sequence
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Constant Constant
        {
            get;
            set;
        }

        public MethodDefinition Method
        {
            get;
            private set;
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

        public FieldMarshal FieldMarshal
        {
            get;
            set;
        }

        object ICollectionItem.Owner
        {
            get { return Method; }
            set { Method = value as MethodDefinition; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}