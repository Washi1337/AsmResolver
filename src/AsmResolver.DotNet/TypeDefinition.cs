using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.Shims;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type (a class, interface or structure) defined in a .NET module.
    /// </summary>
    public partial class TypeDefinition :
        MetadataMember,
        ITypeDefOrRef,
        IHasGenericParameters,
        IHasSecurityDeclaration,
        IOwnedCollectionElement<ITypeOwner>,
        ITypeOwner
    {
        internal static readonly Utf8String ModuleTypeName = "<Module>";

        private readonly LazyVariable<TypeDefinition, Utf8String?> _namespace;
        private ModuleDefinition? _module;

        /// <summary> The internal fields list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="Fields"/> instead.</remarks>
        protected IList<FieldDefinition>? FieldsInternal;

        /// <summary> The internal methods list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="Methods"/> instead.</remarks>
        protected IList<MethodDefinition>? MethodsInternal;

        /// <summary> The internal properties list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="Properties"/> instead.</remarks>
        protected IList<PropertyDefinition>? PropertiesInternal;

        /// <summary> The internal events list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="Events"/> instead.</remarks>
        protected IList<EventDefinition>? EventsInternal;

        /// <summary> The internal security declarations list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="SecurityDeclarations"/> instead.</remarks>
        protected IList<SecurityDeclaration>? SecurityDeclarationsInternal;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="GenericParameters"/> instead.</remarks>
        protected IList<GenericParameter>? GenericParametersInternal;

        /// <summary> The internal interfaces list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="Interfaces"/> instead.</remarks>
        protected IList<InterfaceImplementation>? InterfacesInternal;

        /// <summary> The internal method implementations list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="MethodImplementations"/> instead.</remarks>
        protected IList<MethodImplementation>? MethodImplementationsInternal;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary> The internal nested types list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="NestedTypes"/> instead.</remarks>
        protected IList<TypeDefinition>? NestedTypesInternal;

        /// <summary>
        /// Initializes a new type definition.
        /// </summary>
        /// <param name="token">The token of the type definition.</param>
        protected TypeDefinition(MetadataToken token)
            : base(token)
        {
            _namespace = new LazyVariable<TypeDefinition, Utf8String?>(x => x.GetNamespace());

        }

        /// <summary>
        /// Creates a new type definition.
        /// </summary>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="attributes">The attributes associated to the type.</param>
        public TypeDefinition(Utf8String? ns, Utf8String? name, TypeAttributes attributes)
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
        public TypeDefinition(Utf8String? ns, Utf8String? name, TypeAttributes attributes, ITypeDefOrRef? baseType)
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
        /// <remarks>
        /// This property corresponds to the Namespace column in the type definition table.
        /// </remarks>
        public Utf8String? Namespace
        {
            get => _namespace.GetValue(this);
            set => _namespace.SetValue(Utf8String.IsNullOrEmpty(value) ? null : value);
            // According to the specification, the namespace should always be null or non-empty.
        }

        string? ITypeDescriptor.Namespace => Namespace;

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the type definition table.
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets the full name (including namespace or declaring type full name) of the type.
        /// </summary>
        public string FullName => MemberNameGenerator.GetTypeFullName(this);

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
        /// Gets or sets a value indicating whether the layout of the type is supplied via a
        /// <c>System.Runtime.InteropServices.ExtendedLayoutAttribute</c>
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/dotnet/runtime/blob/0b9d2ccbd5e2ddbcb95fb8d7126755d160b81f64/docs/design/specs/Ecma-335-Augments.md#extended-layout
        /// </remarks>
        public bool IsExtendedLayout
        {
            get => (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExtendedLayout;
            set => Attributes = (Attributes & ~TypeAttributes.LayoutMask)
                | (value ? TypeAttributes.ExtendedLayout : 0);
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
        [LazyProperty]
        public partial ITypeDefOrRef? BaseType
        {
            get;
            set;
        }

        ITypeOwner? IOwnedCollectionElement<ITypeOwner>.Owner
        {
            get => DeclaringType is not null ? DeclaringType : _module;
            set
            {
                switch (value)
                {
                    case ModuleDefinition module:
                        DeclaringType = null;
                        _module = module;
                        break;
                    case TypeDefinition type:
                        DeclaringType = type;
                        _module = null;
                        break;
                    case null:
                        DeclaringType = null;
                        _module = null;
                        break;
                    default:
                        throw new NotSupportedException("The Owner of a TypeDefinition must be a ModuleDefinition or another TypeDefinition");
                }
            }
        }

        IList<TypeDefinition> ITypeOwner.OwnedTypes => NestedTypes;

        /// <summary>
        /// Gets the module that defines the type.
        /// </summary>
        public ModuleDefinition? DeclaringModule => DeclaringType is not null ? DeclaringType.DeclaringModule : _module;

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        [LazyProperty]
        public partial TypeDefinition? DeclaringType
        {
            get;
            private set;
        }

        ITypeDefOrRef? ITypeDefOrRef.DeclaringType => DeclaringType;

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

        /// <summary>
        /// Gets a value indicating whether the type is enclosed by another type.
        /// </summary>
        [MemberNotNullWhen(true, nameof(DeclaringType))]
        public bool IsNested => DeclaringType != null;

        /// <summary>
        /// Gets a value indicating whether the type defines nested types.
        /// </summary>
        public virtual bool HasNestedTypes => NestedTypesInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of nested types that this type defines.
        /// </summary>
        public IList<TypeDefinition> NestedTypes
        {
            get
            {
                if (NestedTypesInternal is null)
                    Interlocked.CompareExchange(ref NestedTypesInternal, GetNestedTypes(), null);
                return NestedTypesInternal;
            }
        }

        IResolutionScope? ITypeDescriptor.Scope => GetDeclaringScope();

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(BaseType))]
        public bool IsValueType => !this.IsTypeOf("System", nameof(Enum)) && BaseType is { } && (BaseType.IsTypeOf("System", nameof(ValueType)) || IsEnum);

        /// <summary>
        /// Gets a value indicating whether the type defines an enumeration of discrete values.
        /// </summary>
        [MemberNotNullWhen(true, nameof(BaseType))]
        public bool IsEnum => BaseType?.IsTypeOf("System", nameof(Enum)) ?? false;

        /// <summary>
        /// Gets a value indicating whether the type describes a delegate referring to a method.
        /// </summary>
        [MemberNotNullWhen(true, nameof(BaseType))]
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
        /// <c>true</c> if this is the global (i.e., &lt;Module&gt;) type, otherwise <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the global (i.e., &lt;Module&gt;) type was not added or does not exist yet in the <see cref="DeclaringModule"/>,
        /// this will return <c>false</c>.
        /// </remarks>
        [MemberNotNullWhen(true, nameof(DeclaringModule))]
        public bool IsModuleType
        {
            get
            {
                var module = DeclaringModule?.GetModuleType();
                return module != null && module == this;
            }
        }

        /// <summary>
        /// Determines whether the type is marked as read-only.
        /// </summary>
        [MemberNotNullWhen(true, nameof(BaseType))]
        public bool IsReadOnly =>
            IsValueType
            && this.HasCustomAttribute("System.Runtime.CompilerServices", nameof(ReadOnlyAttribute));

        /// <summary>
        /// Determines whether the type is marked with the IsByRefLike attribute, indicating a ref struct definition.
        /// </summary>
        [MemberNotNullWhen(true, nameof(BaseType))]
        public bool IsByRefLike =>
            IsValueType
            && this.HasCustomAttribute("System.Runtime.CompilerServices", "IsByRefLikeAttribute");

        /// <summary>
        /// Gets a value indicating whether the type defines fields.
        /// </summary>
        public virtual bool HasFields => FieldsInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of fields defined in the type.
        /// </summary>
        public IList<FieldDefinition> Fields
        {
            get
            {
                if (FieldsInternal is null)
                    Interlocked.CompareExchange(ref FieldsInternal, GetFields(), null);
                return FieldsInternal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type defines methods.
        /// </summary>
        public virtual bool HasMethods => MethodsInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of methods defined in the type.
        /// </summary>
        public IList<MethodDefinition> Methods
        {
            get
            {
                if (MethodsInternal is null)
                    Interlocked.CompareExchange(ref MethodsInternal, GetMethods(), null);
                return MethodsInternal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type defines properties.
        /// </summary>
        public virtual bool HasProperties => PropertiesInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of properties defined in the type.
        /// </summary>
        public IList<PropertyDefinition> Properties
        {
            get
            {
                if (PropertiesInternal is null)
                    Interlocked.CompareExchange(ref PropertiesInternal, GetProperties(), null);
                return PropertiesInternal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type defines events.
        /// </summary>
        public virtual bool HasEvents => EventsInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of events defined in the type.
        /// </summary>
        public IList<EventDefinition> Events
        {
            get
            {
                if (EventsInternal is null)
                    Interlocked.CompareExchange(ref EventsInternal, GetEvents(), null);
                return EventsInternal;
            }
        }

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributesInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (CustomAttributesInternal is null)
                    Interlocked.CompareExchange(ref CustomAttributesInternal, GetCustomAttributes(), null);
                return CustomAttributesInternal;
            }
        }

        /// <inheritdoc />
        public virtual bool HasSecurityDeclarations => SecurityDeclarationsInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<SecurityDeclaration> SecurityDeclarations
        {
            get
            {
                if (SecurityDeclarationsInternal is null)
                    Interlocked.CompareExchange(ref SecurityDeclarationsInternal, GetSecurityDeclarations(), null);
                return SecurityDeclarationsInternal;
            }
        }

        /// <inheritdoc />
        public virtual bool HasGenericParameters => GenericParametersInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<GenericParameter> GenericParameters
        {
            get
            {
                if (GenericParametersInternal is null)
                    Interlocked.CompareExchange(ref GenericParametersInternal, GetGenericParameters(), null);
                return GenericParametersInternal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type implements interfaces.
        /// </summary>
        public virtual bool HasInterfaces => InterfacesInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of interfaces that are implemented by the type.
        /// </summary>
        public IList<InterfaceImplementation> Interfaces
        {
            get
            {
                if (InterfacesInternal is null)
                    Interlocked.CompareExchange(ref InterfacesInternal, GetInterfaces(), null);
                return InterfacesInternal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type explicitly implements interface methods.
        /// </summary>
        public virtual bool HasMethodImplementations => MethodImplementationsInternal is { Count: > 0 };

        /// <summary>
        /// Gets a collection of methods that are explicitly implemented by the type.
        /// </summary>
        public IList<MethodImplementation> MethodImplementations
        {
            get
            {
                if (MethodImplementationsInternal is null)
                    Interlocked.CompareExchange(ref MethodImplementationsInternal, GetMethodImplementations(), null);
                return MethodImplementationsInternal;
            }
        }

        /// <summary>
        /// Gets or sets an override to the layout of a class, indicating its total and packing size.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, the runtime decides the layout of the class.
        /// </remarks>
        [LazyProperty]
        public partial ClassLayout? ClassLayout
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether the type inherits from a particular type.
        /// </summary>
        /// <param name="fullName">The full name of the type</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> inherits from the type,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool InheritsFrom(string fullName) => FindInTypeTree(x => x.FullName == fullName);

        /// <summary>
        /// Determines whether the type inherits from a particular type.
        /// </summary>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> inherits from the type,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool InheritsFrom(string? ns, string name) => FindInTypeTree(x => x.IsTypeOf(ns, name));

        /// <summary>
        /// Determines whether the type inherits from a particular type.
        /// </summary>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> inherits from the type,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool InheritsFrom(Utf8String? ns, Utf8String name) => FindInTypeTree(x => x.IsTypeOfUtf8(ns, name));

        /// <summary>
        /// Determines whether the type implements a particular interface.
        /// </summary>
        /// <param name="fullName">The full name of the interface</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> implements the interface,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool Implements(string fullName)
        {
            return FindInTypeTree(x => x.Interfaces.Any(@interface => @interface.Interface?.FullName == fullName));
        }

        /// <summary>
        /// Determines whether the type implements a particular interface.
        /// </summary>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> implements the interface,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool Implements(string? ns, string name) => FindInTypeTree(
            x => x.Interfaces.Any(@interface => @interface.Interface?.IsTypeOf(ns, name) ?? false));

        /// <summary>
        /// Determines whether the type implements a particular interface.
        /// </summary>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>
        /// <c>true</c> whether the current <see cref="TypeDefinition"/> implements the interface,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool Implements(Utf8String? ns, Utf8String name) => FindInTypeTree(
            x => x.Interfaces.Any(@interface => @interface.Interface?.IsTypeOfUtf8(ns, name) ?? false));

        private bool FindInTypeTree(Predicate<TypeDefinition> condition)
        {
            var visited = new List<TypeDefinition>();

            var type = this;
            do
            {
                // Protect against malicious cyclic dependency graphs.
                if (visited.Contains(type))
                    return false;

                if (condition(type))
                    return true;

                visited.Add(type);
                type = type.BaseType?.Resolve();
            } while (type is not null);

            return false;
        }

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() => ToTypeSignature(IsValueType);

        /// <inheritdoc />
        public TypeSignature ToTypeSignature(bool isValueType)
        {
            return DeclaringModule?.CorLibTypeFactory.FromType(this) as TypeSignature
                   ?? new TypeDefOrRefSignature(this, isValueType);
        }

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) => DeclaringModule == module;

        /// <summary>
        /// Imports the type definition using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported type.</returns>
        public ITypeDefOrRef ImportWith(ReferenceImporter importer) => importer.ImportType(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <summary>
        /// Determines whether the provided definition can be accessed by the type.
        /// </summary>
        /// <param name="definition">The definition to access.</param>
        /// <returns><c>true</c> if this type can access <paramref name="definition"/>, <c>false</c> otherwise.</returns>
        public bool CanAccessDefinition(IMemberDefinition definition)
        {
            return definition.IsAccessibleFromType(this);
        }

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type)
        {
            if (SignatureComparer.Default.Equals(this, type))
                return true;

            bool isInSameAssembly = SignatureComparer.Default.Equals(DeclaringModule, type.DeclaringModule);

            // Most common case: A top-level types is accessible by all other types in the same assembly, or types in
            // a different assembly if this top-level type is public.
            if (DeclaringType is not { } declaringType)
                return IsPublic || isInSameAssembly;

            // The current type is a nested type, which means in order to be accessible, `type` needs to be able to
            // access the declaring type first before it can reach the current type.
            if (!declaringType.IsAccessibleFromType(type))
                return false;

            // Types can always access their direct nested types.
            if (SignatureComparer.Default.Equals(declaringType, type))
                return true;

            // Assuming declaring type is accessible, then public nested types are always accessible.
            if (IsNestedPublic)
                return true;

            // If the current type is marked assembly (internal in C#), `type` must be in the same assembly.
            if (IsNestedAssembly || IsNestedFamilyOrAssembly)
                return isInSameAssembly;

            // If the current type is marked family (protected in C#), `type` must be in the type hierarchy.
            //
            //      class A
            //      {
            //          protected class B {} // <-- `this`
            //      }
            //
            //      class C : A {} // <-- `type` ( can access A+B )
            //
            if ((IsNestedFamily || IsNestedFamilyOrAssembly || IsNestedFamilyAndAssembly)
                && type.BaseType?.Resolve() is { } baseType)
            {
                return (!IsNestedFamilyAndAssembly || isInSameAssembly) && IsAccessibleFromType(baseType);
            }

            return false;
        }

        /// <summary>
        /// Creates a new type reference to this type definition.
        /// </summary>
        /// <returns>The type reference.</returns>
        public TypeReference ToTypeReference()
        {
            var scope = DeclaringType?.ToTypeReference() ?? DeclaringModule as IResolutionScope;

            return new TypeReference(DeclaringModule, scope, Namespace, Name);
        }

        private IResolutionScope? GetDeclaringScope()
        {
            if (DeclaringType is null)
                return DeclaringModule;

            return DeclaringType.ToTypeReference();
        }

        TypeDefinition ITypeDescriptor.Resolve() => this;
        TypeDefinition ITypeDescriptor.Resolve(ModuleDefinition context) => this;
        IMemberDefinition IMemberDescriptor.Resolve() => this;
        IMemberDefinition? IMemberDescriptor.Resolve(ModuleDefinition context) => this;

        /// <summary>
        /// When this type is an enum, extracts the underlying enum type.
        /// </summary>
        /// <returns>The type, or <c>null</c> if none was found.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the type is not an enum.</exception>
        /// <remarks>
        /// To verify whether a type is an enum or not, use the <see cref="IsEnum"/> property.
        /// </remarks>
        public TypeSignature? GetEnumUnderlyingType()
        {
            if (!IsEnum)
                throw new InvalidOperationException("Type is not an enum.");

            foreach (var field in Fields)
            {
                if (field is { IsLiteral: false, IsStatic: false, Signature: not null })
                    return field.Signature.FieldType;
            }

            return null;
        }

        /// <summary>
        /// Finds the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetStaticConstructor()
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                if (Methods[i] is {IsConstructor: true, IsStatic: true, Parameters.Count: 0} method)
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Gets or creates the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        /// <remarks>
        /// If the static constructor was not present in the type, it will be inserted as the first method in the type.
        /// This method can only be used when the type has already been added to the metadata image.
        /// </remarks>
        public MethodDefinition GetOrCreateStaticConstructor() => GetOrCreateStaticConstructor(DeclaringModule);

        /// <summary>
        /// Gets or creates the static constructor that is executed when the CLR loads this type.
        /// </summary>
        /// <param name="module">The image to use for creating the signature of the constructor if it is not present yet.</param>
        /// <returns>The static constructor, or <c>null</c> if none is present.</returns>
        /// <remarks>
        /// If the static constructor was not present in the type, it will be inserted as the first method in the type.
        /// </remarks>
        public MethodDefinition GetOrCreateStaticConstructor(ModuleDefinition? module)
        {
            var cctor = GetStaticConstructor();
            if (cctor == null)
            {
                if (module == null)
                    throw new ArgumentNullException(nameof(module));

                cctor = MethodDefinition.CreateStaticConstructor(module);
                Methods.Insert(0, cctor);
            }

            return cctor;
        }

        /// <summary>
        /// Finds the instance parameterless constructor this type defines.
        /// </summary>
        /// <returns>The constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetConstructor()
        {
            return GetConstructor(SignatureComparer.Default, (IList<TypeSignature>) ArrayShim.Empty<TypeSignature>());
        }

        /// <summary>
        /// Finds the instance constructor with the provided parameter types this type defines.
        /// </summary>
        /// <param name="parameterTypes">An ordered list of types the parameters of the constructor should have.</param>
        /// <returns>The constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetConstructor(params TypeSignature[] parameterTypes)
        {
            return GetConstructor(SignatureComparer.Default, parameterTypes);
        }

        /// <summary>
        /// Finds the instance constructor with the provided parameter types this type defines.
        /// </summary>
        /// <param name="comparer">The signature comparer to use when comparing the parameter types.</param>
        /// <param name="parameterTypes">An ordered list of types the parameters of the constructor should have.</param>
        /// <returns>The constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetConstructor(SignatureComparer comparer, params TypeSignature[] parameterTypes)
        {
            return GetConstructor(comparer, (IList<TypeSignature>) parameterTypes);
        }

        /// <summary>
        /// Finds the instance constructor with the provided parameter types this type defines.
        /// </summary>
        /// <param name="comparer">The signature comparer to use when comparing the parameter types.</param>
        /// <param name="parameterTypes">An ordered list of types the parameters of the constructor should have.</param>
        /// <returns>The constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetConstructor(SignatureComparer comparer, IList<TypeSignature> parameterTypes)
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                if (Methods[i] is not {IsConstructor: true, IsStatic: false} method)
                    continue;

                if (method.Parameters.Count != parameterTypes.Count)
                    continue;

                bool fullMatch = true;
                for (int j = 0; j < method.Parameters.Count && fullMatch; j++)
                {
                    if (!comparer.Equals(method.Parameters[j].ParameterType, parameterTypes[j]))
                        fullMatch = false;
                }

                if (fullMatch)
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Obtains the namespace of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual Utf8String? GetNamespace() => null;

        /// <summary>
        /// Obtains the name of the type definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the base type of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="BaseType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef? GetBaseType() => null;

        /// <summary>
        /// Obtains the list of nested types that this type defines.
        /// </summary>
        /// <returns>The nested types.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="NestedTypes"/> property.
        /// </remarks>
        protected virtual IList<TypeDefinition> GetNestedTypes() =>
            new OwnedCollection<ITypeOwner, TypeDefinition>(this);

        /// <summary>
        /// Obtains the enclosing class of the type definition if available.
        /// </summary>
        /// <returns>The enclosing type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition? GetDeclaringType() => null;

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
        protected virtual ClassLayout? GetClassLayout() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
