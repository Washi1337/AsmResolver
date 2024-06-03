using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides an implementation for the <see cref="ITypeSignatureResolver"/> that resolves metadata tokens from
    /// the underlying module's tables stream.
    /// </summary>
    public class PhysicalTypeSignatureResolver : ITypeSignatureResolver
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="PhysicalTypeSignatureResolver"/> class.
        /// </summary>
        public static PhysicalTypeSignatureResolver Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public virtual ITypeDefOrRef ResolveToken(ref BlobReaderContext context, MetadataToken token)
        {
            switch (token.Table)
            {
                // Check for infinite recursion.
                case TableIndex.TypeSpec when !context.StepInToken(token):
                    context.ReaderContext.BadImage("Infinite metadata loop was detected.");
                    return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop);

                // Any other type is legal.
                case TableIndex.TypeSpec:
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                    if (context.ReaderContext.ParentModule.TryLookupMember(token, out var member)
                        && member is ITypeDefOrRef typeDefOrRef)
                    {
                        if (token.Table == TableIndex.TypeSpec)
                            context.StepOutToken();

                        return typeDefOrRef;
                    }

                    context.ReaderContext.BadImage($"Metadata token in type signature refers to a non-existing TypeDefOrRef member {token}.");
                    return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);

                default:
                    context.ReaderContext.BadImage("Invalid coded index.");
                    return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);
            }
        }

        /// <inheritdoc />
        public virtual TypeSignature ResolveRuntimeType(ref BlobReaderContext context, nint address)
        {
            throw new NotSupportedException(
                "Encountered an COR_ELEMENT_TYPE_INTERNAL type signature which is not supported by this "
                + " type signature reader. Use the AsmResolver.DotNet.Dynamic extension package instead.");
        }
    }
}
