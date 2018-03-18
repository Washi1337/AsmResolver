using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class MethodSemantics : MetadataMember<MetadataRow<MethodSemanticsAttributes, uint, uint>>
    {
        private readonly LazyValue<MethodDefinition> _method;
        private readonly LazyValue<IHasSemantics> _association;
        private MetadataImage _image;

        public MethodSemantics(MethodDefinition method, MethodSemanticsAttributes attributes) 
            : base(new MetadataToken(MetadataTokenType.MethodSemantics))
        {
            _method = new LazyValue<MethodDefinition>(method);
            _association = new LazyValue<IHasSemantics>();
            Attributes = attributes;
        }
        
        internal MethodSemantics(MetadataImage image, MetadataRow<MethodSemanticsAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();

            Attributes = row.Column1;
            
            _method = new LazyValue<MethodDefinition>(() =>
            {
                var methodTable = tableStream.GetTable(MetadataTokenType.Method);
                var methodRow = methodTable.GetRow((int) (row.Column2 - 1));
                return (MethodDefinition) methodTable.GetMemberFromRow(image, methodRow);
            });

            _association = new LazyValue<IHasSemantics>(() =>
            {
                var associationToken = tableStream.GetIndexEncoder(CodedIndex.HasSemantics).DecodeIndex(row.Column3);
                IMetadataMember member;
                return image.TryResolveMember(associationToken, out member) ? (IHasSemantics) member : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _association.IsInitialized && _association.Value != null ? _association.Value.Image : _image; }
        }

        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        public MethodDefinition Method
        {
            get { return _method.Value; }
            set { _method.Value = value; }
        }

        public IHasSemantics Association
        {
            get { return _association.Value; }
            internal set
            {
                _association.Value = value;
                _image = null;
            }
        }
    }    
}