using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class PropertyMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;

        public PropertyMap(TypeDefinition parent)
            : base(null, new MetadataToken(MetadataTokenType.PropertyMap))
        {
            _parent = new LazyValue<TypeDefinition>(parent);
            Properties = new DelegatedMemberCollection<PropertyMap, PropertyDefinition>(
                    this, 
                    GetPropertyOwner,
                    SetPropertyOwner);
        }

        internal PropertyMap(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
            _parent = new LazyValue<TypeDefinition>(() =>
            {
                var typeTable = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                return (TypeDefinition) typeTable.GetMemberFromRow(image, typeTable.GetRow((int) (row.Column1 - 1)));
            });

            Properties = new RangedMemberCollection<PropertyMap, PropertyDefinition>(this, MetadataTokenType.Property,
                1, GetPropertyOwner, SetPropertyOwner);
        }

        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public Collection<PropertyDefinition> Properties
        {
            get;
            private set;
        }

        private static PropertyMap GetPropertyOwner(PropertyDefinition property)
        {
            return property.PropertyMap;
        }

        private static void SetPropertyOwner(PropertyDefinition property, PropertyMap owner)
        {
            property.PropertyMap = owner;
            property.Image = owner == null ? null : owner.Image;
        }
    }
}