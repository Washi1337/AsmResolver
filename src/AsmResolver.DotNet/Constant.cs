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
        private readonly LazyVariable<Constant, IHasConstant?> _parent;
        private readonly LazyVariable<Constant, DataBlobSignature?> _value;

        /// <summary>
        /// Initializes the constant with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected Constant(MetadataToken token)
            : base(token)
        {
            _parent = new LazyVariable<Constant, IHasConstant?>(x => x.GetParent());
            _value = new LazyVariable<Constant, DataBlobSignature?>(x => x.GetValue());
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
            get => _parent.GetValue(this);
            internal set => _parent.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the serialized literal value.
        /// </summary>
        public DataBlobSignature? Value
        {
            get => _value.GetValue(this);
            set => _value.SetValue(value);
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

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(bool value)
        {
            return new Constant(ElementType.Boolean, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(char value)
        {
            return new Constant(ElementType.Char, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(byte value)
        {
            return new Constant(ElementType.U1, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(sbyte value)
        {
            return new Constant(ElementType.I1, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(ushort value)
        {
            return new Constant(ElementType.U2, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(short value)
        {
            return new Constant(ElementType.I2, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(uint value)
        {
            return new Constant(ElementType.U4, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(int value)
        {
            return new Constant(ElementType.I4, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(ulong value)
        {
            return new Constant(ElementType.U8, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(long value)
        {
            return new Constant(ElementType.I8, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(float value)
        {
            return new Constant(ElementType.R4, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(double value)
        {
            return new Constant(ElementType.R8, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(string value)
        {
            return new Constant(ElementType.String, DataBlobSignature.FromValue(value));
        }
    }
}
