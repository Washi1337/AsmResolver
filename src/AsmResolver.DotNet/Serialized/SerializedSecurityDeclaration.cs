using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="SecurityDeclaration"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedSecurityDeclaration : SecurityDeclaration
    {
        private readonly ModuleReaderContext _context;
        private readonly SecurityDeclarationRow _row;

        /// <summary>
        /// Creates a security declaration from a declaration metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the declaration for.</param>
        /// <param name="row">The metadata table row to base the security declaration on.</param>
        public SerializedSecurityDeclaration(
            ModuleReaderContext context,
            MetadataToken token,
            in SecurityDeclarationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Action = row.Action;
        }

        /// <inheritdoc />
        protected override IHasSecurityDeclaration? GetParent()
        {
            var module = _context.ParentModule;

            var ownerToken = module.GetSecurityDeclarationOwner(MetadataToken.Rid);
            return module.TryLookupMember(ownerToken, out var member)
                ? member as IHasSecurityDeclaration
                : _context.BadImageAndReturn<IHasSecurityDeclaration>(
                    $"Invalid parent of security declaration {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override PermissionSetSignature? GetPermissionSet()
        {
            if (_context.BlobStream is not { } blobStream
                || !blobStream.TryGetBlobReaderByIndex(_row.PermissionSet, out var reader))
            {
                return _context.BadImageAndReturn<PermissionSetSignature>(
                    $"Invalid permission set blob index in security declaration {MetadataToken.ToString()}.");
            }

            return PermissionSetSignature.FromReader(new BlobReaderContext(_context), ref reader);
        }
    }
}
