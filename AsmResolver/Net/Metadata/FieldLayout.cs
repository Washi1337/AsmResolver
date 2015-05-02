using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class FieldLayoutTable : MetadataTable<FieldLayout>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldLayout; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof(uint) +
                   (uint)TableStream.GetTable<FieldDefinition>().IndexSize;
        }

        protected override FieldLayout ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new FieldLayout(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.GetTable<FieldDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, FieldLayout member)
        {
            var row = member.MetadataRow;

            row.Column1 = member.Offset;
            row.Column2 = member.Field.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, FieldLayout member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable<FieldDefinition>().IndexSize, row.Column2);
        }
    }

    public class FieldLayout : MetadataMember<MetadataRow<uint,uint>>
    {
        private readonly LazyValue<FieldDefinition> _field;

        internal FieldLayout(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            Offset = row.Column1;

            var tableStream = header.GetStream<TableStream>();
            _field = new LazyValue<FieldDefinition>(() => tableStream.GetTable<FieldDefinition>()[(int)(row.Column2 - 1)]);
        }

        public uint Offset
        {
            get;
            set;
        }

        public FieldDefinition Field
        {
            get { return _field.Value; }
            set { _field.Value = value; }
        }
    }
}
