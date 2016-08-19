using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class GenericParameterTable : MetadataTable<GenericParameter>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.GenericParam; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   sizeof (ushort) +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize +
                   (uint)TableStream.StringIndexSize;
        }

        protected override GenericParameter ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new GenericParameter(Header, token,new MetadataRow<ushort, ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadUInt16(),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize),
                Column4 = reader.ReadIndex(TableStream.StringIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, GenericParameter member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Index;
            row.Column2 = (ushort)member.Attributes;
            row.Column3 = TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef)
                .EncodeToken(member.Owner.MetadataToken);
            row.Column4 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
        }

        protected override void WriteMember(WritingContext context, GenericParameter member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize, row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
        }
    }

    public class GenericParameter : MetadataMember<MetadataRow<ushort, ushort, uint, uint>>
    {
        private readonly LazyValue<IGenericParameterProvider> _owner;
        private readonly LazyValue<string> _name;

        internal GenericParameter(MetadataHeader header, MetadataToken token, MetadataRow<ushort, ushort, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            Index = row.Column1;
            Attributes = (GenericParameterAttributes)row.Column2;

            _owner = new LazyValue<IGenericParameterProvider>(() =>
            {
                var ownerToken = tableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).DecodeIndex(row.Column3);
                return ownerToken.Rid != 0 ? (IGenericParameterProvider)tableStream.ResolveMember(ownerToken) : null;
            });

            _name = new LazyValue<string>(() => header.GetStream<StringStream>().GetStringByOffset(row.Column4));
        }

        public int Index
        {
            get;
            set;
        }

        public GenericParameterAttributes Attributes
        {
            get;
            set;
        }

        public IGenericParameterProvider Owner
        {
            get { return _owner.Value; }
            set { _owner.Value = value; }
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
