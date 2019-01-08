
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single type parameter associated to a generic member.
    /// </summary>
    public class GenericParameter : MetadataMember<MetadataRow<ushort, GenericParameterAttributes, uint, uint>>
    {
        private readonly LazyValue<IGenericParameterProvider> _owner;
        private readonly LazyValue<string> _name;
        private MetadataImage _image;

        public GenericParameter(int index, string name, GenericParameterAttributes attributes = GenericParameterAttributes.NonVariant)
            : base(new MetadataToken(MetadataTokenType.GenericParam))
        {
            _owner = new LazyValue<IGenericParameterProvider>();
            Index = index;
            _name = new LazyValue<string>(name);
            Attributes = attributes;
            Constraints = new GenericParameterConstraintCollection(this);
        }

        internal GenericParameter(MetadataImage image, MetadataRow<ushort, GenericParameterAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Index = row.Column1;
            Attributes = row.Column2;

            _owner = new LazyValue<IGenericParameterProvider>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.TypeOrMethodDef);
                var ownerToken = encoder.DecodeIndex(row.Column3);
                return image.TryResolveMember(ownerToken, out var member) ? (IGenericParameterProvider) member : null;
            });

            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column4));
            Constraints = new GenericParameterConstraintCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _owner.IsInitialized && _owner.Value != null 
            ? _owner.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the index of the generic parameter.
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the generic parameter.
        /// </summary>
        public GenericParameterAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member that defines the generic parameter.
        /// </summary>
        public IGenericParameterProvider Owner
        {
            get => _owner.Value;
            internal set
            {
                _owner.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the name of the generic parameter.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets a collection of constraints on the types that can be substituted into this generic parameter.
        /// </summary>
        public GenericParameterConstraintCollection Constraints
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
