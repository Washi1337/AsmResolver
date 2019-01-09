using System.Linq;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a property that is declared in a type definition.
    /// </summary>
    /// <remarks>
    /// Properties do not contain any code themselves. Rather, the code is defined in the methods stored in
    /// the <see cref="Semantics"/> properties. 
    /// </remarks>
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
                => PropertySignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3), true));
            
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
        public override MetadataImage Image => _propertyMap.IsInitialized && _propertyMap.Value != null 
            ? _propertyMap.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the attributes associated to the property.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc cref="IMemberReference.Name" />
        public string Name
        {
            get => _name.Value;
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName(Signature);
            }
        }

        /// <summary>
        /// Gets or sets the signature associated to the property.
        /// </summary>
        public PropertySignature Signature
        {
            get => _signature.Value;
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature => Signature;

        /// <summary>
        /// Gets the map the property is contained in.
        /// </summary>
        public PropertyMap PropertyMap
        {
            get => _propertyMap.Value;
            internal set
            {
                _propertyMap.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets the type that declares the property.
        /// </summary>
        public TypeDefinition DeclaringType => PropertyMap.Parent;

        ITypeDefOrRef IMemberReference.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <inheritdoc />
        public MethodSemanticsCollection Semantics
        {
            get;
        }

        /// <summary>
        /// Gets the getter method associated to the property (if available).
        /// </summary>
        public MethodDefinition GetMethod
        {
            get
            {
                var semantics = Semantics.FirstOrDefault(x => (x.Attributes & MethodSemanticsAttributes.Getter) != 0);
                return semantics?.Method;
            }
        }

        /// <summary>
        /// Gets the setter method associated to the property (if available).
        /// </summary>
        public MethodDefinition SetMethod
        {
            get
            {
                var semantics = Semantics.FirstOrDefault(x => (x.Attributes & MethodSemanticsAttributes.Setter) != 0);
                return semantics?.Method;
            }
        }

        /// <summary>
        /// Gets the default value associated to the property (if available).
        /// </summary>
        public Constant Constant
        {
            get => _constant.Value;
            set => this.SetConstant(_constant, value);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}