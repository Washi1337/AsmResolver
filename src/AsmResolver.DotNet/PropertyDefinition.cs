using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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
        private readonly LazyVariable<string?> _name;
        private readonly LazyVariable<TypeDefinition?> _declaringType;
        private readonly LazyVariable<PropertySignature?> _signature;
        private readonly LazyVariable<Constant?> _constant;
        private IList<MethodSemantics>? _semantics;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected PropertyDefinition(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<string?>(GetName);
            _signature = new LazyVariable<PropertySignature?>(GetSignature);
            _declaringType = new LazyVariable<TypeDefinition?>(GetDeclaringType);
            _constant = new LazyVariable<Constant?>(GetConstant);
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the property.</param>
        public PropertyDefinition(string name, PropertyAttributes attributes, PropertySignature signature)
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

        /// <inheritdoc />
        public string? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetPropertyFullName(Name, DeclaringType, Signature);

        /// <summary>
        /// Gets or sets the signature of the property. This includes the property type, as well as any parameters the
        /// property might define.
        /// </summary>
        public PropertySignature? Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public ModuleDefinition? Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        public TypeDefinition? DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
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
            get => _constant.Value;
            set => _constant.Value = value;
        }

        /// <summary>
        /// Gets the method definition representing the get accessor of this property definition.
        /// </summary>
        public MethodDefinition? GetMethod =>
            Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Getter)?.Method;

        /// <summary>
        /// Gets the method definition representing the set accessor of this property definition.
        /// </summary>
        public MethodDefinition? SetMethod =>
            Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Setter)?.Method;

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type) =>
            Semantics.Any(s => s.Method?.IsAccessibleFromType(type) ?? false);

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        /// <summary>
        /// Obtains the name of the property definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string? GetName() => null;

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
