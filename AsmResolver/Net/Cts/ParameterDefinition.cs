using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ParameterDefinition : MetadataMember<MetadataRow<ParameterAttributes, ushort, uint>>, IHasConstant, IHasCustomAttribute, IHasFieldMarshal
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MethodDefinition> _method;

        public ParameterDefinition(int sequence, string name, ParameterAttributes attributes)
            : base(null, new MetadataToken(MetadataTokenType.Param))
        {
            _name = new LazyValue<string>(name);
            Sequence = sequence;
            Attributes = attributes;

            _method = new LazyValue<MethodDefinition>(default(MethodDefinition));
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

        // todo:
        //public Constant Constant
        //{
        //    get;
        //    set;
        //}

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