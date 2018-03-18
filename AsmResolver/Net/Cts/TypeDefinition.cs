using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class TypeDefinition : MetadataMember<MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>>, ITypeDefOrRef, IHasSecurityAttribute, IGenericParameterProvider
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private readonly LazyValue<ITypeDefOrRef> _baseType;
        private readonly LazyValue<ClassLayout> _classLayout;
        private readonly LazyValue<PropertyMap> _propertyMap;
        private readonly LazyValue<EventMap> _eventMap;
        private readonly LazyValue<TypeDefinition> _declaringType;
        
        private string _fullName;
        private ModuleDefinition _module;

        public TypeDefinition(string @namespace, string name, ITypeDefOrRef baseType = null)
            : this(@namespace, name, TypeAttributes.Class, baseType)
        {
        }
        
        public TypeDefinition(string @namespace, string name, TypeAttributes attributes, ITypeDefOrRef baseType = null)
            : base(new MetadataToken(MetadataTokenType.TypeDef))
        {
            Attributes = attributes;
            _namespace = new LazyValue<string>(@namespace);
            _name = new LazyValue<string>(name);
            _baseType = new LazyValue<ITypeDefOrRef>(baseType);
            Fields = new DelegatedMemberCollection<TypeDefinition, FieldDefinition>(this, GetFieldOwner, SetFieldOwner);
            Methods = new DelegatedMemberCollection<TypeDefinition, MethodDefinition>(this, GetMethodOwner, SetMethodOwner);

            _classLayout = new LazyValue<ClassLayout>();
            _propertyMap = new LazyValue<PropertyMap>();
            _eventMap = new LazyValue<EventMap>();
            _declaringType = new LazyValue<TypeDefinition>();
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            NestedClasses = new NestedClassCollection(this);
            GenericParameters = new GenericParameterCollection(this);
            Interfaces = new InterfaceImplementationCollection(this);
            MethodImplementations = new MethodImplementationCollection(this);
        }

        internal TypeDefinition(MetadataImage image, MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            Module = image.Assembly.Modules.FirstOrDefault();
            var tableStream = image.Header.GetStream<TableStream>();
            var stringStream = image.Header.GetStream<StringStream>();

            Attributes = row.Column1;

            _name = _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
            _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
            _baseType = new LazyValue<ITypeDefOrRef>(() =>
            {
                var baseTypeToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column4);
                if (baseTypeToken.Rid != 0)
                {
                    IMetadataMember baseType;
                    if (image.TryResolveMember(baseTypeToken, out baseType))
                        return baseType as ITypeDefOrRef;
                }
                return null;
            });

            Fields = new RangedMemberCollection<TypeDefinition, FieldDefinition>(this, MetadataTokenType.Field, 4, GetFieldOwner, SetFieldOwner);
            Methods = new RangedMemberCollection<TypeDefinition, MethodDefinition>(this, MetadataTokenType.Method, 5, GetMethodOwner, SetMethodOwner);
            
            _classLayout = new LazyValue<ClassLayout>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.ClassLayout);
                var layoutRow = table.GetRowByKey(2, row.MetadataToken.Rid);
                return layoutRow != null ? (ClassLayout) table.GetMemberFromRow(image, layoutRow) : null;
            });
            
            _propertyMap = new LazyValue<PropertyMap>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.PropertyMap);
                var mapRow = table.GetRowByKey(0, row.MetadataToken.Rid);
                return mapRow != null ? (PropertyMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            _eventMap = new LazyValue<EventMap>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.EventMap);
                var mapRow = table.GetRowByKey(0, row.MetadataToken.Rid);
                return mapRow != null ? (EventMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            _declaringType = new LazyValue<TypeDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.NestedClass);
                var nestedClassRow = table.GetRowByKey(0, row.MetadataToken.Rid);
                return nestedClassRow != null
                    ? ((NestedClass) table.GetMemberFromRow(image, nestedClassRow)).EnclosingClass
                    : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            NestedClasses = new NestedClassCollection(this);
            GenericParameters = new GenericParameterCollection(this);
            Interfaces = new InterfaceImplementationCollection(this);
            MethodImplementations = new MethodImplementationCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return Module != null ? Module.Image : null; }
        }

        public TypeAttributes Attributes
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

        public string Namespace
        {
            get { return _namespace.Value; }
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
        }

        IResolutionScope ITypeDescriptor.ResolutionScope
        {
            get { return Module; }
        }

        public ITypeDefOrRef BaseType
        {
            get { return _baseType.Value; }
            set { _baseType.Value = value; }
        }

        public Collection<FieldDefinition> Fields
        {
            get;
            private set;
        }

        public Collection<MethodDefinition> Methods
        {
            get;
            private set;
        }

        public ModuleDefinition Module
        {
            get { return _module; }
            internal set
            {
                if (_module != value)
                {
                    _module = value;
                    if (NestedClasses != null)
                    {
                        foreach (var nestedClass in NestedClasses)
                            nestedClass.Class.Module = value;
                    }
                }
            }
        }

        public PropertyMap PropertyMap
        {
            get { return _propertyMap.Value; }
            set
            {
                if (value != null && value.Parent != null)
                    throw new InvalidOperationException("Property map is already added to another type.");
                if (_propertyMap.Value != null)
                    _propertyMap.Value.Parent = null;
                _propertyMap.Value = value;
                if (value != null)
                    value.Parent = this;
            }
        }
        
        public EventMap EventMap
        {
            get { return _eventMap.Value; }
            set
            {
                if (value != null && value.Parent != null)
                    throw new InvalidOperationException("Event map is already added to another type.");
                if (_eventMap.Value != null)
                    _eventMap.Value.Parent = null;
                _eventMap.Value = value;
                if (value != null)
                    value.Parent = this;
            }
        }

        public virtual string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                if (DeclaringType != null)
                    return _fullName = DeclaringType.FullName + '+' + Name;
                return _fullName = string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        public TypeDefinition DeclaringType
        {
            get { return _declaringType.Value; }
            internal set { _declaringType.Value = value; }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public NestedClassCollection NestedClasses
        {
            get;
            private set;
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public SecurityDeclarationCollection SecurityDeclarations
        {
            get;
            private set;
        }

        public GenericParameterCollection GenericParameters
        {
            get;
            private set;
        }

        public InterfaceImplementationCollection Interfaces
        {
            get;
            private set;
        }

        public MethodImplementationCollection MethodImplementations
        {
            get;
            private set;
        }

        public ClassLayout ClassLayout
        {
            get { return _classLayout.Value;}
            set
            {
                if (value != null && value.Parent != null)
                    throw new InvalidOperationException("Class Layout is already added to another type.");
                if (_classLayout.Value != null)
                    _classLayout.Value.Parent = null;
                _classLayout.Value = value;
                if (value != null)
                    value.Parent = this;
            }
        }

        public bool IsNotPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NotPublic); }
            set { SetTypeAccessAttribute(TypeAttributes.NotPublic, value); }
        }

        public bool IsPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.Public); }
            set { SetTypeAccessAttribute(TypeAttributes.Public, value); }
        }

        public bool IsNestedPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedPublic); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedPublic, value); }
        }

        public bool IsNestedPrivate
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedPrivate); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedPrivate, value); }
        }

        public bool IsNestedFamily
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamily); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamily, value); }
        }

        public bool IsNestedAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedAssembly, value); }
        }

        public bool IsNestedFamilyAndAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly, value); }
        }

        public bool IsNestedFamilyOrAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly, value); }
        }

        public bool IsClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Class); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask,
                    (uint)TypeAttributes.Class, value);
            }
        }

        public bool IsInterface
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Interface); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask,
                    (uint)TypeAttributes.Interface, value);
            }
        }

        public bool IsAbstract
        {
            get { return Attributes.HasFlag(TypeAttributes.Abstract); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Abstract, value); }
        }

        public bool IsSealed
        {
            get { return Attributes.HasFlag(TypeAttributes.Sealed); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Sealed, value); }
        }

        public bool IsSpecialName
        {
            get { return Attributes.HasFlag(TypeAttributes.SpecialName); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.SpecialName, value); }
        }

        public bool IsImport
        {
            get { return Attributes.HasFlag(TypeAttributes.Import); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Import, value); }
        }

        public bool IsSerializable
        {
            get { return Attributes.HasFlag(TypeAttributes.Serializable); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Serializable, value); }
        }

        public bool IsAnsiClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.AnsiClass, value);
            }
        }

        public bool IsUnicodeClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.UnicodeClass, value);
            }
        }

        public bool IsAutoClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.AutoClass, value);
            }
        }

        public bool IsBeforeFieldInit
        {
            get { return Attributes.HasFlag(TypeAttributes.BeforeFieldInit); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.BeforeFieldInit, value); }
        }

        public bool IsForwarder
        {
            get { return Attributes.HasFlag(TypeAttributes.Forwarder); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Forwarder, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return Attributes.HasFlag(TypeAttributes.RuntimeSpecialName); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.RuntimeSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return Attributes.HasFlag(TypeAttributes.HasSecurity); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.HasSecurity, value); }
        }

        public bool IsEnum
        {
            get { return BaseType != null && BaseType.IsTypeOf("System", "Enum"); }
        }

        public bool IsValueType
        {
            get
            {
                return BaseType != null &&
                       (BaseType.IsTypeOf("System", "ValueType") || IsEnum);
            }
        }
        private bool GetTypeAccessAttribute(TypeAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.VisibilityMask,
                (uint)attribute);
        }

        private void SetTypeAccessAttribute(TypeAttributes attribute, bool value)
        {
            Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.VisibilityMask,
                (uint)attribute, value);
        }

        public ITypeDescriptor GetElementType()
        {
            return this;
        }

        public IEnumerable<TypeDefinition> GetAllTypes()
        {
            var stack = new Stack<TypeDefinition>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var nestedClass in current.NestedClasses)
                    stack.Push(nestedClass.Class);
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }

        // TODO
        //IGenericParameterProvider IGenericContext.Type
        //{
        //    get { return this; }
        //}

        //IGenericParameterProvider IGenericContext.Method
        //{
        //    get { return null; }
        //}

        private static TypeDefinition GetFieldOwner(FieldDefinition field)
        {
            return field.DeclaringType;
        }

        private static void SetFieldOwner(FieldDefinition field, TypeDefinition type)
        {
            field.DeclaringType = type;
        }

        private static TypeDefinition GetMethodOwner(MethodDefinition method)
        {
            return method.DeclaringType;
        }

        private static void SetMethodOwner(MethodDefinition method, TypeDefinition type)
        {
            method.DeclaringType = type;
        }

    }
}
