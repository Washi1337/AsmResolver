using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class FieldLayout : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<FieldDefinition> _field;

        public FieldLayout(uint offset)
            : base(null, new MetadataToken(MetadataTokenType.FieldLayout))
        {
            Offset = offset;
            _field = new LazyValue<FieldDefinition>(default(FieldDefinition));
        }
        
        internal FieldLayout(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
            Offset = row.Column1;

            _field = new LazyValue<FieldDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Field);
                MetadataRow fieldRow;
                return table.TryGetRow((int) (row.Column2 - 1), out fieldRow)
                    ? (FieldDefinition) table.GetMemberFromRow(image, fieldRow)
                    : null;
            });
        }

        public uint Offset
        {
            get;
            set;
        }

        public FieldDefinition Field
        {
            get { return _field.Value; }
            internal set { _field.Value = value; }
        }
    }
}
