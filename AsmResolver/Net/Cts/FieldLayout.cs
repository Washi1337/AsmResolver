using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class FieldLayout : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<FieldDefinition> _field;
        private MetadataImage _image;

        public FieldLayout(uint offset)
            : base(new MetadataToken(MetadataTokenType.FieldLayout))
        {
            Offset = offset;
            _field = new LazyValue<FieldDefinition>();
        }
        
        internal FieldLayout(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
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

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _field.IsInitialized && _field.Value != null ? _field.Value.Image : _image; }
        }

        public uint Offset
        {
            get;
            set;
        }

        public FieldDefinition Field
        {
            get { return _field.Value; }
            internal set
            {
                _field.Value = value;
                _image = null;
            }
        }
    }
}
