using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ClassLayoutTable : MetadataTable<ClassLayout>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ClassLayout; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   sizeof (uint) +
                   (uint)TableStream.GetTable<TypeDefinition>().IndexSize;
        }

        protected override ClassLayout ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new ClassLayout(Header, token, new MetadataRow<ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.GetTable<TypeDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, ClassLayout member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.PackingSize;
            row.Column2 = member.ClassSize;
            row.Column3 = member.Parent.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, ClassLayout member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt16(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteIndex(TableStream.GetTable<TypeDefinition>().IndexSize, row.Column3);
        }
    }

    public class ClassLayout : MetadataMember<MetadataRow<ushort,uint,uint>>
    {
        public ClassLayout(TypeDefinition parent, uint classSize, ushort packingSize)
            : base(null, new MetadataToken(MetadataTokenType.ClassLayout), new MetadataRow<ushort, uint, uint>())
        {
            Parent = parent;
            ClassSize = classSize;
            PackingSize = packingSize;
        }

        public ClassLayout(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            PackingSize = row.Column1;
            ClassSize = row.Column2;

            var tableStream = header.GetStream<TableStream>();
            Parent = tableStream.GetTable<TypeDefinition>()[(int)(row.Column3- 1)];
        }

        public ushort PackingSize
        {
            get;
            set;
        }

        public uint ClassSize
        {
            get;
            set;
        }

        public TypeDefinition Parent
        {
            get;
            set;
        }
    }
}
