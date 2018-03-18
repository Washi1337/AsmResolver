using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a field definition that is declared in a type.
    /// </summary>
    public class FieldDefinition : MetadataMember<MetadataRow<FieldAttributes, uint, uint>>, ICallableMemberReference, IHasConstant, IHasFieldMarshal, IMemberForwarded
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<FieldSignature> _signature;
        private readonly LazyValue<Constant> _constant;
        private readonly LazyValue<TypeDefinition> _declaringType;
        private readonly LazyValue<FieldRva> _fieldRva;
        private readonly LazyValue<FieldMarshal> _fieldMarshal;
        private readonly LazyValue<FieldLayout> _fieldLayout;
        private readonly LazyValue<ImplementationMap> _pinvokeMap;
        private string _fullName;
        private MetadataImage _image;

        public FieldDefinition(string name, FieldAttributes attributes, FieldSignature signature)
            : base(new MetadataToken(MetadataTokenType.Field))
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (signature == null)
                throw new ArgumentNullException("signature");

            _name = new LazyValue<string>(name);
            Attributes = attributes;
            _signature = new LazyValue<FieldSignature>(signature);
            
            _constant = new LazyValue<Constant>();
            _declaringType = new LazyValue<TypeDefinition>();
            _fieldRva = new LazyValue<FieldRva>();
            _fieldMarshal = new LazyValue<FieldMarshal>();
            _fieldLayout = new LazyValue<FieldLayout>();
            _pinvokeMap = new LazyValue<ImplementationMap>();
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal FieldDefinition(MetadataImage image, MetadataRow<FieldAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() => 
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _signature = new LazyValue<FieldSignature>(() => 
                FieldSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));

            _declaringType = new LazyValue<TypeDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                var typeRow = table.GetRowClosestToKey(4, row.MetadataToken.Rid);
                return (TypeDefinition) table.GetMemberFromRow(image, typeRow);
            });
            
            _constant = new LazyValue<Constant>(() =>
            {
                var table = (ConstantTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Constant);
                var constantRow = table.FindConstantOfOwner(row.MetadataToken);
                return constantRow != null ? (Constant) table.GetMemberFromRow(image, constantRow) : null;
            });
            
            _fieldRva = new LazyValue<FieldRva>(() =>
            {
                var tableStream = image.Header.GetStream<TableStream>();
                var table = (FieldRvaTable) tableStream.GetTable(MetadataTokenType.FieldRva);
                var rvaRow = table.FindFieldRvaOfField(row.MetadataToken.Rid);
                return rvaRow != null ? (FieldRva) table.GetMemberFromRow(image, rvaRow) : null;
            });
            
            _fieldMarshal = new LazyValue<FieldMarshal>(() =>
            {
                var table = (FieldMarshalTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.FieldMarshal);
                var marshalRow = table.FindFieldMarshalOfOwner(row.MetadataToken);
                return marshalRow != null ? (FieldMarshal) table.GetMemberFromRow(image, marshalRow) : null;
            });
            
            _fieldLayout = new LazyValue<FieldLayout>(() =>
            {
                var table = (FieldLayoutTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.FieldLayout);
                var layoutRow = table.FindFieldLayoutOfOwner(row.MetadataToken);
                return layoutRow != null ? (FieldLayout) table.GetMemberFromRow(image, layoutRow) : null;
            });
            
            _pinvokeMap = new LazyValue<ImplementationMap>(() =>
            {
                if (!row.Column1.HasFlag(FieldAttributes.PinvokeImpl)) 
                    return null;
                
                var table = (ImplementationMapTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.ImplMap);
                var mapRow = table.FindImplementationMapOfOwner(row.MetadataToken);
                return mapRow != null ? (ImplementationMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _declaringType.IsInitialized && _declaringType.Value != null ? _declaringType.Value.Image : _image; }
        }

        /// <summary>
        /// Gets or sets the attributes of the field.
        /// </summary>
        public FieldAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName
        {
            get { return _fullName ?? (_fullName = this.GetFullName(Signature)); }
        }

        /// <summary>
        /// Gets the type that declares the field.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get { return _declaringType.Value;}
            internal set { _declaringType.Value = value; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        /// <summary>
        /// Gets or sets the signature of the field, containing e.g. the field type.
        /// </summary>
        public FieldSignature Signature
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

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given constant member is already added to another field.
        /// </exception>
        public Constant Constant
        {
            get { return _constant.Value; }
            set { this.SetConstant(_constant, value); }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given field marshal is already added to another field.
        /// </exception>
        public FieldMarshal FieldMarshal
        {
            get { return _fieldMarshal.Value; }
            set { this.SetFieldMarshal(_fieldMarshal, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a field RVA member associated, which indicates
        /// the field has an initial value that is loaded upon runtime.
        /// </summary>
        public bool HasFieldRva
        {
            get { return Attributes.HasFlag(FieldAttributes.HasFieldRva); }
            set { Attributes.SetFlag(FieldAttributes.HasFieldRva, value); }
        }

        /// <summary>
        /// Gets or sets the field RVA member containing the initial data of the field (if present).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given field RVA is already added to another field.
        /// </exception>
        public FieldRva FieldRva
        {
            get { return _fieldRva.Value; }
            set
            {
                if (value != null && value.Field != null)
                    throw new InvalidOperationException("Field Rva is already added to another field.");
                if (_fieldRva.Value != null)
                    _fieldRva.Value.Field = null;
                _fieldRva.Value = value;
                if (value != null)
                    value.Field = this;
            }
        }

        /// <summary>
        /// Gets or sets the layout descriptor of the field (if present).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given field layout is already added to another field.
        /// </exception>
        public FieldLayout FieldLayout
        {
            get { return _fieldLayout.Value; }
            set
            {
                if (value != null && value.Field != null)
                    throw new InvalidOperationException("Field layout is already added to another field.");
                if (_fieldLayout.Value != null)
                    _fieldLayout.Value.Field = null;
                _fieldLayout.Value = value;
                if (value != null)
                    value.Field = this;
            }
        }

        /// <summary>
        /// Gets or sets the PInvoke implementation mapping associated to the field (if present).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given implementation map is already added to another member.
        /// </exception>
        public ImplementationMap PInvokeMap
        {
            get { return _pinvokeMap.Value; }
            set { this.SetPInvokeMap(_pinvokeMap, value); }
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
        
        public bool IsPrivate
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Private); }
            set { SetFieldAccessAttribute(FieldAttributes.Private, value); }
        }

        public bool IsFamilyAndAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly); }
            set { SetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly, value); }
        }

        public bool IsFamilyOrAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly); }
            set { SetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly, value); }
        }

        public bool IsAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Assembly); }
            set { SetFieldAccessAttribute(FieldAttributes.Assembly, value); }
        }

        public bool IsFamily
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Family); }
            set { SetFieldAccessAttribute(FieldAttributes.Family, value); }
        }

        public bool IsPublic
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Public); }
            set { SetFieldAccessAttribute(FieldAttributes.Public, value); }
        }

        public bool IsStatic
        {
            get { return Attributes.HasFlag(FieldAttributes.Static); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.Static, value); }
        }

        public bool IsLiteral
        {
            get { return Attributes.HasFlag(FieldAttributes.Literal); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.Literal, value); }
        }

        public bool IsInitOnly
        {
            get { return Attributes.HasFlag(FieldAttributes.InitOnly); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.InitOnly, value); }
        }

        public bool IsSpecialName
        {
            get { return Attributes.HasFlag(FieldAttributes.SpecialName);}
            set { Attributes = Attributes.SetFlag(FieldAttributes.SpecialName, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return Attributes.HasFlag(FieldAttributes.RuntimeSpecialName);}
            set { Attributes = Attributes.SetFlag(FieldAttributes.RuntimeSpecialName, value); }
        }

        private bool GetFieldAccessAttribute(FieldAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)FieldAttributes.FieldAccessMask,
                (uint)attribute);
        }

        private void SetFieldAccessAttribute(FieldAttributes attribute, bool value)
        {
            Attributes = (FieldAttributes)((uint)Attributes).SetMaskedAttribute((uint)FieldAttributes.FieldAccessMask,
                (uint)attribute, value);
        }

        public override string ToString()
        {
            return FullName;
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }
    }
}
