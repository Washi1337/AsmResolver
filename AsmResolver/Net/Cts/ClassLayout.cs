using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ClassLayout : MetadataMember<MetadataRow<ushort, uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;

        public ClassLayout(TypeDefinition parent, uint classSize, ushort packingSize)
            : base(null, new MetadataToken(MetadataTokenType.ClassLayout))
        {
            _parent = new LazyValue<TypeDefinition>(parent);
            ClassSize = classSize;
            PackingSize = packingSize;
        }

        public ClassLayout(MetadataImage image, MetadataRow<ushort, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            PackingSize = row.Column1;
            ClassSize = row.Column2;

            _parent = new LazyValue<TypeDefinition>(() =>
            {
                var typeTable = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                MetadataRow typeRow;
                return typeTable.TryGetRow((int) (row.Column3 - 1), out typeRow)
                    ? (TypeDefinition) typeTable.GetMemberFromRow(image, typeRow)
                    : null;
            });
        }

        public ushort PackingSize
        {
            get;
            set;
        }

        public uint ClassSize
        {
            get;
            set;
        }

        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }
    }
}