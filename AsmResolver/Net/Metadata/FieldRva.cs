using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

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
            row.Column1 = member.Rva; // TODO: update RVA ?
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
        internal FieldRva(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            Rva = row.Column1;
            Field = header.GetStream<TableStream>().GetTable<FieldDefinition>()[(int)(row.Column2 - 1)];
        }

        public uint Rva
        {
            get;
            set;
        }

        public FieldDefinition Field
        {
            get;
            set;
        }
    }
}
