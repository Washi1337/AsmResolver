using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a set of security attributes assigned to a metadata member. 
    /// </summary>
    public class SecurityDeclaration : 
        IMetadataMember,
        IOwnedCollectionElement<IHasSecurityDeclaration>
    {
        private readonly LazyVariable<IHasSecurityDeclaration> _parent;
        private readonly LazyVariable<PermissionSetSignature> _permissionSet;
        private MetadataToken _token;

        /// <summary>
        /// Initializes the <see cref="SecurityDeclaration"/> with a metadata token.
        /// </summary>
        /// <param name="token">The token.</param>
        protected SecurityDeclaration(MetadataToken token)
        {
            _token = token;
            _parent = new LazyVariable<IHasSecurityDeclaration>(GetParent);
            _permissionSet = new LazyVariable<PermissionSetSignature>(GetPermissionSet);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SecurityDeclaration"/> class.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="permissionSet"></param>
        public SecurityDeclaration(SecurityAction action, PermissionSetSignature permissionSet)
            : this(new MetadataToken(TableIndex.DeclSecurity, 0))
        {
            Action = action;
            PermissionSet = permissionSet;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken => _token;

        MetadataToken IMetadataMember.MetadataToken
        {
            get => _token;
            set => _token = value;
        }

        /// <summary>
        /// Gets the action that is applied. 
        /// </summary>
        public SecurityAction Action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member that is assigned the permission set.
        /// </summary>
        public IHasSecurityDeclaration Parent
        {
            get => _parent.Value;
            private set => _parent.Value = value;
        }

        /// <inheritdoc />
        IHasSecurityDeclaration IOwnedCollectionElement<IHasSecurityDeclaration>.Owner
        {
            get => Parent;
            set => Parent = value;
        }

        /// <summary>
        /// Gets or sets the collection of security attributes.
        /// </summary>
        public PermissionSetSignature PermissionSet
        {
            get => _permissionSet.Value;
            set => _permissionSet.Value = value;
        }

        /// <summary>
        /// Obtains the member that is assigned the permission set.
        /// </summary>
        /// <returns>The parent.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual IHasSecurityDeclaration GetParent() => null;
        
        /// <summary>
        /// Obtains the assigned permission set.
        /// </summary>
        /// <returns>The permission set.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="PermissionSet"/> property.
        /// </remarks>
        protected virtual PermissionSetSignature GetPermissionSet() => null;
    }
}