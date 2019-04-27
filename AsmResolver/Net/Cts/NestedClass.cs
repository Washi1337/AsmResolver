using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata that binds two types together in such a way that one type is nested in the other.
    /// </summary>
    public class NestedClass : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<TypeDefinition> _enclosingClass;
        private MetadataImage _image;

        public NestedClass(TypeDefinition @class) 
            : base( new MetadataToken(MetadataTokenType.NestedClass))
        {
            _class = new LazyValue<TypeDefinition>(@class);
            _enclosingClass = new LazyValue<TypeDefinition>();
        }
        
        internal NestedClass(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var typeTable = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);

            _class = new LazyValue<TypeDefinition>(() =>
                typeTable.TryGetRow((int) (row.Column1 - 1), out var type)
                    ? (TypeDefinition) typeTable.GetMemberFromRow(image, type)
                    : null);

            _enclosingClass = new LazyValue<TypeDefinition>(() =>
                typeTable.TryGetRow((int) (row.Column2 - 1), out var type)
                    ? (TypeDefinition) typeTable.GetMemberFromRow(image, type)
                    : null);

        }

        /// <inheritdoc />
        public override MetadataImage Image => _enclosingClass.IsInitialized && _enclosingClass.Value != null
            ? _enclosingClass.Value.Image
            : _image;

        /// <summary>
        /// Gets or sets the type that is nested in the enclosing type.
        /// </summary>
        public TypeDefinition Class
        {
            get => _class.Value;
            set
            {
                if (_class.IsInitialized && _class.Value != null)
                    _class.Value.DeclaringType = null;
                _class.Value = value;
                if (value != null)
                    Class.DeclaringType = EnclosingClass;
            }
        }

        /// <summary>
        /// Gets the type that declares the nested type.
        /// </summary>
        public TypeDefinition EnclosingClass
        {
            get => _enclosingClass.Value;
            internal set
            {
                _enclosingClass.Value = value;
                _image = null;
                if (_class.IsInitialized && _class.Value != null)
                    _class.Value.DeclaringType = value;
            }
        }

        public override string ToString()
        {
            return (EnclosingClass?.FullName ?? "?") + "+" + (Class?.Name ?? "?");
        }
    }
}