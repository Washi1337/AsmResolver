using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a constraint on a substitution put onto a type used as an argument for a generic parameter.  
    /// </summary>
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
                return table.TryGetRow((int) (row.Column1 - 1), out var paramRow)
                    ? (GenericParameter) table.GetMemberFromRow(image, paramRow)
                    : null;
            });

            _constraint = new LazyValue<ITypeDefOrRef>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.TypeDefOrRef);
                var constraintToken = encoder.DecodeIndex(row.Column2);
                return image.TryResolveMember(constraintToken, out var member) ? (ITypeDefOrRef) member : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image => _owner.IsInitialized && _owner.Value != null
            ? _owner.Value.Image 
            : _image;

        /// <summary>
        /// Gets the generic parameter on which the constraint was put on.
        /// </summary>
        public GenericParameter Owner
        {
            get => _owner.Value;
            internal set
            {
                _owner.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the type constraint that is put onto the generic parameter. 
        /// </summary>
        public ITypeDefOrRef Constraint
        {
            get => _constraint.Value;
            set => _constraint.Value = value;
        }

        public override string ToString()
        {
            return $"({Constraint}) {Owner}";
        }
    }
}
