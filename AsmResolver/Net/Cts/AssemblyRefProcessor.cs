using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyRefProcessor : MetadataMember<MetadataRow<uint,uint>>
    {
        private readonly LazyValue<AssemblyReference> _reference;
        private MetadataImage _image;

        public AssemblyRefProcessor(AssemblyReference reference, uint processor)
            : base(new MetadataToken(MetadataTokenType.AssemblyRefProcessor))
        {
            Processor = processor;
            _reference.Value = reference;
        }

        internal AssemblyRefProcessor(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Processor = row.Column1;
            _reference = new LazyValue<AssemblyReference>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.AssemblyRef);
                MetadataRow referenceRow;
                return table.TryGetRow((int) (row.Column2 - 1), out referenceRow)
                    ? (AssemblyReference) table.GetMemberFromRow(image, referenceRow)
                    : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _reference.IsInitialized && _reference.Value != null ? _reference.Value.Image : _image; }
        }

        public uint Processor
        {
            get;
            set;
        }

        public AssemblyReference Reference
        {
            get { return _reference.Value; }
            internal set
            {
                _reference.Value = value;
                _image = null;
            }
        }
    }
}
