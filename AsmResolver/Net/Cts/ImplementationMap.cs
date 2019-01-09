using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a P/Invoke mapping between a managed method definition and a native entry point.
    /// </summary>
    public class ImplementationMap : MetadataMember<MetadataRow<ImplementationMapAttributes,uint,uint,uint>>
    {
        private readonly LazyValue<IMemberForwarded> _memberForwarded;
        private readonly LazyValue<string> _importName;
        private readonly LazyValue<ModuleReference> _importScope;
        private MetadataImage _image;

        public ImplementationMap(string moduleName, string importName, ImplementationMapAttributes attributes)
            : this(new ModuleReference(moduleName), importName, attributes)
        {
        }

        public ImplementationMap(ModuleReference importScope, string importName, ImplementationMapAttributes attributes)
            : base(new MetadataToken(MetadataTokenType.ImplMap))
        {
            _memberForwarded = new LazyValue<IMemberForwarded>();
            _importScope = new LazyValue<ModuleReference>(importScope);
            _importName = new LazyValue<string>(importName);
            Attributes = attributes;
        }

        internal ImplementationMap(MetadataImage image, MetadataRow<ImplementationMapAttributes, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();

            Attributes = row.Column1;

            _memberForwarded = new LazyValue<IMemberForwarded>(() =>
            {
                var memberForwardedToken = tableStream.GetIndexEncoder(CodedIndex.MemberForwarded).DecodeIndex(row.Column2);
                return image.TryResolveMember(memberForwardedToken, out var member)
                    ? (IMemberForwarded) member
                    : null;
            });

            _importName = new LazyValue<string>(() =>
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column3));

            _importScope = new LazyValue<ModuleReference>(() =>
            {
                var table = tableStream.GetTable(MetadataTokenType.ModuleRef);
                return table.TryGetRow((int) (row.Column4 - 1), out var moduleRow)
                    ? (ModuleReference) table.GetMemberFromRow(image, moduleRow)
                    : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image => _memberForwarded.IsInitialized && _memberForwarded.Value != null 
            ? _memberForwarded.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the attributes associated to the mapping.
        /// </summary>
        public ImplementationMapAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member that was used to map the native procedure to.
        /// </summary>
        public IMemberForwarded MemberForwarded
        {
            get => _memberForwarded.Value;
            internal set
            {
                _memberForwarded.Value = value;
                _image = null;
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the native procedure to call.
        /// </summary>
        public string ImportName
        {
            get => _importName.Value;
            set => _importName.Value = value;
        }

        /// <summary>
        /// Gets or sets the reference to the module containing the native procedure.
        /// </summary>
        public ModuleReference ImportScope
        {
            get => _importScope.Value;
            set => _importScope.Value = value;
        }
    }
}