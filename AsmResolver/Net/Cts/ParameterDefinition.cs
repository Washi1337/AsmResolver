using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata associated to a parameter, including the name of the parameter as well as custom
    /// attributes or default values. 
    /// </summary>
    /// <remarks>
    /// A parameter definition is not always required for a parameter to function. The CLR accepts unnamed parameters.
    /// </remarks>
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
        public override MetadataImage Image => _method.IsInitialized && _method.Value != null 
            ? _method.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the attributes associated to the parameter.
        /// </summary>
        public ParameterAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the parameter.
        /// </summary>
        /// <remarks>
        /// When this index is 0, it refers to the return value of the method.
        /// When it contains a non-zero index n, it refers to the n-th parameter (which is index n-1).  
        /// </remarks>
        public int Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        public Constant Constant
        {
            get => _constant.Value;
            set => this.SetConstant(_constant, value);
        }

        /// <summary>
        /// Gets the method that defines the parameter.
        /// </summary>
        public MethodDefinition Method
        {
            get => _method.Value;
            internal set
            {
                _method.Value = value;
                _image = null;
            }
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Gets or sets the custom marshaller used to marshal the parameter value (if available).
        /// </summary>
        public FieldMarshal FieldMarshal
        {
            get => _fieldMarshal.Value;
            set => this.SetFieldMarshal(_fieldMarshal, value);
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}