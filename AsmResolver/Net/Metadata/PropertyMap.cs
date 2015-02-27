using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class PropertyMapTable : MetadataTable<PropertyMap>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.PropertyMap; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<TypeDefinition>().IndexSize +
                   (uint)TableStream.GetTable<PropertyDefinition>().IndexSize;
        }

        protected override PropertyMap ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new PropertyMap(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<TypeDefinition>().IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetTable<PropertyDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, PropertyMap member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Parent.MetadataToken.Rid;
            // TODO: update property list
        }

        protected override void WriteMember(WritingContext context, PropertyMap member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetTable<TypeDefinition>().IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetTable<PropertyDefinition>().IndexSize, row.Column2);
        }
    }

    public class PropertyMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private RangedDefinitionCollection<PropertyDefinition> _properties;

        public PropertyMap(TypeDefinition parent)
            : base(null, new MetadataToken(MetadataTokenType.PropertyMap), new MetadataRow<uint, uint>())
        {
            Parent = parent;
        }

        internal PropertyMap(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            Parent = tableStream.GetTable<TypeDefinition>()[(int)row.Column1 - 1];
        }

        public TypeDefinition Parent
        {
            get;
            private set;
        }

        public RangedDefinitionCollection<PropertyDefinition> Properties
        {
            get
            {
                return _properties ??
                       (_properties = RangedDefinitionCollection<PropertyDefinition>.Create(Header, this,
                           x => (int)x.MetadataRow.Column2));
            }
        }
    }
}
