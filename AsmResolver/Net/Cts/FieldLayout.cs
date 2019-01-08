using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata associated to a field definition regarding the raw data offset of the field.
    /// </summary>
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
                return table.TryGetRow((int) (row.Column2 - 1), out var fieldRow)
                    ? (FieldDefinition) table.GetMemberFromRow(image, fieldRow)
                    : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image => _field.IsInitialized && _field.Value != null 
            ? _field.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the offset the field will be found at inside the parent type.
        /// </summary>
        public uint Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the field that is associated to this field layout metadata.
        /// </summary>
        public FieldDefinition Field
        {
            get => _field.Value;
            internal set
            {
                _field.Value = value;
                _image = null;
            }
        }
    }
}
