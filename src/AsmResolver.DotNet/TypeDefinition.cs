using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type (a class, interface or structure) defined in a .NET module.
    /// </summary>
    public class TypeDefinition :
        MetadataMember,
        ITypeDefOrRef,
        IMemberDefinition,
        IHasGenericParameters,
        IHasSecurityDeclaration,
        IOwnedCollectionElement<ModuleDefinition>,
        IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<string> _namespace;
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<ITypeDefOrRef> _baseType;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<ClassLayout> _classLayout;
        private IList<TypeDefinition> _nestedTypes;
        private ModuleDefinition _module;
        private IList<FieldDefinition> _fields;
        private IList<MethodDefinition> _methods;
        private IList<PropertyDefinition> _properties;
        private IList<EventDefinition> _events;
        private IList<CustomAttribute> _customAttributes;
        private IList<SecurityDeclaration> _securityDeclarations;
        private IList<GenericParameter> _genericParameters;
        private IList<InterfaceImplementation> _interfaces;
        private IList<MethodImplementation> _methodImplementations;

        /// <summary>
        /// Initializes a new type definition.
        /// </summary>
        /// <param name="token">The token of the type definition.</param>
        protected TypeDefinition(MetadataToken token)
            : base(token)
        {
            _namespace = new LazyVariable<string>(GetNamespace);
            _name = new LazyVariable<string>(GetName);
            _baseType = new LazyVariable<ITypeDefOrRef>(GetBaseType);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
            _classLayout = new LazyVariable<ClassLayout>(GetClassLayout);
        }

        /// <summary>
        /// Creates a new type definition.
        /// </summary>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="attributes">The attributes associated to the type.</param>
        public TypeDefinition(string ns, string name, TypeAttributes attributes)
            : this(ns, name, attributes, null)
        {
        }

        /// <summary>
        /// Creates a new type definition.
        /// </summary>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="attributes">The attributes associated to the type.</param>
        /// <param name="baseType">The super class that this type extends.</param>
        public TypeDefinition(string ns, string name, TypeAttributes attributes, ITypeDefOrRef baseType)
            : this(new MetadataToken(TableIndex.TypeDef, 0))
        {
            Namespace = ns;
            Name = name;
            Attributes = attributes;
            BaseType = baseType;
        }

        /// <summary>
        /// Gets or sets the namespace the type resides in.
        /// </summary>
        public string Namespace
        {
            get => _namespace.Value;
            set => _namespace.Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets the full name (including namespace or declaring type full name) of the type.
        /// </summary>
        public string FullName => this.GetTypeFullName();

        /// <summary>
        /// Gets or sets the attributes associated to the type.
        /// </summary>
        public TypeAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the type is in a public scope or not.
        /// </summary>
        public bool IsNotPublic
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic;
            set => Attributes = value ? Attributes & ~TypeAttributes.VisibilityMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is in a public scope or not.
        /// </summary>
        public bool IsPublic
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.Public : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with public visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedPublic
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedPublic : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with private visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedPrivate
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedPrivate : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with family visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedFamily
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedFamily : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with assembly visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedAssembly
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with family and assembly visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedFamilyAndAssembly
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamilyAndAssembly;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedFamilyAndAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nested with family or assembly visibility.
        /// </summary>
        /// <remarks>
        /// Updating the value of this property does not automatically make the type nested in another type.
        /// Similarly, adding this type to another enclosing type will not automatically update this property.
        /// </remarks>
        public bool IsNestedFamilyOrAssembly
        {
            get => (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamilyOrAssembly;
            set => Attributes = (Attributes & ~TypeAttributes.VisibilityMask)
                                | (value ? TypeAttributes.NestedFamilyOrAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the fields of the type are auto-laid out by the
        /// common language runtime (CLR).
        /// </summary>
        public bool IsAutoLayout
        {
            get => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout;
            set => Attributes = value ? (Attributes & ~TypeAttributes.LayoutMask) : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the fields of the type are laid out sequentially.
        /// </summary>
        public bool IsSequentialLayout
        {
            get => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;
            set => Attributes = (Attributes & ~TypeAttributes.LayoutMask)
                                | (value ? TypeAttributes.SequentialLayout : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the fields of the type are laid out explicitly.
        /// </summary>
        public bool IsExplicitLayout
        {
            get => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;
            set => Attributes = (Attributes & ~TypeAttributes.LayoutMask)
                                | (value ? TypeAttributes.ExplicitLayout : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a class.
        /// </summary>
        public bool IsClass
        {
            get => (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class;
            set => Attributes = value ? Attributes & ~TypeAttributes.ClassSemanticsMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an interface.
        /// </summary>
        public bool IsInterface
        {
            get => (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
            set => Attributes = (Attributes & ~TypeAttributes.ClassSemanticsMask)
                                | (value ? TypeAttributes.Interface : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is defined abstract and should be extended before
        /// an object can be instantiated.
        /// </summary>
        public bool IsAbstract
        {
            get => (Attributes & TypeAttributes.Abstract) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Abstract)
                                | (value ? TypeAttributes.Abstract : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is defined sealed and cannot be extended by a sub class.
        /// </summary>
        public bool IsSealed
        {
            get => (Attributes & TypeAttributes.Sealed) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Sealed)
                                | (value ? TypeAttributes.Sealed : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type has a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & TypeAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.SpecialName)
                                | (value ? TypeAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should check the encoding of the name.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & TypeAttributes.Forwarder) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Forwarder)
                                | (value ? TypeAttributes.Forwarder : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is imported.
        /// </summary>
        public bool IsImport
        {
            get => (Attributes & TypeAttributes.Import) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Import)
                                | (value ? TypeAttributes.Import : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is serializable.
        /// </summary>
        public bool IsSerializable
        {
            get => (Attributes & TypeAttributes.Serializable) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Serializable)
                                | (value ? TypeAttributes.Serializable : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether LPTSTR string instances are interpreted as ANSI strings.
        /// </summary>
        public bool IsAnsiClass
        {
            get => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass;
            set => Attributes = value ? Attributes & ~TypeAttributes.StringFormatMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether LPTSTR string instances are interpreted as Unicode strings.
        /// </summary>
        public bool IsUnicodeClass
        {
            get => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;
            set => Attributes = (Attributes & ~TypeAttributes.StringFormatMask)
                                | (value ? TypeAttributes.UnicodeClass : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether LPTSTR string instances are interpreted automatically by the runtime.
        /// </summary>
        public bool IsAutoClass
        {
            get => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;
            set => Attributes = (Attributes & ~TypeAttributes.StringFormatMask)
                                | (value ? TypeAttributes.AutoClass : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether LPTSTR string instances are interpreted using a non-standard encoding.
        /// </summary>
        public bool IsCustomFormatClass
        {
            get => (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.CustomFormatClass;
            set => Attributes = (Attributes & ~TypeAttributes.StringFormatMask)
                                | (value ? TypeAttributes.CustomFormatClass : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the runtime should initialize the class before any time before the first
        /// static field access.
        /// </summary>
        public bool IsBeforeFieldInit
        {
            get => (Attributes & TypeAttributes.BeforeFieldInit) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.BeforeFieldInit)
                                | (value ? TypeAttributes.BeforeFieldInit : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the type is an exported type and forwards the definition to another module.
        /// </summary>
        public bool IsForwarder
        {
            get => (Attributes & TypeAttributes.Forwarder) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.Forwarder)
                                | (value ? TypeAttributes.Forwarder : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the type has additional security attributes associated to it.
        /// </summary>
        public bool HasSecurity
        {
            get => (Attributes & TypeAttributes.HasSecurity) != 0;
            set => Attributes = (Attributes & ~TypeAttributes.HasSecurity)
                                | (value ? TypeAttributes.HasSecurity : 0);
        }

        /// <summary>
        /// Gets or sets the super class that this type extends.
        /// </summary>
        public ITypeDefOrRef BaseType
        {
            get => _baseType.Value;
            set => _baseType.Value = value;
        }

        /// <summary>
        /// Gets the module that defines the type.
        /// </summary>
        public ModuleDefinition Module => DeclaringType != null ? DeclaringType.Module : _module;

        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => _module = value;
        }

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
        }

        ITypeDefOrRef ITypeDefOrRef.DeclaringType => DeclaringType;

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <summary>
        /// Gets a value indicating whether the type is enclosed by another type.
        /// </summary>
        public bool IsNested => DeclaringType != null;

        /// <summary>
        /// Gets a collection of nested types that this type defines.
        /// </summary>
        public IList<TypeDefinition> NestedTypes
        {
            get
            {
                if (_nestedTypes is null)
                    Interlocked.CompareExchange(ref _nestedTypes, GetNestedTypes(), null);
                return _nestedTypes;
            }
        }

        IResolutionScope ITypeDescriptor.Scope => GetDeclaringScope();

        /// <inheritdoc />
        public bool IsValueType => BaseType is {} && (BaseType.IsTypeOf("System", nameof(ValueType)) || IsEnum);

        /// <summary>
        /// Gets a value indicating whether the type defines an enumeration of discrete values.
        /// </summary>
        public bool IsEnum => BaseType?.IsTypeOf("System", nameof(Enum)) ?? false;

        /// <summary>
        /// Gets a value indicating whether the type describes a delegate referring to a method.
        /// </summary>
        public bool IsDelegate
        {
            get
            {
                var baseType = BaseType;
                if (baseType is null)
                    return false;

                return baseType.IsTypeOf("System", nameof(Delegate))
                    || baseType.IsTypeOf("System", nameof(MulticastDelegate));
            }
        }

        /// <summary>
        /// Determines whether the type is marked as read-only.
        /// </summary>
        public bool IsReadOnly =>
            IsValueType
            && this.HasCustomAttribute("System.Runtime.CompilerServices", nameof(ReadOnlyAttribute));

        /// <summary>
        /// Gets a collection of fields defined in the type.
        /// </summary>
        public IList<FieldDefinition> Fields
        {
            get
            {
                if (_fields is null)
                    Interlocked.CompareExchange(ref _fields, GetFields(), null);
                return _fields;
            }
        }

        /// <summary>
        /// Gets a collection of methods defined in the type.
        /// </summary>
        public IList<MethodDefinition> Methods
        {
            get
            {
                if (_methods is null)
                    Interlocked.CompareExchange(ref _methods, GetMethods(), null);
                return _methods;
            }
        }

        /// <summary>
        /// Gets a collection of properties defined in the type.
        /// </summary>
        public IList<PropertyDefinition> Properties
        {
            get
            {
                if (_properties is null)
                    Interlocked.CompareExchange(ref _properties, GetProperties(), null);
                return _properties;
            }
        }

        /// <summary>
        /// Gets a collection of events defined in the type.
        /// </summary>
        public IList<EventDefinition> Events
        {
            get
            {
                if (_events is null)
                    Interlocked.CompareExchange(ref _events, GetEvents(), null);
                return _events;
            }
        }

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <inheritdoc />
        public IList<SecurityDeclaration> SecurityDeclarations
        {
            get
            {
                if (_securityDeclarations is null)
                    Interlocked.CompareExchange(ref _securityDeclarations, GetSecurityDeclarations(), null);
                return _securityDeclarations;
            }
        }

        /// <inheritdoc />
        public IList<GenericParameter> GenericParameters
        {
            get
            {
                if (_genericParameters is null)
                    Interlocked.CompareExchange(ref _genericParameters, GetGenericParameters(), null);
                return _genericParameters;
            }
        }

        /// <summary>
        /// Gets a collection of interfaces that are implemented by the type.
        /// </summary>
        public IList<InterfaceImplementation> Interfaces
        {
            get
            {
                if (_interfaces is null)
                    Interlocked.CompareExchange(ref _interfaces, GetInterfaces(), null);
                return _interfaces;
            }
        }

        /// <summary>
        /// Gets a collection of methods that are explicitly implemented by the type.
        /// </summary>
        public IList<MethodImplementation> MethodImplementations
        {
            get
            {
                if (_methodImplementations is null)
                    Interlocked.CompareExchange(ref _methodImplementations, GetMethodImplementations(), null);
                return _methodImplementations;
            }
        }

        /// <summary>
        /// Gets or sets an override to the layout of a class, indicating its total and packing size.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, the runtime decides the layout of the class.
        /// </remarks>
        public ClassLayout ClassLayout
        {
            get => _classLayout.Value;
            set => _classLayout.Value = value;
        }

        /// <summary>
        /// Determines whether the type inherits from a particular type
        /// </summary>
        /// <param name="fullName">The full name of the type</param>
        /// <returns>Whether the current <see cref="TypeDefinition"/> inherits the type</returns>
        public bool InheritsFrom(string fullName)
        {
            var type = this;
            do
            {
                if (type.FullName == fullName)
                    return true;

                var current = type;
                type = type.BaseType?.Resolve();

                // This prevents an issue where the base type is the same as itself
                // ... so basically a cyclic dependency
                if (current == type)
                    return false;
            } while (type is {});

            return false;
        }

        /// <summary>
        /// Determines whether the type implements a particular interface
        /// </summary>
        /// <param name="fullName">The full name of the interface</param>
        /// <returns>Whether the type implements the interface</returns>
        public bool Implements(string fullName)
        {
            var type = this;
            do
            {
                if (type.Interfaces.Any(@interface => @interface.Interface.FullName == fullName))
                    return true;

                var current = type;
                type = type.BaseType?.Resolve();

                // This prevents an issue where the base type is the same as itself
                // ... so basically a cyclic dependency
                if (current == type)
                    return false;
            } while (type is {});

            return false;
        }

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() =>
            new TypeDefOrRefSignature(this, IsValueType);

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type)
        {
            // TODO: Check types of the same family.

            if (this == type)
                return true;

            var comparer = new SignatureComparer();
            bool isInSameAssembly = comparer.Equals(Module, type.Module);

            if (IsNested)
            {
                if (!DeclaringType.IsAccessibleFromType(type))
                    return false;

                return IsNestedPublic
                       || isInSameAssembly && IsNestedAssembly
                       || DeclaringType == type;
            }

            return IsPublic
                   || isInSameAssembly;
        }

        /// <summary>
        /// Creates a new type reference to this type definition.
        /// </summary>
        /// <returns>The type reference.</returns>
        public TypeReference ToTypeReference()
        {
            var scope = DeclaringType is null
                ? (IResolutionScope) Module
                : DeclaringType.ToTypeReference();

            return new TypeReference(Module, scope, Namespace, Name);
        }

        private IResolutionScope GetDeclaringScope()
        {
            if (DeclaringType is null)
                return Module;

            return DeclaringType.ToTypeReference();
        }

        TypeDefinition ITypeDescriptor.Resolve() => this;

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        /// <summary>
        /// When this type is an enum, extracts the underlying enum type.
        /// </summary>
        /// <returns>The type, or <c>null</c> if none was found.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the type is not an enum.</exception>
        /// <remarks>
        /// To verify whether a type is an enum or not, use the <see cref="IsEnum"/> property.
        /// </remarks>
        public TypeSignature GetEnumUnderlyingType()
        {
            if (!IsEnum)
                throw new InvalidOperationException("Type is not an enum.");

            foreach (var field in Fields)
            {
                if (!field.IsLiteral && !field.IsStatic && field.Signature != null)
                    return field.Signature.FieldType;
            }

            return null;
        }

        /// <summary>
        /// Gets the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition GetStaticConstructor()
        {
            return Methods.FirstOrDefault(m =>
                m.IsPrivate
                && m.IsConstructor
                && m.IsStatic
                && m.Parameters.Count == 0);
        }

        /// <summary>
        /// Gets or creates the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        /// <remarks>
        /// If the static constructor was not present in the type, it will be inserted as the first method in the type.
        /// This method can only be used when the type has already been added to the metadata image.
        /// </remarks>
        public MethodDefinition GetOrCreateStaticConstructor() => GetOrCreateStaticConstructor(Module);

        /// <summary>
        /// Gets or creates the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <param name="module">The image to use for creating the signature of the constructor if it is not present yet.</param>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        /// <remarks>
        /// If the static constructor was not present in the type, it will be inserted as the first method in the type.
        /// </remarks>
        public MethodDefinition GetOrCreateStaticConstructor(ModuleDefinition module)
        {
            var cctor = GetStaticConstructor();
            if (cctor == null)
            {
                if (module == null)
                    throw new ArgumentNullException(nameof(module));

                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Private
                    | MethodAttributes.Static
                    | MethodAttributes.SpecialName
                    | MethodAttributes.RuntimeSpecialName,
                    MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

                cctor.CilMethodBody = new CilMethodBody(cctor);
                cctor.CilMethodBody.Instructions.Add(new CilInstruction(0, CilOpCodes.Ret));

                Methods.Insert(0, cctor);
            }

            return cctor;
        }

        /// <summary>
        /// Obtains the namespace of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual string GetNamespace() => null;

        /// <summary>
        /// Obtains the name of the type definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the base type of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="BaseType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef GetBaseType() => null;

        /// <summary>
        /// Obtains the list of nested types that this type defines.
        /// </summary>
        /// <returns>The nested types.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="NestedTypes"/> property.
        /// </remarks>
        protected virtual IList<TypeDefinition> GetNestedTypes() =>
            new OwnedCollection<TypeDefinition, TypeDefinition>(this);

        /// <summary>
        /// Obtains the enclosing class of the type definition if available.
        /// </summary>
        /// <returns>The enclosing type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <summary>
        /// Obtains the collection of fields that this type defines.
        /// </summary>
        /// <returns>The fields.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Fields"/> property.
        /// </remarks>
        protected virtual IList<FieldDefinition> GetFields() =>
            new OwnedCollection<TypeDefinition, FieldDefinition>(this);

        /// <summary>
        /// Obtains the collection of methods that this type defines.
        /// </summary>
        /// <returns>The methods.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Methods"/> property.
        /// </remarks>
        protected virtual IList<MethodDefinition> GetMethods() =>
            new OwnedCollection<TypeDefinition, MethodDefinition>(this);


        /// <summary>
        /// Obtains the collection of properties that this type defines.
        /// </summary>
        /// <returns>The properties.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Properties"/> property.
        /// </remarks>
        protected virtual IList<PropertyDefinition> GetProperties() =>
            new OwnedCollection<TypeDefinition, PropertyDefinition>(this);

        /// <summary>
        /// Obtains the collection of events that this type defines.
        /// </summary>
        /// <returns>The events.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Events"/> property.
        /// </remarks>
        protected virtual IList<EventDefinition> GetEvents() =>
            new OwnedCollection<TypeDefinition, EventDefinition>(this);

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <summary>
        /// Obtains the list of security declarations assigned to the member.
        /// </summary>
        /// <returns>The security declarations</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="SecurityDeclarations"/> property.
        /// </remarks>
        protected virtual IList<SecurityDeclaration> GetSecurityDeclarations() =>
            new OwnedCollection<IHasSecurityDeclaration, SecurityDeclaration>(this);

        /// <summary>
        /// Obtains the list of generic parameters this member declares.
        /// </summary>
        /// <returns>The generic parameters</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="GenericParameters"/> property.
        /// </remarks>
        protected virtual IList<GenericParameter> GetGenericParameters() =>
            new OwnedCollection<IHasGenericParameters, GenericParameter>(this);

        /// <summary>
        /// Obtains the list of interfaces this type implements.
        /// </summary>
        /// <returns>The interfaces.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Interfaces"/> property.
        /// </remarks>
        protected virtual IList<InterfaceImplementation> GetInterfaces() =>
            new OwnedCollection<TypeDefinition, InterfaceImplementation>(this);

        /// <summary>
        /// Obtains the list of methods this type implements.
        /// </summary>
        /// <returns>The method implementations.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="MethodImplementations"/> property.
        /// </remarks>
        protected virtual IList<MethodImplementation> GetMethodImplementations() =>
            new List<MethodImplementation>();

        /// <summary>
        /// Obtains the class layout of this type.
        /// </summary>
        /// <returns>The class layout.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ClassLayout"/> property.
        /// </remarks>
        protected virtual ClassLayout GetClassLayout() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}