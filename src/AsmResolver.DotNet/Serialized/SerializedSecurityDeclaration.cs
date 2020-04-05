using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    public class SerializedSecurityDeclaration : SecurityDeclaration
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly SecurityDeclarationRow _row;

        /// <summary>
        /// Creates a security declaration from a declaration metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the security declaration.</param>
        /// <param name="token">The token to initialize the declaration for.</param>
        /// <param name="row">The metadata table row to base the security declaration on.</param>
        public SerializedSecurityDeclaration(SerializedModuleDefinition parentModule, MetadataToken token,
            SecurityDeclarationRow row)
            : base(token)
        {
            _parentModule = parentModule;
            _row = row;

            Action = row.Action;
        }

        /// <inheritdoc />
        protected override IHasSecurityDeclaration GetParent()
        {
            var ownerToken = _parentModule.GetSecurityDeclarationOwner(MetadataToken.Rid);
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as IHasSecurityDeclaration
                : null;
        }

        /// <inheritdoc />
        protected override PermissionSetSignature GetPermissionSet()
        {
            var reader = _parentModule.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.PermissionSet);
            
            return reader is {} ? 
                PermissionSetSignature.FromReader(_parentModule, reader) 
                : null;
        }
    }
}