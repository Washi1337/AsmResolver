using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Provides members for resolving raw metadata tokens and addresses to types.
    /// </summary>
    public interface ITypeSignatureResolver
    {
        /// <summary>
        /// Resolves a metadata token to a type.
        /// </summary>
        /// <param name="context">The blob reading context the type is situated in.</param>
        /// <param name="token">The token to resolve.</param>
        /// <returns>The type.</returns>
        ITypeDefOrRef ResolveToken(ref BlobReadContext context, MetadataToken token);

        /// <summary>
        /// Resolves an address to a runtime method table to a type signature.
        /// </summary>
        /// <param name="context">The blob reading context the type is situated in.</param>
        /// <param name="address">The address to resolve.</param>
        /// <returns>The type.</returns>
        TypeSignature ResolveRuntimeType(ref BlobReadContext context, nint address);
    }

}
