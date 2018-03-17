using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class SecurityDeclaration : MetadataMember<MetadataRow<SecurityAction, uint, uint>>, IHasCustomAttribute
    {
        private readonly LazyValue<IHasSecurityAttribute> _parent;
        private readonly LazyValue<PermissionSetSignature> _permissionSet;
        private MetadataImage _image;

        public SecurityDeclaration(SecurityAction action, PermissionSetSignature permissionSet)
            : base(new MetadataToken(MetadataTokenType.DeclSecurity))
        {
            Action = action;
            _parent = new LazyValue<IHasSecurityAttribute>();
            _permissionSet = new LazyValue<PermissionSetSignature>(permissionSet);

            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal SecurityDeclaration(MetadataImage image, MetadataRow<SecurityAction, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();
            Action = row.Column1;

            _parent = new LazyValue<IHasSecurityAttribute>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).DecodeIndex(row.Column2);
                return parentToken.Rid != 0 ? (IHasSecurityAttribute) tableStream.ResolveRow(parentToken) : null;
            });

            _permissionSet = new LazyValue<PermissionSetSignature>(() =>
                PermissionSetSignature.FromReader(image,
                    tableStream.MetadataHeader.GetStream<BlobStream>().CreateBlobReader(row.Column3)));

            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        public IHasSecurityAttribute Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        public SecurityAction Action
        {
            get;
            set;
        }

        public PermissionSetSignature PermissionSet
        {
            get { return _permissionSet.Value; }
            set { _permissionSet.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
    }
}
