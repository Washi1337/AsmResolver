using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class GenericParameterConstraint : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<GenericParameter> _owner;
        private readonly LazyValue<ITypeDefOrRef> _constraint;
        private MetadataImage _image;

        public GenericParameterConstraint(ITypeDefOrRef constraint)
            : base(new MetadataToken(MetadataTokenType.GenericParamConstraint))
        {
            _owner = new LazyValue<GenericParameter>();
            _constraint = new LazyValue<ITypeDefOrRef>(constraint);
        }
        
        internal GenericParameterConstraint(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            _owner = new LazyValue<GenericParameter>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.GenericParam);
                MetadataRow paramRow;
                return table.TryGetRow((int) (row.Column1 - 1), out paramRow)
                    ? (GenericParameter) table.GetMemberFromRow(image, paramRow)
                    : null;
            });

            _constraint = new LazyValue<ITypeDefOrRef>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.TypeDefOrRef);
                var constraintToken = encoder.DecodeIndex(row.Column2);
                IMetadataMember member;
                return image.TryResolveMember(constraintToken, out member) ? (ITypeDefOrRef) member : null;
            });
        }

        public override MetadataImage Image
        {
            get { return _owner.IsInitialized && _owner.Value != null ? _owner.Value.Image : _image; }
        }

        public GenericParameter Owner
        {
            get { return _owner.Value; }
            internal set
            {
                _owner.Value = value;
                _image = null;
            }
        }

        public ITypeDefOrRef Constraint
        {
            get { return _constraint.Value; }
            set { _constraint.Value = value; }
        }

        public override string ToString()
        {
            return string.Format("({0}) {1}", Constraint, Owner);
        }
    }
}
