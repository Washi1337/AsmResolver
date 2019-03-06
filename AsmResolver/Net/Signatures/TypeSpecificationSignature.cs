using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides a base for all simple type specification signatures that are based on a single, embedded type signature.
    /// </summary>
    public abstract class TypeSpecificationSignature : TypeSignature
    {
        protected TypeSpecificationSignature()
        {
        }

        protected TypeSpecificationSignature(TypeSignature baseType)
        {
            BaseType = baseType;
        }

        /// <summary>
        /// Gets or sets the type this signature was based on.
        /// </summary>
        public TypeSignature BaseType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => BaseType.Name;

        /// <inheritdoc />
        public override string Namespace => BaseType.Namespace;

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => BaseType.ResolutionScope;

        /// <inheritdoc />
        public override ITypeDescriptor GetElementType()
        {
            return BaseType.GetElementType();
        }
        
        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   BaseType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            BaseType.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            BaseType.Write(buffer, writer);
            base.Write(buffer, writer);
        }
    }
}
