using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a literal value that is assigned to a field, parameter or property.
    /// </summary>
    public class Constant : MetadataMember
    {
        private readonly LazyVariable<IHasConstant?> _parent;
        private readonly LazyVariable<DataBlobSignature?> _value;

        /// <summary>
        /// Initializes the constant with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected Constant(MetadataToken token)
            : base(token)
        {
            _parent = new LazyVariable<IHasConstant?>(GetParent);
            _value = new LazyVariable<DataBlobSignature?>(GetValue);
        }

        /// <summary>
        /// Creates a new constant for a member, with the provided constant type and raw literal value.
        /// </summary>
        /// <param name="type">The type of the constant.</param>
        /// <param name="value">The raw literal value of the constant.</param>
        public Constant(ElementType type, DataBlobSignature? value)
            : this(new MetadataToken(TableIndex.Constant, 0))
        {
            Type = type;
            Value = value;
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
        public IHasConstant? Parent
        {
            get => _parent.Value;
            internal set => _parent.Value = value;
        }

        /// <summary>
        /// Gets or sets the serialized literal value.
        /// </summary>
        public DataBlobSignature? Value
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
        protected virtual IHasConstant? GetParent() => null;


        /// <summary>
        /// Obtains the literal value of the constant.
        /// </summary>
        /// <returns>The value.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Value"/> property.
        /// </remarks>
        protected virtual DataBlobSignature? GetValue() => null;
    }
}
