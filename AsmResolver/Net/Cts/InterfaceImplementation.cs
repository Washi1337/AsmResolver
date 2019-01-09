using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata added to a type indicating the type is implementing a particular interface.
    /// </summary>
    public class InterfaceImplementation : MetadataMember<MetadataRow<uint, uint>>, IHasCustomAttribute
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<ITypeDefOrRef> _interface;
        private MetadataImage _image;

        public InterfaceImplementation(ITypeDefOrRef @interface)
            : base(new MetadataToken(MetadataTokenType.InterfaceImpl))
        {
            _class = new LazyValue<TypeDefinition>();
            _interface = new LazyValue<ITypeDefOrRef>(@interface);
            
            CustomAttributes = new CustomAttributeCollection(this);
        }
        
        internal InterfaceImplementation(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();
            _class = new LazyValue<TypeDefinition>(() =>
            {
                var table = tableStream.GetTable(MetadataTokenType.TypeDef);
                return table.TryGetRow((int) (row.Column1 - 1), out var typeRow)
                    ? (TypeDefinition) table.GetMemberFromRow(image, typeRow)
                    : null;
            });

            _interface = new LazyValue<ITypeDefOrRef>(() =>
            {
                var interfaceToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column2);
                return image.TryResolveMember(interfaceToken, out var member) ? (ITypeDefOrRef) member : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _class.IsInitialized && _class.Value != null ? _class.Value.Image : _image; }
        }

        /// <summary>
        /// Gets the type that is implementing the interface.
        /// </summary>
        public TypeDefinition Class
        {
            get => _class.Value;
            internal set
            {
                _class.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the interface that is implemented.
        /// </summary>
        public ITypeDefOrRef Interface
        {
            get => _interface.Value;
            set => _interface.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }
    }
}