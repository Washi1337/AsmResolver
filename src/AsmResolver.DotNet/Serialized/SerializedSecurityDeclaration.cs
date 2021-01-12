using System;
using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="SecurityDeclaration"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedSecurityDeclaration : SecurityDeclaration
    {
        private readonly ModuleReadContext _context;
        private readonly SecurityDeclarationRow _row;

        /// <summary>
        /// Creates a security declaration from a declaration metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the declaration for.</param>
        /// <param name="row">The metadata table row to base the security declaration on.</param>
        public SerializedSecurityDeclaration(
            ModuleReadContext context,
            MetadataToken token,
            in SecurityDeclarationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Action = row.Action;
        }

        /// <inheritdoc />
        protected override IHasSecurityDeclaration GetParent()
        {
            var module = _context.ParentModule;
            
            var ownerToken = module.GetSecurityDeclarationOwner(MetadataToken.Rid);
            return module.TryLookupMember(ownerToken, out var member)
                ? member as IHasSecurityDeclaration
                : null;
        }

        /// <inheritdoc />
        protected override PermissionSetSignature GetPermissionSet()
        {
            var reader = _context.Image.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.PermissionSet);
            
            return reader is {} ? 
                PermissionSetSignature.FromReader(_context.ParentModule, reader) 
                : null;
        }
    }
}