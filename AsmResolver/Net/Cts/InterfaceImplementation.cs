using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
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
                MetadataRow typeRow;
                return table.TryGetRow((int) (row.Column1 - 1), out typeRow)
                    ? (TypeDefinition) table.GetMemberFromRow(image, typeRow)
                    : null;
            });

            _interface = new LazyValue<ITypeDefOrRef>(() =>
            {
                var interfaceToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column2);
                IMetadataMember member;
                return image.TryResolveMember(interfaceToken, out member) ? (ITypeDefOrRef) member : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _class.IsInitialized && _class.Value != null ? _class.Value.Image : _image; }
        }

        public TypeDefinition Class
        {
            get { return _class.Value;}
            internal set
            {
                _class.Value = value;
                _image = null;
            }
        }

        public ITypeDefOrRef Interface
        {
            get { return _interface.Value;}
            set { _interface.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
    }
}