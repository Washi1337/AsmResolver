using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ParameterDefinition : MetadataMember<MetadataRow<ParameterAttributes, ushort, uint>>, IHasConstant, IHasCustomAttribute, IHasFieldMarshal
    {
        private readonly LazyValue<string> _name;
        private CustomAttributeCollection _customAttributes;

        public ParameterDefinition(int sequence, string name, ParameterAttributes attributes)
            : base(null, new MetadataToken(MetadataTokenType.Param))
        {
            _name = new LazyValue<string>(name);
            Sequence = sequence;
            Attributes = attributes;
        }

        internal ParameterDefinition(MetadataImage image, MetadataRow<ParameterAttributes, ushort, uint> row)
            : base(image, row.MetadataToken)
        {
            var stringStream = image.Header.GetStream<StringStream>();

            Attributes = row.Column1;
            Sequence = row.Column2;
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
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
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                _customAttributes = new CustomAttributeCollection(this);
                return _customAttributes;
            }
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