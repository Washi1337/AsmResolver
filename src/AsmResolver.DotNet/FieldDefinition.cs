using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single field in a type definition of a .NET module.
    /// </summary>
    public class FieldDefinition :
        MetadataMember,
        IMemberDefinition,
        IFieldDescriptor,
        IHasCustomAttribute,
        IHasConstant,
        IMemberForwarded,
        IHasFieldMarshal,
        IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<FieldDefinition, Utf8String?> _name;
        private readonly LazyVariable<FieldDefinition, FieldSignature?> _signature;
        private readonly LazyVariable<FieldDefinition, TypeDefinition?> _declaringType;
        private readonly LazyVariable<FieldDefinition, Constant?> _constant;
        private readonly LazyVariable<FieldDefinition, MarshalDescriptor?> _marshalDescriptor;
        private readonly LazyVariable<FieldDefinition, ImplementationMap?> _implementationMap;
        private readonly LazyVariable<FieldDefinition, ISegment?> _fieldRva;
        private readonly LazyVariable<FieldDefinition, int?> _fieldOffset;

        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes a new field definition.
        /// </summary>
        /// <param name="token">The token of the field.</param>
        protected FieldDefinition(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<FieldDefinition, Utf8String?>(x => x.GetName());
            _signature = new LazyVariable<FieldDefinition, FieldSignature?>(x => x.GetSignature());
            _declaringType = new LazyVariable<FieldDefinition, TypeDefinition?>(x => x.GetDeclaringType());
            _constant = new LazyVariable<FieldDefinition, Constant?>(x => x.GetConstant());
            _marshalDescriptor = new LazyVariable<FieldDefinition, MarshalDescriptor?>(x => x.GetMarshalDescriptor());
            _implementationMap = new LazyVariable<FieldDefinition, ImplementationMap?>(x => x.GetImplementationMap());
            _fieldRva = new LazyVariable<FieldDefinition, ISegment?>(x => x.GetFieldRva());
            _fieldOffset = new LazyVariable<FieldDefinition, int?>(x => x.GetFieldOffset());
        }

        /// <summary>
        /// Creates a new field definition.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the field.</param>
        public FieldDefinition(Utf8String? name, FieldAttributes attributes, FieldSignature? signature)
            : this(new MetadataToken(TableIndex.Field, 0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <summary>
        /// Creates a new field definition.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="fieldType">The type of values the field contains.</param>
        public FieldDefinition(Utf8String name, FieldAttributes attributes, TypeSignature? fieldType)
            : this(new MetadataToken(TableIndex.Field, 0))
        {
            Name = name;
            Attributes = attributes;
            Signature = fieldType is not null
                ? new FieldSignature(fieldType)
                : null;
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the field table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the signature of the field. This includes the field type.
        /// </summary>
        public FieldSignature? Signature
        {
            get => _signature.GetValue(this);
            set => _signature.SetValue(value);
        }

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetFieldFullName(this);

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public FieldAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is in a private scope.
        /// </summary>
        public bool IsPrivateScope
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.PrivateScope;
            set => Attributes = value ? Attributes & ~FieldAttributes.FieldAccessMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same enclosing type.
        /// </summary>
        public bool IsPrivate
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Private : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked family and assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, within the same assembly.
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamilyAndAssembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.FamilyAndAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same assembly.
        /// </summary>
        public bool IsAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Assembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same enclosing type, as well as any derived type.
        /// </summary>
        public bool IsFamily
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Family : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked family or assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, or within the same assembly.
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamilyOrAssembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.FamilyOrAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked public, and can be accessed by
        /// any member having access to the enclosing type.
        /// </summary>
        public bool IsPublic
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Public : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field requires an object instance to access it.
        /// </summary>
        /// <remarks>
        /// This property does not reflect the value of <see cref="CallingConventionSignature.HasThis"/>, nor will it
        /// change the value of <see cref="CallingConventionSignature.HasThis"/> if this property is changed. For a
        /// valid .NET image, these values should match, however.
        /// </remarks>
        public bool IsStatic
        {
            get => (Attributes & FieldAttributes.Static) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.Static)
                                | (value ? FieldAttributes.Static : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked init-only, and can only be assigned a value by
        /// a constructor of the enclosing type.
        /// </summary>
        public bool IsInitOnly
        {
            get => (Attributes & FieldAttributes.InitOnly) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.InitOnly)
                                | (value ? FieldAttributes.InitOnly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked as a literal, and its value is decided upon
        /// compile time.
        /// </summary>
        public bool IsLiteral
        {
            get => (Attributes & FieldAttributes.Literal) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.Literal)
                                | (value ? FieldAttributes.Literal : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked as not serialized, indicating the field does
        /// not have to be serialized when the enclosing type is remoted.
        /// </summary>
        public bool IsNotSerialized
        {
            get => (Attributes & FieldAttributes.NotSerialized) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.NotSerialized)
                                | (value ? FieldAttributes.NotSerialized : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field uses a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & FieldAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.SpecialName)
                                | (value ? FieldAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field's implementation is forwarded through Platform Invoke.
        /// </summary>
        public bool IsPInvokeImpl
        {
            get => (Attributes & FieldAttributes.PInvokeImpl) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.PInvokeImpl)
                                | (value ? FieldAttributes.PInvokeImpl : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field uses a name that is used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & FieldAttributes.RuntimeSpecialName) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.RuntimeSpecialName)
                                | (value ? FieldAttributes.RuntimeSpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has marshalling information associated to it.
        /// </summary>
        public bool HasFieldMarshal
        {
            get => (Attributes & FieldAttributes.HasFieldMarshal) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasFieldMarshal)
                                | (value ? FieldAttributes.HasFieldMarshal : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a default value associated to it.
        /// </summary>
        public bool HasDefault
        {
            get => (Attributes & FieldAttributes.HasDefault) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasDefault)
                                | (value ? FieldAttributes.HasDefault : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has an initial value associated to it that is referenced
        /// by a relative virtual address.
        /// </summary>
        public bool HasFieldRva
        {
            get => (Attributes & FieldAttributes.HasFieldRva) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasFieldRva)
                                | (value ? FieldAttributes.HasFieldRva : 0);
        }

        /// <inheritdoc />
        public ModuleDefinition? Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the field.
        /// </summary>
        public TypeDefinition? DeclaringType
        {
            get => _declaringType.GetValue(this);
            private set => _declaringType.SetValue(value);
        }

        TypeDefinition? IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

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
        public Constant? Constant
        {
            get => _constant.GetValue(this);
            set => _constant.SetValue(value);
        }

        /// <inheritdoc />
        public MarshalDescriptor? MarshalDescriptor
        {
            get => _marshalDescriptor.GetValue(this);
            set => _marshalDescriptor.SetValue(value);
        }

        /// <inheritdoc />
        public ImplementationMap? ImplementationMap
        {
            get => _implementationMap.GetValue(this);
            set
            {
                if (value?.MemberForwarded is not null)
                    throw new ArgumentException("Cannot add an implementation map that was already added to another member.");
                if (_implementationMap.GetValue(this) is { } map)
                    map.MemberForwarded = null;
                _implementationMap.SetValue(value);
                if (value is not null)
                    value.MemberForwarded = this;
            }
        }

        /// <summary>
        /// Gets or sets a segment containing the initial value of the field.
        /// </summary>
        /// <remarks>
        /// Updating this property does not automatically update the <see cref="HasFieldRva"/> property, nor does the
        /// value of <see cref="HasFieldRva"/> reflect whether the field has initialization data or not. Well-formed
        /// .NET binaries should always set the <see cref="HasFieldRva"/> flag to <c>true</c> if this property is non-null.
        /// </remarks>
        public ISegment? FieldRva
        {
            get => _fieldRva.GetValue(this);
            set => _fieldRva.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the explicit offset of the field, relative to the starting address of the object (if available).
        /// </summary>
        public int? FieldOffset
        {
            get => _fieldOffset.GetValue(this);
            set => _fieldOffset.SetValue(value);
        }

        FieldDefinition IFieldDescriptor.Resolve() => this;

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            return Module == module
                   && (Signature?.IsImportedInModule(module) ?? false);
        }

        /// <summary>
        /// Imports the field using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported field.</returns>
        public IFieldDescriptor ImportWith(ReferenceImporter importer) => importer.ImportField(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type)
        {
            // The field is only accessible if its declaring type is accessible.
            if (DeclaringType is not { } declaringType || !declaringType.IsAccessibleFromType(type))
                return false;

            // Public fields are always accessible.
            if (IsPublic)
                return true;

            // Types can always access their own fields and any fields in their declaring types.
            var type1 = type;
            while (type1 is not null)
            {
                if (SignatureComparer.Default.Equals(declaringType, type1))
                    return true;
                type1 = type1.DeclaringType;
            }

            bool isInSameAssembly = SignatureComparer.Default.Equals(declaringType.Module, type.Module);

            // Assembly (internal in C#) fields are accessible by types in the same assembly.
            if (IsAssembly || IsFamilyOrAssembly)
                return isInSameAssembly;

            // Family (protected in C#) fields are accessible by any base type.
            if ((IsFamily || IsFamilyOrAssembly || IsFamilyAndAssembly)
                && type.BaseType?.Resolve() is { } baseType)
            {
                return (!IsFamilyAndAssembly || isInSameAssembly) && IsAccessibleFromType(baseType);
            }

            return false;
        }

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
        /// Obtains the name of the field definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the signature of the field definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual FieldSignature? GetSignature() => null;

        /// <summary>
        /// Obtains the declaring type of the field definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition? GetDeclaringType() => null;

        /// <summary>
        /// Obtains the constant value assigned to the field definition.
        /// </summary>
        /// <returns>The constant.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constant"/> property.
        /// </remarks>
        protected virtual Constant? GetConstant() => null;

        /// <summary>
        /// Obtains the marshal descriptor value assigned to the field definition.
        /// </summary>
        /// <returns>The marshal descriptor.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="MarshalDescriptor"/> property.
        /// </remarks>
        protected virtual MarshalDescriptor? GetMarshalDescriptor() => null;

        /// <summary>
        /// Obtains the platform invoke information assigned to the field.
        /// </summary>
        /// <returns>The mapping.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ImplementationMap"/> property.
        /// </remarks>
        protected virtual ImplementationMap? GetImplementationMap() => null;

        /// <summary>
        /// Obtains the initial value of the field.
        /// </summary>
        /// <returns>The initial value.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="FieldRva"/> property.
        /// </remarks>
        protected virtual ISegment? GetFieldRva() => null;

        /// <summary>
        /// Obtains the offset of the field as defined in the field layout.
        /// </summary>
        /// <returns>The field offset.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="FieldOffset"/> property.
        /// </remarks>
        protected virtual int? GetFieldOffset() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
