using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class PInvokeImplementationTable : MetadataTable<PInvokeImplementation>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ImplMap; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.GetTable<ModuleReference>().IndexSize;
        }

        protected override PInvokeImplementation ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new PInvokeImplementation(Header, token, new MetadataRow<ushort, uint, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.GetTable<ModuleReference>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, PInvokeImplementation member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Attributes;
            row.Column2 = TableStream.GetIndexEncoder(CodedIndex.MemberForwarded)
                .EncodeToken(member.MemberForwarded.MetadataToken);
            row.Column3 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.ImportName);
            row.Column4 = member.ImportScope.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, PInvokeImplementation member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetTable<ModuleReference>().IndexSize, row.Column4);
        }
    }

    public class PInvokeImplementation : MetadataMember<MetadataRow<ushort,uint,uint,uint>>
    {
        private readonly LazyValue<IMemberForwarded> _memberForwarded;
        private readonly LazyValue<string> _importName;
        private readonly LazyValue<ModuleReference> _importScope;

        public PInvokeImplementation(string moduleName, string importName, PInvokeImplementationAttributes attributes)
            : this(new ModuleReference(moduleName), importName, attributes)
        {

        }

        public PInvokeImplementation(ModuleReference importScope, string importName, PInvokeImplementationAttributes attributes)
            : base(null, new MetadataToken(MetadataTokenType.ImplMap), new MetadataRow<ushort, uint, uint, uint>())
        {
            _memberForwarded = new LazyValue<IMemberForwarded>();
            _importScope = new LazyValue<ModuleReference>(importScope);
            _importName = new LazyValue<string>(importName);
            Attributes = attributes;
        }

        internal PInvokeImplementation(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            Attributes = (PInvokeImplementationAttributes)row.Column1;

            _memberForwarded = new LazyValue<IMemberForwarded>(() =>
            {
                var memberForwardedToken = tableStream.GetIndexEncoder(CodedIndex.MemberForwarded).DecodeIndex(row.Column2);
                return memberForwardedToken.Rid != 0
                    ? (IMemberForwarded)tableStream.ResolveMember(memberForwardedToken)
                    : null;
            });

            _importName = new LazyValue<string>(() => header.GetStream<StringStream>().GetStringByOffset(row.Column3));
            _importScope = new LazyValue<ModuleReference>(() => tableStream.GetTable<ModuleReference>()[(int)(row.Column4 - 1)]);
        }

        public PInvokeImplementationAttributes Attributes
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
    }
}
