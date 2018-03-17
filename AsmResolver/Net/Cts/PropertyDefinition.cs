using System.Linq;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class PropertyDefinition : MetadataMember<MetadataRow<ushort, uint, uint>>, ICallableMemberReference, IHasConstant, IHasSemantics
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<PropertySignature> _signature;
        private readonly LazyValue<PropertyMap> _propertyMap;
        private readonly LazyValue<Constant> _constant;
        private string _fullName;
        private MetadataImage _image;

        public PropertyDefinition(string name, PropertySignature signature) 
            : base(new MetadataToken(MetadataTokenType.Property))
        {
            _name = new LazyValue<string>(name);
            _signature = new LazyValue<PropertySignature>(signature);
            
            _propertyMap = new LazyValue<PropertyMap>();
            _constant = new LazyValue<Constant>();
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
        }
        
        internal PropertyDefinition(MetadataImage image, MetadataRow<PropertyAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() => 
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _signature = new LazyValue<PropertySignature>(() 
                => PropertySignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
            
            _propertyMap = new LazyValue<PropertyMap>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.PropertyMap);
                var mapRow = table.GetRowClosestToKey(1, row.MetadataToken.Rid);
                return mapRow != null ? (PropertyMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            _constant = new LazyValue<Constant>(() =>
            {
                var table = (ConstantTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Constant);
                var constantRow = table.FindConstantOfOwner(row.MetadataToken);
                return constantRow != null ? (Constant) table.GetMemberFromRow(image, constantRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _propertyMap.IsInitialized && _propertyMap.Value != null ? _propertyMap.Value.Image : _image; }
        }

        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName(Signature);
            }
        }

        public PropertySignature Signature
        {
            get { return _signature.Value; }
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature
        {
            get { return Signature; }
        }

        public PropertyMap PropertyMap
        {
            get { return _propertyMap.Value; }
            internal set
            {
                _propertyMap.Value = value;
                _image = null;
            }
        }

        public TypeDefinition DeclaringType
        {
            get { return PropertyMap.Parent; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public MethodSemanticsCollection Semantics
        {
            get;
            private set;
        }

        public MethodDefinition GetMethod
        {
            get
            {
                var semantics = Semantics.FirstOrDefault(x => (x.Attributes & MethodSemanticsAttributes.Getter) != 0);
                return semantics != null ? semantics.Method : null;
            }
        }

        public MethodDefinition SetMethod
        {
            get
            {
                var semantics = Semantics.FirstOrDefault(x => (x.Attributes & MethodSemanticsAttributes.Setter) != 0);
                return semantics != null ? semantics.Method : null;
            }
        }

        public Constant Constant
        {
            get { return _constant.Value; }
            set { this.SetConstant(_constant, value); }
        }

        public IMetadataMember Resolve()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}