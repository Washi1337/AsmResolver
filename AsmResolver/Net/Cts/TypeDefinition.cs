using System;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class TypeDefinition : MetadataMember<MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>>, ITypeDefOrRef, IHasSecurityAttribute, IGenericParameterProvider
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private readonly LazyValue<ITypeDefOrRef> _baseType;
        private readonly LazyValue<ModuleDefinition> _module;
        private readonly LazyValue<ClassLayout> _classLayout;
        private readonly LazyValue<PropertyMap> _propertyMap;
        private readonly LazyValue<EventMap> _eventMap;
        private readonly LazyValue<TypeDefinition> _declaringType;
        
        private string _fullName;

        public TypeDefinition(string @namespace, string name)
            : this(@namespace, name, null)
        {
        }

        public TypeDefinition(string @namespace, string name, ITypeDefOrRef baseType)
            : base(null, new MetadataToken(MetadataTokenType.TypeDef))
        {
            _namespace = new LazyValue<string>(@namespace);
            _name = new LazyValue<string>(name);
            _baseType = new LazyValue<ITypeDefOrRef>(baseType);
            Fields = new DelegatedMemberCollection<TypeDefinition, FieldDefinition>(this, GetFieldOwner, SetFieldOwner);
            Methods = new DelegatedMemberCollection<TypeDefinition, MethodDefinition>(this, GetMethodOwner, SetMethodOwner);

            _module = new LazyValue<ModuleDefinition>(default(ModuleDefinition));
            _classLayout = new LazyValue<ClassLayout>(default(ClassLayout));
            _propertyMap = new LazyValue<PropertyMap>(default(PropertyMap));
            _eventMap = new LazyValue<EventMap>(default(EventMap));
            _declaringType = new LazyValue<TypeDefinition>(default(TypeDefinition));
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            NestedClasses = new NestedClassCollection(this);
            GenericParameters = new GenericParameterCollection(this);
            Interfaces = new InterfaceImplementationCollection(this);
            MethodImplementations = new MethodImplementationCollection(this);
        }

        internal TypeDefinition(MetadataImage image, MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
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

            _module = new LazyValue<ModuleDefinition>(() =>
                (ModuleDefinition) Image.ResolveMember(new MetadataToken(MetadataTokenType.Module, 1)));
            
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

        public IResolutionScope ResolutionScope
        {
            get;
            internal set;
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
            get { return _module.Value; }
            internal set { _module.Value = value; }
        }

        public PropertyMap PropertyMap
        {
            get { return _propertyMap.Value; }
            set { _propertyMap.Value = value; }
        }
        
        public EventMap EventMap
        {
            get { return _eventMap.Value; }
            set { _eventMap.Value = value; }
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
            private set { _classLayout.Value = value; }
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

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            var typeRow = new MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>
            {
                Column1 = Attributes,
                Column2 = buffer.StringStreamBuffer.GetStringOffset(Name),
                Column3 = buffer.StringStreamBuffer.GetStringOffset(Namespace),
                Column4 = BaseType != null ? tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).EncodeToken(BaseType.MetadataToken) : 0,
            };
            tableStream.GetTable<TypeDefinitionTable>().Add(typeRow);
            
            foreach (var method in Methods)
                method.AddToBuffer(buffer);
            typeRow.Column6 = Methods.Count == 0
                ? (uint) Math.Max(1, tableStream.GetTable(MetadataTokenType.Method).Count)
                : Methods[0].MetadataToken.Rid;

            foreach (var field in Fields)
                field.AddToBuffer(buffer);
            typeRow.Column5 = Fields.Count == 0
                ? (uint) Math.Max(1, tableStream.GetTable(MetadataTokenType.Field).Count)
                : Fields[0].MetadataToken.Rid;

            foreach (var attribute in CustomAttributes)
                attribute.AddToBuffer(buffer);
            foreach (var declaration in SecurityDeclarations)
                declaration.AddToBuffer(buffer);
            foreach (var parameter in GenericParameters)
                parameter.AddToBuffer(buffer);
            foreach (var @interface in Interfaces)
                @interface.AddToBuffer(buffer);
            foreach (var impl in MethodImplementations)
                impl.AddToBuffer(buffer);
            if (ClassLayout != null)
                ClassLayout.AddToBuffer(buffer);
            if (PropertyMap != null)
                PropertyMap.AddToBuffer(buffer);
            if (EventMap != null)
                EventMap.AddToBuffer(buffer);
        }
    }
}
