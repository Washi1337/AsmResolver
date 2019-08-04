using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a field definition that is declared in a type.
    /// </summary>
    public class FieldDefinition : 
        MetadataMember<MetadataRow<FieldAttributes, uint, uint>>,
        IMemberDefinition,
        ICallableMemberReference, 
        IHasConstant, 
        IHasFieldMarshal, 
        IMemberForwarded
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
        private readonly MetadataImage _image;

        public FieldDefinition(string name, FieldAttributes attributes, FieldSignature signature)
            : base(new MetadataToken(MetadataTokenType.Field))
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));

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
                FieldSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3), true));

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
        public override MetadataImage Image => _declaringType.IsInitialized && _declaringType.Value != null 
            ? _declaringType.Value.Image
            : _image;

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
            get => _name.Value;
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName => _fullName ?? (_fullName = this.GetFullName(Signature));

        /// <summary>
        /// Gets the type that declares the field.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get { return _declaringType.Value;}
            internal set { _declaringType.Value = value; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType => DeclaringType;

        /// <summary>
        /// Gets or sets the signature of the field, containing e.g. the field type.
        /// </summary>
        public FieldSignature Signature
        {
            get => _signature.Value;
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature => Signature;

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given constant member is already added to another field.
        /// </exception>
        public Constant Constant
        {
            get => _constant.Value;
            set => this.SetConstant(_constant, value);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given field marshal is already added to another field.
        /// </exception>
        public FieldMarshal FieldMarshal
        {
            get => _fieldMarshal.Value;
            set => this.SetFieldMarshal(_fieldMarshal, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a field RVA member associated, which indicates
        /// the field has an initial value that is loaded upon runtime.
        /// </summary>
        public bool HasFieldRva
        {
            get => Attributes.HasFlag(FieldAttributes.HasFieldRva);
            set => Attributes.SetFlag(FieldAttributes.HasFieldRva, value);
        }

        /// <summary>
        /// Gets or sets the field RVA member containing the initial data of the field (if present).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the given field RVA is already added to another field.
        /// </exception>
        public FieldRva FieldRva
        {
            get => _fieldRva.Value;
            set
            {
                if (value?.Field != null)
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
            get => _fieldLayout.Value;
            set
            {
                if (value?.Field != null)
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
            get => _pinvokeMap.Value;
            set => this.SetPInvokeMap(_pinvokeMap, value);
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the field is private.
        /// </summary>
        public bool IsPrivate
        {
            get => GetFieldAccessAttribute(FieldAttributes.Private);
            set => SetFieldAccessAttribute(FieldAttributes.Private, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visibility of this field is described by FamANDAssem; that is,
        /// the field can be accessed from derived classes, but only if they are in the same assembly.
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get => GetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly);
            set => SetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the potential visibility of this field is described by FamORAssem;
        /// that is, the field can be accessed by derived classes wherever they are, and by classes in the same assembly.
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get => GetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly);
            set => SetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the potential visibility of this field is described by Assembly;
        /// that is, the field is visible at most to other types in the same assembly, and is not visible to derived
        /// types outside the assembly.
        /// </summary>
        public bool IsAssembly
        {
            get => GetFieldAccessAttribute(FieldAttributes.Assembly);
            set => SetFieldAccessAttribute(FieldAttributes.Assembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visibility of this field is described by Family; that is, the
        /// field is visible only within its class and derived classes.
        /// </summary>
        public bool IsFamily
        {
            get => GetFieldAccessAttribute(FieldAttributes.Family);
            set => SetFieldAccessAttribute(FieldAttributes.Family, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is public.
        /// </summary>
        public bool IsPublic
        {
            get => GetFieldAccessAttribute(FieldAttributes.Public);
            set => SetFieldAccessAttribute(FieldAttributes.Public, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is static.
        /// </summary>
        public bool IsStatic
        {
            get => Attributes.HasFlag(FieldAttributes.Static);
            set => Attributes = Attributes.SetFlag(FieldAttributes.Static, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of the field is written at compile time and cannot
        /// be changed.
        /// </summary>
        public bool IsLiteral
        {
            get => Attributes.HasFlag(FieldAttributes.Literal);
            set => Attributes = Attributes.SetFlag(FieldAttributes.Literal, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field can only be assigned a new value in the constructors
        /// of the parent type.
        /// </summary>
        public bool IsInitOnly
        {
            get => Attributes.HasFlag(FieldAttributes.InitOnly);
            set => Attributes = Attributes.SetFlag(FieldAttributes.InitOnly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a special name. 
        /// </summary>
        public bool IsSpecialName
        {
            get => Attributes.HasFlag(FieldAttributes.SpecialName);
            set => Attributes = Attributes.SetFlag(FieldAttributes.SpecialName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a special name used by the runtime. 
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => Attributes.HasFlag(FieldAttributes.RuntimeSpecialName);
            set => Attributes = Attributes.SetFlag(FieldAttributes.RuntimeSpecialName, value);
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

        public bool IsAccessibleFromType(TypeDefinition type)
        {
            if (!DeclaringType.IsAccessibleFromType(type))
                return false;
            
            var comparer = new SignatureComparer();
            bool isInSameAssembly = comparer.Equals(DeclaringType.Module, type.Module);

            return IsPublic
                   || isInSameAssembly && IsAssembly
                   || comparer.Equals(DeclaringType, type);
            // TODO: check if in the same family of declaring types.
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }
    }
}
