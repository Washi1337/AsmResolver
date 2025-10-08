using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single property in a type definition of a .NET module.
    /// </summary>
    public class PropertyDefinition :
        MetadataMember,
        IHasSemantics,
        IHasCustomAttribute,
        IHasConstant,
        IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<PropertyDefinition, Utf8String?> _name;
        private readonly LazyVariable<PropertyDefinition, TypeDefinition?> _declaringType;
        private readonly LazyVariable<PropertyDefinition, PropertySignature?> _signature;
        private readonly LazyVariable<PropertyDefinition, Constant?> _constant;
        private IList<MethodSemantics>? _semantics;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected PropertyDefinition(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<PropertyDefinition, Utf8String?>(x => x.GetName());
            _signature = new LazyVariable<PropertyDefinition, PropertySignature?>(x => x.GetSignature());
            _declaringType = new LazyVariable<PropertyDefinition, TypeDefinition?>(x => x.GetDeclaringType());
            _constant = new LazyVariable<PropertyDefinition, Constant?>(x => x.GetConstant());
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the property.</param>
        public PropertyDefinition(Utf8String? name, PropertyAttributes attributes, PropertySignature? signature)
            : this(new MetadataToken(TableIndex.Property,0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the property uses a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & PropertyAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.SpecialName)
                                | (value ? PropertyAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the property uses a special name used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & PropertyAttributes.RuntimeSpecialName) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.RuntimeSpecialName)
                                | (value ? PropertyAttributes.RuntimeSpecialName : 0);
        }


        /// <summary>
        /// Gets or sets a value indicating the property has a default value.
        /// </summary>
        public bool HasDefault
        {
            get => (Attributes & PropertyAttributes.HasDefault) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.HasDefault)
                                | (value ? PropertyAttributes.HasDefault : 0);
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the property definition table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetPropertyFullName(this);

        /// <summary>
        /// Gets or sets the signature of the property. This includes the property type, as well as any parameters the
        /// property might define.
        /// </summary>
        public PropertySignature? Signature
        {
            get => _signature.GetValue(this);
            set => _signature.SetValue(value);
        }

        /// <inheritdoc />
        public ModuleDefinition? DeclaringModule => DeclaringType?.DeclaringModule;

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        public TypeDefinition? DeclaringType
        {
            get => _declaringType.GetValue(this);
            private set => _declaringType.SetValue(value);
        }

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition? IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <inheritdoc />
        public IList<MethodSemantics> Semantics
        {
            get
            {
                if (_semantics is null)
                    Interlocked.CompareExchange(ref _semantics, GetSemantics(), null);
                return _semantics;
            }
        }

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributes.Count > 0;

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
        public Constant? Constant
        {
            get => _constant.GetValue(this);
            set => _constant.SetValue(value);
        }

        /// <summary>
        /// Gets the method definition representing the first get accessor of this property definition.
        /// </summary>
        public MethodDefinition? GetMethod
        {
            get => Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Getter)?.Method;
            set => SetSemanticMethods(value, SetMethod);
        }

        /// <summary>
        /// Gets the method definition representing the first set accessor of this property definition.
        /// </summary>
        public MethodDefinition? SetMethod
        {
            get => Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Setter)?.Method;
            set => SetSemanticMethods(GetMethod, value);
        }

        /// <summary>
        /// Clear <see cref="Semantics"/> and apply these methods to the property definition.
        /// </summary>
        /// <param name="getMethod">The method definition representing the get accessor of this property definition.</param>
        /// <param name="setMethod">The method definition representing the set accessor of this property definition.</param>
        public void SetSemanticMethods(MethodDefinition? getMethod, MethodDefinition? setMethod)
        {
            Semantics.Clear();
            if (getMethod is not null)
                Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
            if (setMethod is not null)
                Semantics.Add(new MethodSemantics(setMethod, MethodSemanticsAttributes.Setter));
        }

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type) =>
            Semantics.Any(s => s.Method?.IsAccessibleFromType(type) ?? false);

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        IMemberDefinition IMemberDescriptor.Resolve(ModuleDefinition context) => this;

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            return DeclaringModule == module && (Signature?.IsImportedInModule(module) ?? false);
        }

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => throw new NotSupportedException();

        /// <summary>
        /// Obtains the name of the property definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the signature of the property definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual PropertySignature? GetSignature() => null;

        /// <summary>
        /// Obtains the declaring type of the property definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition? GetDeclaringType() => null;

        /// <summary>
        /// Obtains the methods associated to this property definition.
        /// </summary>
        /// <returns>The method semantic objects.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Semantics"/> property.
        /// </remarks>
        protected virtual IList<MethodSemantics> GetSemantics() =>
            new MethodSemanticsCollection(this);

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
        /// Obtains the constant value assigned to the property definition.
        /// </summary>
        /// <returns>The constant.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constant"/> property.
        /// </remarks>
        protected virtual Constant? GetConstant() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
