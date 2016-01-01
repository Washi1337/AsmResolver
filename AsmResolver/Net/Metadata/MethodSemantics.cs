using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class MethodSemanticsTable : MetadataTable<MethodSemantics>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodSemantics; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof(ushort) +
                   (uint)TableStream.GetTable<MethodDefinition>().IndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize;
        }

        protected override MethodSemantics ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new MethodSemantics(TableStream.StreamHeader.MetadataHeader, token, new MetadataRow<ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetTable<MethodDefinition>().IndexSize),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, MethodSemantics member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Attributes;
            row.Column2 = member.Method.MetadataToken.Rid;
            row.Column3 = TableStream.GetIndexEncoder(CodedIndex.HasSemantics)
                .EncodeToken(member.Association.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, MethodSemantics member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.GetTable<MethodDefinition>().IndexSize, row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize, row.Column3);
        }
    }

    public class MethodSemantics : MetadataMember<MetadataRow<ushort, uint, uint>>
    {
        private readonly LazyValue<MethodDefinition> _method;
        private readonly LazyValue<IHasSemantics> _association;

        internal MethodSemantics(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            Attributes = (MethodSemanticsAttributes)row.Column1;
            
            _method = new LazyValue<MethodDefinition>(() => tableStream.GetTable<MethodDefinition>()[(int)(row.Column2 - 1)]);

            _association = new LazyValue<IHasSemantics>(() =>
            {
                var associationToken = tableStream.GetIndexEncoder(CodedIndex.HasSemantics).DecodeIndex(row.Column3);
                return associationToken.Rid != 0 ? (IHasSemantics)tableStream.ResolveMember(associationToken) : null;
            });
        }

        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        public MethodDefinition Method
        {
            get { return _method.Value; }
            set { _method.Value = value; }
        }

        public IHasSemantics Association
        {
            get { return _association.Value; }
            set { _association.Value = value; }
        }
    }
}
