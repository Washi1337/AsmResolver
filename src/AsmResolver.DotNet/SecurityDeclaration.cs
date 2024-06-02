using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a set of security attributes assigned to a metadata member.
    /// </summary>
    public class SecurityDeclaration :
        MetadataMember,
        IOwnedCollectionElement<IHasSecurityDeclaration>
    {
        private readonly LazyVariable<SecurityDeclaration, IHasSecurityDeclaration?> _parent;
        private readonly LazyVariable<SecurityDeclaration, PermissionSetSignature?> _permissionSet;

        /// <summary>
        /// Initializes the <see cref="SecurityDeclaration"/> with a metadata token.
        /// </summary>
        /// <param name="token">The token.</param>
        protected SecurityDeclaration(MetadataToken token)
            : base(token)
        {
            _parent = new LazyVariable<SecurityDeclaration, IHasSecurityDeclaration?>(x => x.GetParent());
            _permissionSet = new LazyVariable<SecurityDeclaration, PermissionSetSignature?>(x => x.GetPermissionSet());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SecurityDeclaration"/> class.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="permissionSet"></param>
        public SecurityDeclaration(SecurityAction action, PermissionSetSignature? permissionSet)
            : this(new MetadataToken(TableIndex.DeclSecurity, 0))
        {
            Action = action;
            PermissionSet = permissionSet;
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
        public IHasSecurityDeclaration? Parent
        {
            get => _parent.GetValue(this);
            private set => _parent.SetValue(value);
        }

        /// <inheritdoc />
        IHasSecurityDeclaration? IOwnedCollectionElement<IHasSecurityDeclaration>.Owner
        {
            get => Parent;
            set => Parent = value;
        }

        /// <summary>
        /// Gets or sets the collection of security attributes.
        /// </summary>
        public PermissionSetSignature? PermissionSet
        {
            get => _permissionSet.GetValue(this);
            set => _permissionSet.SetValue(value);
        }

        /// <summary>
        /// Obtains the member that is assigned the permission set.
        /// </summary>
        /// <returns>The parent.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual IHasSecurityDeclaration? GetParent() => null;

        /// <summary>
        /// Obtains the assigned permission set.
        /// </summary>
        /// <returns>The permission set.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="PermissionSet"/> property.
        /// </remarks>
        protected virtual PermissionSetSignature? GetPermissionSet() => null;
    }
}
