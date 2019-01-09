using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a map that binds a <see cref="TypeDefinition"/> to a collection of <see cref="PropertyDefinition"/>s.
    /// </summary>
    public class PropertyMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;
        private MetadataImage _image;

        public PropertyMap()
            : base(new MetadataToken(MetadataTokenType.PropertyMap))
        {
            _parent = new LazyValue<TypeDefinition>();
            Properties = new DelegatedMemberCollection<PropertyMap, PropertyDefinition>(
                    this, 
                    GetPropertyOwner,
                    SetPropertyOwner);
        }

        internal PropertyMap(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            _parent = new LazyValue<TypeDefinition>(() =>
            {
                var typeTable = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                return (TypeDefinition) typeTable.GetMemberFromRow(image, typeTable.GetRow((int) (row.Column1 - 1)));
            });

            Properties = new RangedMemberCollection<PropertyMap, PropertyDefinition>(this, MetadataTokenType.Property,
                1, GetPropertyOwner, SetPropertyOwner);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _parent.IsInitialized && _parent.Value != null 
            ? _parent.Value.Image
            : _image;

        /// <summary>
        /// Gets the type the property map was assigned to.
        /// </summary>
        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets a collection of properties the <see cref="Parent"/> type declares.
        /// </summary>
        public Collection<PropertyDefinition> Properties
        {
            get;
        }

        private static PropertyMap GetPropertyOwner(PropertyDefinition property)
        {
            return property.PropertyMap;
        }

        private static void SetPropertyOwner(PropertyDefinition property, PropertyMap owner)
        {
            property.PropertyMap = owner;
        }
    }
}