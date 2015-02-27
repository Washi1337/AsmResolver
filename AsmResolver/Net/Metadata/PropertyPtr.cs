using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class PropertyPtrTable : MetadataTable<PropertyPtr>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.PropertyPtr; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<PropertyDefinition>().IndexSize;
        }

        protected override PropertyPtr ReadMember(MetadataToken token, ReadingContext context)
        {
            return new PropertyPtr(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable<PropertyDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, PropertyPtr member)
        {
            member.MetadataRow.Column1 = member.Property.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, PropertyPtr member)
        {
            context.Writer.WriteIndex(TableStream.GetTable<PropertyDefinition>().IndexSize, member.MetadataRow.Column1);
        }
    }

    public class PropertyPtr : MetadataMember<MetadataRow<uint>>
    {
        internal PropertyPtr(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Property = header.GetStream<TableStream>().GetTable<PropertyDefinition>()[(int)(row.Column1 - 1)];
        }

        public PropertyDefinition Property
        {
            get;
            set;
        }
    }
}
