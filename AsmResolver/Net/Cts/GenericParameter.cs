
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
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
                IMetadataMember member;
                return image.TryResolveMember(ownerToken, out member) ? (IGenericParameterProvider) member : null;
            });

            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column4));
            Constraints = new GenericParameterConstraintCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _owner.IsInitialized && _owner.Value != null ? _owner.Value.Image : _image; }
        }

        public int Index
        {
            get;
            set;
        }

        public GenericParameterAttributes Attributes
        {
            get;
            set;
        }

        public IGenericParameterProvider Owner
        {
            get { return _owner.Value; }
            internal set
            {
                _owner.Value = value;
                _image = null;
            }
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public GenericParameterConstraintCollection Constraints
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
