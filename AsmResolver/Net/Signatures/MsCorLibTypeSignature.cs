using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single type signature found in the standard library and implementation of the CLR. 
    /// </summary>
    /// <remarks>
    /// Use this type of signature if you want to reference common types like <c>Int32</c> or <c>String</c>, as they
    /// result in fewer bytes emitted to the assembly.
    /// </remarks>
    public sealed class MsCorLibTypeSignature : TypeSignature, IResolvable
    {
        /// <summary>
        /// Translates a raw element type to a corlib type signature.
        /// </summary>
        /// <param name="image">The image the type was defined in.</param>
        /// <param name="elementType">The type to convert.</param>
        /// <returns>The converted type.</returns>
        /// <exception cref="ArgumentException">Occurs when the provided element type is not a valid primitive type.</exception>
        public static MsCorLibTypeSignature FromElementType(MetadataImage image, ElementType elementType)
        {
            var type = image.TypeSystem.GetMscorlibType(elementType);
            if (type == null)
                throw new ArgumentException($"Element type {elementType} is not recognized as a valid corlib type signature.");
            return type;
        }

        internal MsCorLibTypeSignature(ITypeDefOrRef type, ElementType elementType, bool isValueType)
        {
            Type = type;
            ElementType = elementType;
            IsValueType = isValueType;
        }

        /// <summary>
        /// Gets the type referenced by the signature. 
        /// </summary>
        public ITypeDefOrRef Type
        {
            get;
        }

        /// <inheritdoc />
        public override ElementType ElementType
        {
            get;
        }

        /// <inheritdoc />
        public override string Name => Type.Name;

        /// <inheritdoc />
        public override string Namespace => Type.Namespace;

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => Type.ResolutionScope;

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef()
        {
            return Type;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof (byte)
                + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);

            base.Write(buffer, writer);
        }

        /// <inheritdoc />
        public IMetadataMember Resolve()
        {
            return Type.Resolve();
        }
    }
}
