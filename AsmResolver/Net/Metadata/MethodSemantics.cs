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
            return new MethodSemantics(TableStream.StreamHeader.MetaDataHeader, token, new MetadataRow<ushort, uint, uint>()
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
        internal MethodSemantics(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            Attributes = (MethodSemanticsAttributes)row.Column1;

            var tableStream = header.GetStream<TableStream>();
            Method = tableStream.GetTable<MethodDefinition>()[(int)(row.Column2 - 1)];

            var associationToken = tableStream.GetIndexEncoder(CodedIndex.HasSemantics).DecodeIndex(row.Column3);
            if (associationToken.Rid != 0)
                Association = (IHasSemantics)tableStream.ResolveMember(associationToken);
        }

        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        public MethodDefinition Method
        {
            get;
            set;
        }

        public IHasSemantics Association
        {
            get;
            set;
        }
    }
}
