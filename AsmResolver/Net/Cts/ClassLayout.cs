using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a description of the physical layout of a particular type definition.
    /// </summary>
    public class ClassLayout : MetadataMember<MetadataRow<ushort, uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;
        private MetadataImage _image;

        public ClassLayout(uint classSize, ushort packingSize)
            : base(new MetadataToken(MetadataTokenType.ClassLayout))
        {
            _parent = new LazyValue<TypeDefinition>();
            ClassSize = classSize;
            PackingSize = packingSize;
        }

        public ClassLayout(MetadataImage image, MetadataRow<ushort, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
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

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        /// <summary>
        /// Gets or sets a value indicating the alignment of the fields defined in the <see cref="Parent"/> type.
        /// If set to 0, then the alignment is selected by the runtime.  
        /// </summary>
        /// <remarks>
        /// This value must be one of 0, 1, 2, 4, 8, 16, 32, 64 or 128 for a valid application to be produced.
        /// </remarks>
        public ushort PackingSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the amount of bytes the <see cref="Parent"/> type uses in memory.
        /// </summary>
        public uint ClassSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent type definition that this class layout applies to.
        /// </summary>
        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }
    }
}