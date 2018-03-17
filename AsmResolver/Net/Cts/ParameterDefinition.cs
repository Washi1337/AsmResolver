using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ParameterDefinition : MetadataMember<MetadataRow<ParameterAttributes, ushort, uint>>, IHasConstant, IHasCustomAttribute, IHasFieldMarshal
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MethodDefinition> _method;
        private readonly LazyValue<Constant> _constant;
        private readonly LazyValue<FieldMarshal> _fieldMarshal;
        private MetadataImage _image;

        public ParameterDefinition(int sequence, string name, ParameterAttributes attributes)
            : base(new MetadataToken(MetadataTokenType.Param))
        {
            _name = new LazyValue<string>(name);
            Sequence = sequence;
            Attributes = attributes;

            _method = new LazyValue<MethodDefinition>();
            _constant = new LazyValue<Constant>();
            _fieldMarshal = new LazyValue<FieldMarshal>();
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ParameterDefinition(MetadataImage image, MetadataRow<ParameterAttributes, ushort, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Attributes = row.Column1;
            Sequence = row.Column2;
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column3));
            
            _method =  new LazyValue<MethodDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Method);
                var methodRow = table.GetRowClosestToKey(5, row.MetadataToken.Rid);
                return (MethodDefinition) table.GetMemberFromRow(image, methodRow);
            });
            
            _constant = new LazyValue<Constant>(() =>
            {
                var table = (ConstantTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Constant);
                var constantRow = table.FindConstantOfOwner(row.MetadataToken);
                return constantRow != null ? (Constant) table.GetMemberFromRow(image, constantRow) : null;
            });
            
            _fieldMarshal = new LazyValue<FieldMarshal>(() =>
            {
                var table = (FieldMarshalTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.FieldMarshal);
                var marshalRow = table.FindFieldMarshalOfOwner(row.MetadataToken);
                return marshalRow != null ? (FieldMarshal) table.GetMemberFromRow(image, marshalRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _method.IsInitialized && _method.Value != null ? _method.Value.Image : _image; }
        }

        public ParameterAttributes Attributes
        {
            get;
            set;
        }

        public int Sequence
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public Constant Constant
        {
            get { return _constant.Value;}
            set { this.SetConstant(_constant, value); }
        }

        public MethodDefinition Method
        {
            get { return _method.Value; }
            internal set
            {
                _method.Value = value;
                _image = null;
            }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public FieldMarshal FieldMarshal
        {
            get { return _fieldMarshal.Value;}
            set { this.SetFieldMarshal(_fieldMarshal, value); }
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}