using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ParameterDefinition : MetadataMember<MetadataRow<ParameterAttributes, ushort, uint>>, IHasConstant, IHasCustomAttribute, IHasFieldMarshal
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MethodDefinition> _method;
        private readonly LazyValue<Constant> _constant;

        public ParameterDefinition(int sequence, string name, ParameterAttributes attributes)
            : base(null, new MetadataToken(MetadataTokenType.Param))
        {
            _name = new LazyValue<string>(name);
            Sequence = sequence;
            Attributes = attributes;

            _method = new LazyValue<MethodDefinition>(default(MethodDefinition));
            _constant = new LazyValue<Constant>(default(Constant));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ParameterDefinition(MetadataImage image, MetadataRow<ParameterAttributes, ushort, uint> row)
            : base(image, row.MetadataToken)
        {
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
            
            CustomAttributes = new CustomAttributeCollection(this);
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
            set { _constant.Value = value; }
        }

        public MethodDefinition Method
        {
            get;
            internal set;
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        // todo:
        //public FieldMarshal FieldMarshal
        //{
        //    get;
        //    set;
        //}
        
        public override string ToString()
        {
            return Name;
        }
    }
}