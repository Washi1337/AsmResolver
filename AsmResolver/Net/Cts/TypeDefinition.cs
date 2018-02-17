
using System.Collections;
using System.Runtime.Remoting.Messaging;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Cts.Collections;

namespace AsmResolver.Net.Metadata
{
    public class TypeDefinition : MetadataMember<MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>>, ITypeDefOrRef, IHasSecurityAttribute, IGenericParameterProvider
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private readonly LazyValue<ITypeDefOrRef> _baseType;
        private readonly LazyValue<ModuleDefinition> _module;
        private readonly LazyValue<ClassLayout> _classLayout;
        //private PropertyMap _propertyMap;
        //private EventMap _eventMap;
        //private NestedClassCollection _nestedClasses;
        //private GenericParameterCollection _genericParameters;
        //private InterfaceImplementationCollection _interfaces;
        //private MethodImplementationCollection _methodImplementations;
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
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
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
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
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

        //public PropertyMap PropertyMap
        //{
        //    get
        //    {
        //        if (_propertyMap != null)
        //            return _propertyMap;
        //        return Header == null
        //            ? (_propertyMap = new PropertyMap(this))
        //            : (_propertyMap =
        //                Header.GetStream<TableStream>().GetTable<PropertyMap>().FirstOrDefault(x => x.Parent == this));
        //    }
        //}

        //public EventMap EventMap
        //{
        //    get
        //    {
        //        if (_eventMap != null)
        //            return _eventMap;
        //        return Header == null
        //            ? (_eventMap = new EventMap(this))
        //            : (_eventMap =
        //                Header.GetStream<TableStream>().GetTable<EventMap>().FirstOrDefault(x => x.Parent == this));
        //    }
        //}

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
            get
            {
                // TODO
                return null;
                //if (Image == null)
                //    return null;
                //var tableStream = Image.Header.GetStream<TableStream>();
                //if (tableStream == null)
                //    return null;
                //var nestedClassTable = tableStream.GetTable<NestedClass>();
                //if (nestedClassTable == null)
                //    return null;
                //var nestedClass = nestedClassTable.FirstOrDefault(x => x.Class == this);
                //if (nestedClass == null)
                //    return null;
                //return nestedClass.EnclosingClass;
            }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        // TODO
        //public NestedClassCollection NestedClasses
        //{
        //    get { return _nestedClasses ?? (_nestedClasses = new NestedClassCollection(this)); }
        //}

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

        // TODO
        //public GenericParameterCollection GenericParameters
        //{
        //    get { return _genericParameters ?? (_genericParameters = new GenericParameterCollection(this)); }
        //}

        //public InterfaceImplementationCollection Interfaces
        //{
        //    get { return _interfaces ?? (_interfaces = new InterfaceImplementationCollection(this)); }
        //}

        //public MethodImplementationCollection MethodImplementations
        //{
        //    get { return _methodImplementations ?? (_methodImplementations = new MethodImplementationCollection(this)); }
        //}

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
    }
}
