using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a type defined in a .NET metadata image. A type is the base of every .NET image and may contain
    /// methods, fields, properties, events and other nested types.
    /// </summary>
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
        public override MetadataImage Image => Module?.Image;

        /// <summary>
        /// Gets or sets the attributes associated to the type.
        /// </summary>
        public TypeAttributes Attributes
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

        /// <summary>
        /// Gets or sets the namespace the type is located in.
        /// </summary>
        /// <remarks>
        /// For a valid assembly, any nested type should have a namespace of <c>null</c>.
        /// This is NOT enforced by AsmResolver however and is also not updated automatically.
        /// </remarks>
        public string Namespace
        {
            get => _namespace.Value;
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
        }

        IResolutionScope ITypeDescriptor.ResolutionScope => Module;

        /// <summary>
        /// Gets or sets the base type of the type.
        /// </summary>
        /// <remarks>
        /// For valid types in an assembly, the base type should always be at least System.Object for any non-interface
        /// type or non-module types.
        /// </remarks>
        public ITypeDefOrRef BaseType
        {
            get => _baseType.Value;
            set => _baseType.Value = value;
        }

        /// <summary>
        /// Gets a collection of fields defined in the type.
        /// </summary>
        public Collection<FieldDefinition> Fields
        {
            get;
        }

        /// <summary>
        /// Gets a collection of methods defined in the type.
        /// </summary>
        public Collection<MethodDefinition> Methods
        {
            get;
        }

        /// <summary>
        /// Gets the module that defines the type.
        /// </summary>
        public ModuleDefinition Module
        {
            get => _module;
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

        /// <summary>
        /// Gets or sets the properties in the type (if available). 
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when assigning a property map that is already added to
        /// another type.</exception>
        public PropertyMap PropertyMap
        {
            get => _propertyMap.Value;
            set
            {
                if (value?.Parent != null)
                    throw new InvalidOperationException("Property map is already added to another type.");
                if (_propertyMap.Value != null)
                    _propertyMap.Value.Parent = null;
                _propertyMap.Value = value;
                if (value != null)
                    value.Parent = this;
            }
        }
        
        /// <summary>
        /// Gets or sets the events in the type (if available). 
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when assigning an event map that is already added to
        /// another type.</exception>
        public EventMap EventMap
        {
            get => _eventMap.Value;
            set
            {
                if (value?.Parent != null)
                    throw new InvalidOperationException("Event map is already added to another type.");
                if (_eventMap.Value != null)
                    _eventMap.Value.Parent = null;
                _eventMap.Value = value;
                if (value != null)
                    value.Parent = this;
            }
        }

        /// <inheritdoc />
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

        /// <summary>
        /// When the type is nested, gets the enclosing type that declares this type.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            internal set => _declaringType.Value = value;
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor => DeclaringType;

        ITypeDefOrRef IMemberReference.DeclaringType => DeclaringType;

        /// <summary>
        /// Gets a collection of nested classes declared in this type.
        /// </summary>
        public NestedClassCollection NestedClasses
        {
            get;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <inheritdoc />
        public SecurityDeclarationCollection SecurityDeclarations
        {
            get;
        }

        /// <inheritdoc />
        public GenericParameterCollection GenericParameters
        {
            get;
        }

        /// <summary>
        /// Gets a collection of interfaces that are being implemented by this type.
        /// </summary>
        public InterfaceImplementationCollection Interfaces
        {
            get;
        }

        /// <summary>
        /// Gets a collection of mappings that are used to implement the methods defined by the interfaces the type is
        /// using.
        /// </summary>
        public MethodImplementationCollection MethodImplementations
        {
            get;
        }

        /// <summary>
        /// Gets or sets the raw layout of the class (if available).
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when assigning a class layout that is already assigned
        /// to another type.</exception>
        public ClassLayout ClassLayout
        {
            get => _classLayout.Value;
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

        /// <summary>
        /// Gets or sets a value indicating whether the type is not public.
        /// </summary>
        public bool IsNotPublic
        {
            get => GetTypeAccessAttribute(TypeAttributes.NotPublic);
            set => SetTypeAccessAttribute(TypeAttributes.NotPublic, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is public.
        /// </summary>
        /// <remarks>
        /// For nested types, use <see cref="IsNestedPublic"/> instead. 
        /// </remarks>
        public bool IsPublic
        {
            get => GetTypeAccessAttribute(TypeAttributes.Public);
            set => SetTypeAccessAttribute(TypeAttributes.Public, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a nested public class.
        /// </summary>
        /// <remarks>
        /// For non-nested types, use <see cref="IsPublic"/> instead. 
        /// </remarks>
        public bool IsNestedPublic
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedPublic);
            set => SetTypeAccessAttribute(TypeAttributes.NestedPublic, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a nested private class.
        /// </summary>
        public bool IsNestedPrivate
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedPrivate);
            set => SetTypeAccessAttribute(TypeAttributes.NestedPrivate, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested and visible only within its own family.
        /// </summary>
        public bool IsNestedFamily
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedFamily);
            set => SetTypeAccessAttribute(TypeAttributes.NestedFamily, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested and visible only within its own assembly.
        /// </summary>
        public bool IsNestedAssembly
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedAssembly);
            set => SetTypeAccessAttribute(TypeAttributes.NestedAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested and visible only to classes that belong to
        /// both its own family and its own assembly.
        /// </summary>
        public bool IsNestedFamilyAndAssembly
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly);
            set => SetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested and visible only to classes that belong to
        /// either its own family or to its own assembly.
        /// </summary>
        public bool IsNestedFamilyOrAssembly
        {
            get => GetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly);
            set => SetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a class.
        /// </summary>
        public bool IsClass
        {
            get => ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Class);
            set => Attributes = (TypeAttributes) ((uint) Attributes).SetMaskedAttribute(
                (uint) TypeAttributes.ClassSemanticsMask,
                (uint) TypeAttributes.Class, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get => ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Interface);
            set => Attributes = (TypeAttributes) ((uint) Attributes).SetMaskedAttribute(
                (uint) TypeAttributes.ClassSemanticsMask,
                (uint) TypeAttributes.Interface, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is defined as abstract and needs to be extended first
        /// before it can be instantiated.
        /// </summary>
        public bool IsAbstract
        {
            get => Attributes.HasFlag(TypeAttributes.Abstract);
            set => Attributes = Attributes.SetFlag(TypeAttributes.Abstract, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is defined as sealed and cannot be extended any further.
        /// </summary>
        public bool IsSealed
        {
            get => Attributes.HasFlag(TypeAttributes.Sealed);
            set => Attributes = Attributes.SetFlag(TypeAttributes.Sealed, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type has a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => Attributes.HasFlag(TypeAttributes.SpecialName);
            set => Attributes = Attributes.SetFlag(TypeAttributes.SpecialName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type has a <see cref="ComImportAttribute"> attribute applied,
        /// indicating that it was imported from a COM type library.
        /// </summary>
        public bool IsImport
        {
            get => Attributes.HasFlag(TypeAttributes.Import);
            set => Attributes = Attributes.SetFlag(TypeAttributes.Import, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is serializable.
        /// </summary>
        public bool IsSerializable
        {
            get => Attributes.HasFlag(TypeAttributes.Serializable);
            set => Attributes = Attributes.SetFlag(TypeAttributes.Serializable, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ANSI string format attribute is selected for the type.
        /// </summary>
        public bool IsAnsiClass
        {
            get => ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass);
            set => Attributes = (TypeAttributes) ((uint) Attributes).SetMaskedAttribute(
                (uint) TypeAttributes.StringFormatMask,
                (uint) TypeAttributes.AnsiClass, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Unicode string format attribute is selected for the type.
        /// </summary>
        public bool IsUnicodeClass
        {
            get => ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass);
            set => Attributes = (TypeAttributes) ((uint) Attributes).SetMaskedAttribute(
                (uint) TypeAttributes.StringFormatMask,
                (uint) TypeAttributes.UnicodeClass, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the auto string format attribute is selected for the type.
        /// </summary>
        public bool IsAutoClass
        {
            get => ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass);
            set => Attributes = (TypeAttributes) ((uint) Attributes).SetMaskedAttribute(
                (uint) TypeAttributes.StringFormatMask,
                (uint) TypeAttributes.AutoClass, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type should be initialized before any static field is accessed.
        /// </summary>
        public bool IsBeforeFieldInit
        {
            get => Attributes.HasFlag(TypeAttributes.BeforeFieldInit);
            set => Attributes = Attributes.SetFlag(TypeAttributes.BeforeFieldInit, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an exported forwarder type.
        /// </summary>
        public bool IsForwarder
        {
            get => Attributes.HasFlag(TypeAttributes.Forwarder);
            set => Attributes = Attributes.SetFlag(TypeAttributes.Forwarder, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type has a special name used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => Attributes.HasFlag(TypeAttributes.RuntimeSpecialName);
            set => Attributes = Attributes.SetFlag(TypeAttributes.RuntimeSpecialName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type has security attributes associated to it. 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        public bool HasSecurity
        {
            get => Attributes.HasFlag(TypeAttributes.HasSecurity);
            set => Attributes = Attributes.SetFlag(TypeAttributes.HasSecurity, value);
        }

        /// <summary>
        /// Gets a value indicating whether the type is an enum type.
        /// </summary>
        public bool IsEnum => BaseType != null && BaseType.IsTypeOf("System", "Enum");

        /// <summary>
        /// Gets a value indicating whether the type is a value type.
        /// </summary>
        public bool IsValueType => BaseType != null &&
                                   (BaseType.IsTypeOf("System", "ValueType") || IsEnum);

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

        /// <inheritdoc />
        public ITypeDescriptor GetElementType()
        {
            return this;
        }

        /// <inheritdoc />
        public TypeSignature ToTypeSignature()
        {
            return new TypeDefOrRefSignature(this);
        }

        /// <summary>
        /// Gets a collection of all the nested types declared in this type.
        /// The collection starts off with the type itself. 
        /// </summary>
        /// <returns>A collection of types.</returns>
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
