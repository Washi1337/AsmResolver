using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
   public class ImplementationMap : MetadataMember<MetadataRow<ImplementationMapAttributes,uint,uint,uint>>
    {
        private readonly LazyValue<IMemberForwarded> _memberForwarded;
        private readonly LazyValue<string> _importName;
        private readonly LazyValue<ModuleReference> _importScope;

        public ImplementationMap(string moduleName, string importName, ImplementationMapAttributes attributes)
            : this(new ModuleReference(moduleName), importName, attributes)
        {
        }

        public ImplementationMap(ModuleReference importScope, string importName, ImplementationMapAttributes attributes)
            : base(null, new MetadataToken(MetadataTokenType.ImplMap))
        {
            _memberForwarded = new LazyValue<IMemberForwarded>();
            _importScope = new LazyValue<ModuleReference>(importScope);
            _importName = new LazyValue<string>(importName);
            Attributes = attributes;
        }

        internal ImplementationMap(MetadataImage image, MetadataRow<ImplementationMapAttributes, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            Attributes = row.Column1;

            _memberForwarded = new LazyValue<IMemberForwarded>(() =>
            {
                var memberForwardedToken = tableStream.GetIndexEncoder(CodedIndex.MemberForwarded).DecodeIndex(row.Column2);
                IMetadataMember member;
                return image.TryResolveMember(memberForwardedToken, out member)
                    ? (IMemberForwarded) member
                    : null;
            });

            _importName = new LazyValue<string>(() =>
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column3));

            _importScope = new LazyValue<ModuleReference>(() =>
            {
                var table = tableStream.GetTable(MetadataTokenType.ModuleRef);
                MetadataRow moduleRow;
                return table.TryGetRow((int) (row.Column4 - 1), out moduleRow)
                    ? (ModuleReference) table.GetMemberFromRow(image, moduleRow)
                    : null;
            });
        }

        public ImplementationMapAttributes Attributes
        {
            get;
            set;
        }

        public IMemberForwarded MemberForwarded
        {
            get { return _memberForwarded.Value; }
            set { _memberForwarded.Value = value; }
        }

        public string ImportName
        {
            get { return _importName.Value; }
            set { _importName.Value = value; }
        }

        public ModuleReference ImportScope
        {
            get { return _importScope.Value; }
            set { _importScope.Value = value; }
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            
            tableStream.GetTable<ImplementationMapTable>().Add(new MetadataRow<ImplementationMapAttributes, uint, uint, uint>
            {
                Column1 = Attributes,
                Column2 = tableStream.GetIndexEncoder(CodedIndex.MemberForwarded).EncodeToken(MemberForwarded.MetadataToken),
                Column3 = buffer.StringStreamBuffer.GetStringOffset(ImportName),
                Column4 = ImportScope.MetadataToken.Rid
            });
        }
    }
}