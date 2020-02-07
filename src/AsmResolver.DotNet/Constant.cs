using AsmResolver.DotNet.Signatures;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a literal value that is assigned to a field, parameter or property.
    /// </summary>
    public class Constant : IMetadataMember
    {
        private readonly LazyVariable<IHasConstant> _parent;
        private readonly LazyVariable<DataBlobSignature> _value;

        protected Constant(MetadataToken token)
        {
            MetadataToken = token;
            _parent = new LazyVariable<IHasConstant>(GetParent);
            _value = new LazyVariable<DataBlobSignature>(GetValue);
        }

        public Constant(ElementType type, DataBlobSignature value)
        {
            Type = type;
            Value = value;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the type of constant that is stored in the blob stream. 
        /// </summary>
        /// <remarks>This field must always be a value-type.</remarks>
        public ElementType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member that is assigned a constant.
        /// </summary>
        public IHasConstant Parent
        {
            get => _parent.Value;
            internal set => _parent.Value = value;
        }

        /// <summary>
        /// Gets or sets the serialized literal value.
        /// </summary>
        public DataBlobSignature Value
        {
            get => _value.Value;
            set => _value.Value = value;
        }

        /// <summary>
        /// Obtains the owner of the constant.
        /// </summary>
        /// <returns>The parent.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual IHasConstant GetParent() => null;


        /// <summary>
        /// Obtains the literal value of the constant.
        /// </summary>
        /// <returns>The value.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Value"/> property.
        /// </remarks>
        protected virtual DataBlobSignature GetValue() => null;
    }
}