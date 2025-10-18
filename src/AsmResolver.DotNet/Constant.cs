using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a literal value that is assigned to a field, parameter or property.
    /// </summary>
    public partial class Constant : MetadataMember
    {
        private readonly object _lock = new();

        /// <summary>
        /// Initializes the constant with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected Constant(MetadataToken token)
            : base(token)
        {
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
        [LazyProperty]
        public partial IHasConstant? Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the serialized literal value.
        /// </summary>
        [LazyProperty]
        public partial DataBlobSignature? Value
        {
            get;
            set;
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
        /// Interprets the raw data stored in the <see cref="Value"/> property as a literal.
        /// </summary>
        /// <returns>The deserialized literal.</returns>
        public object? InterpretData()
        {
            return Value?.InterpretData(Type);
        }

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
        public static Constant FromValue(nuint value)
        {
            return FromValue((uint)value);
        }

        /// <summary>
        /// Create a <see cref="Constant"/> from a value
        /// </summary>
        /// <param name="value">The value to be assigned to the constant</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromValue(nint value)
        {
            return FromValue((int)value);
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
        public static Constant FromValue(string? value)
        {
            return new Constant(value is null ? ElementType.Class : ElementType.String, DataBlobSignature.FromValue(value));
        }

        /// <summary>
        /// Create a <see cref="Constant"/> representing a null reference
        /// </summary>
        /// <remarks>
        /// This can be used for any non-primitive default value.
        /// </remarks>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromNull()
        {
            return new Constant(ElementType.Class, DataBlobSignature.FromNull());
        }

        /// <summary>
        /// Create a <see cref="Constant"/> representing a default value
        /// </summary>
        /// <param name="elementType">The type of the new constant.</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromDefault(ElementType elementType) => elementType switch
        {
            ElementType.Boolean => FromValue(false),
            ElementType.Char => FromValue('\0'),
            ElementType.I1 => FromValue((sbyte)0),
            ElementType.U1 => FromValue((byte)0),
            ElementType.I2 => FromValue((short)0),
            ElementType.U2 => FromValue((ushort)0),
            ElementType.I4 => FromValue(0),
            ElementType.U4 => FromValue(0u),
            ElementType.I => FromValue((nint)0),
            ElementType.U => FromValue((nuint)0),
            ElementType.I8 => FromValue(0L),
            ElementType.U8 => FromValue(0UL),
            ElementType.R4 => FromValue(0f),
            ElementType.R8 => FromValue(0d),
            _ => FromNull(),
        };

        /// <summary>
        /// Create a <see cref="Constant"/> representing a default value
        /// </summary>
        /// <param name="type">The type of the new constant.</param>
        /// <returns>
        /// A new <see cref="Constant"/> with the correct <see cref="Type"/> and <see cref="Value"/>
        /// </returns>
        public static Constant FromDefault(TypeSignature type)
        {
            return FromDefault(type.ElementType);
        }
    }
}
