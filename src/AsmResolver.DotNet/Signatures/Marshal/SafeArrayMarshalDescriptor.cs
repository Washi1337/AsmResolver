using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents a description for marshalling a safe array, which is a self-describing array that carries the type,
    /// rank, and bounds of the associated array data.
    /// </summary>
    public class SafeArrayMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single safe array marshal descriptor from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module defining the descriptor.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The descriptor.</returns>
        public new static SafeArrayMarshalDescriptor FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            if (!reader.TryReadCompressedUInt32(out uint type))
                return new SafeArrayMarshalDescriptor(SafeArrayVariantType.NotSet);

            var variantType = (SafeArrayVariantType) type & SafeArrayVariantType.TypeMask;
            var flags = (SafeArrayTypeFlags) type & ~ SafeArrayTypeFlags.Mask;

            var result = new SafeArrayMarshalDescriptor(variantType, flags);
            
            if (reader.CanRead(1))
            {
                string typeName = reader.ReadSerString();
                if (typeName != null)
                    result.UserDefinedSubType = TypeNameParser.Parse(parentModule, typeName);
            }

            return result;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="variantType">The element type of the safe array.</param>
        public SafeArrayMarshalDescriptor(SafeArrayVariantType variantType)
        {
            VariantType = variantType;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="variantType">The element type of the safe array.</param>
        /// <param name="flags">The flags associated to the element type of the safe array.</param>
        public SafeArrayMarshalDescriptor(SafeArrayVariantType variantType, SafeArrayTypeFlags flags)
        {
            VariantType = variantType;
            VariantTypeFlags = flags;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="variantType">The element type of the safe array.</param>
        /// <param name="flags">The flags associated to the element type of the safe array.</param>
        /// <param name="subType">The user defined array element type.</param>
        public SafeArrayMarshalDescriptor(SafeArrayVariantType variantType, SafeArrayTypeFlags flags, TypeSignature subType)
        {
            VariantType = variantType;
            VariantTypeFlags = flags;
            UserDefinedSubType = subType;
        }
        
        /// <inheritdoc />
        public override NativeType NativeType => NativeType.SafeArray;

        /// <summary>
        /// Gets or sets the element type of the safe array.
        /// </summary>
        public SafeArrayVariantType VariantType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flags associated to the element type of the safe array.
        /// </summary>
        public SafeArrayTypeFlags VariantTypeFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a vector.
        /// </summary>
        public bool IsVector
        {
            get => (VariantTypeFlags & SafeArrayTypeFlags.Vector) != 0;
            set => VariantTypeFlags = VariantTypeFlags & ~SafeArrayTypeFlags.Vector
                                    | (value ? SafeArrayTypeFlags.Vector : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an array.
        /// </summary>
        public bool IsArray
        {
            get => (VariantTypeFlags & SafeArrayTypeFlags.Array) != 0;
            set => VariantTypeFlags = VariantTypeFlags & ~SafeArrayTypeFlags.Array
                                    | (value ? SafeArrayTypeFlags.Array : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a by-reference type.
        /// </summary>
        public bool IsByRef
        {
            get => (VariantTypeFlags & SafeArrayTypeFlags.ByRef) != 0;
            set => VariantTypeFlags = VariantTypeFlags & ~SafeArrayTypeFlags.ByRef
                                    | (value ? SafeArrayTypeFlags.ByRef : 0);
        }

        /// <summary>
        /// Gets or sets the user defined element type of the safe array. 
        /// </summary>
        /// <remarks>
        /// This value is usually <c>null</c>. Valid .NET assemblies require <see cref="VariantType"/> to be set to
        /// <see cref="SafeArrayVariantType.Unknown"/>, <see cref="SafeArrayVariantType.Dispatch"/>, or
        /// <see cref="SafeArrayVariantType.Record"/>.
        /// </remarks>
        public TypeSignature UserDefinedSubType
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) NativeType);
            if (VariantType != SafeArrayVariantType.NotSet)
            {
                writer.WriteCompressedUInt32(((uint) VariantType & 0xFFF) | (uint) VariantTypeFlags);

                if (UserDefinedSubType != null)
                    writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(UserDefinedSubType));
            }
        }
    }
}