using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type parameter that a generic method or type in a .NET module defines.
    /// </summary>
    public class GenericParameter :
        MetadataMember,
        INameProvider,
        IHasCustomAttribute,
        IModuleProvider,
        IOwnedCollectionElement<IHasGenericParameters>
    {
        private readonly LazyVariable<GenericParameter, Utf8String?> _name;
        private readonly LazyVariable<GenericParameter, IHasGenericParameters?> _owner;
        private IList<GenericParameterConstraint>? _constraints;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes a new empty generic parameter.
        /// </summary>
        /// <param name="token">The token of the generic parameter.</param>
        protected GenericParameter(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<GenericParameter, Utf8String?>(x => x.GetName());
            _owner = new LazyVariable<GenericParameter, IHasGenericParameters?>(x => x.GetOwner());
        }

        /// <summary>
        /// Creates a new generic parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public GenericParameter(Utf8String? name)
            : this(new MetadataToken(TableIndex.GenericParam, 0))
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new generic parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="attributes">Additional attributes to assign to the parameter.</param>
        public GenericParameter(Utf8String? name, GenericParameterAttributes attributes)
            : this(new MetadataToken(TableIndex.GenericParam, 0))
        {
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the member that defines this generic parameter.
        /// </summary>
        public IHasGenericParameters? Owner
        {
            get => _owner.GetValue(this);
            private set => _owner.SetValue(value);
        }

        IHasGenericParameters? IOwnedCollectionElement<IHasGenericParameters>.Owner
        {
            get => Owner;
            set => Owner = value;
        }

        /// <summary>
        /// Gets or sets the name of the generic parameter.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the generic parameter table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets additional attributes assigned to this generic parameter.
        /// </summary>
        public GenericParameterAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the variance of this generic parameter.
        /// </summary>
        public GenericParameterAttributes Variance
        {
            get => Attributes & GenericParameterAttributes.VarianceMask;
            set => Attributes = (Attributes & ~GenericParameterAttributes.VarianceMask)
                                | value & GenericParameterAttributes.VarianceMask;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this generic parameter is <see cref="GenericParameterAttributes.NonVariant"/>.
        /// </summary>
        public bool IsNonVariant
        {
            get => Variance == GenericParameterAttributes.NonVariant;
        }

        /// <summary>
        /// Gets a value indicating whether this generic parameter is <see cref="GenericParameterAttributes.Covariant"/>.
        /// </summary>
        public bool IsCovariant
        {
            get => Variance == GenericParameterAttributes.Covariant;
        }

        /// <summary>
        /// Gets a value indicating whether this generic parameter is <see cref="GenericParameterAttributes.Contravariant"/>.
        /// </summary>
        public bool IsContravariant
        {
            get => Variance == GenericParameterAttributes.Contravariant;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this generic parameter has a <see cref="GenericParameterAttributes.ReferenceTypeConstraint"/>.
        /// </summary>
        public bool HasReferenceTypeConstraint
        {
            get => (Attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            set => Attributes = (Attributes & ~GenericParameterAttributes.ReferenceTypeConstraint)
                                | (value ? GenericParameterAttributes.ReferenceTypeConstraint : default);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this generic parameter has a <see cref="GenericParameterAttributes.NotNullableValueTypeConstraint"/>.
        /// </summary>
        public bool HasNotNullableValueTypeConstraint
        {
            get => (Attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
            set => Attributes = (Attributes & ~GenericParameterAttributes.NotNullableValueTypeConstraint)
                                | (value ? GenericParameterAttributes.NotNullableValueTypeConstraint : default);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this generic parameter has a <see cref="GenericParameterAttributes.DefaultConstructorConstraint"/>.
        /// </summary>
        public bool HasDefaultConstructorConstraint
        {
            get => (Attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0;
            set => Attributes = (Attributes & ~GenericParameterAttributes.DefaultConstructorConstraint)
                                | (value ? GenericParameterAttributes.DefaultConstructorConstraint : default);
        }

        /// <summary>
        /// Gets the index of this parameter within the list of generic parameters that the owner defines.
        /// </summary>
        public ushort Number => Owner is null ? (ushort) 0 : (ushort) Owner.GenericParameters.IndexOf(this);

        /// <inheritdoc />
        public ModuleDefinition? Module => Owner?.Module;

        /// <summary>
        /// Gets a collection of constraints put on the generic parameter.
        /// </summary>
        public IList<GenericParameterConstraint> Constraints
        {
            get
            {
                if (_constraints is null)
                    Interlocked.CompareExchange(ref _constraints, GetConstraints(), null);
                return _constraints;
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

        /// <summary>
        /// Obtains the name of the generic parameter.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the owner of the generic parameter.
        /// </summary>
        /// <returns>The owner</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Owner"/> property.
        /// </remarks>
        protected virtual IHasGenericParameters? GetOwner() => null;

        /// <summary>
        /// Obtains a collection of constraints put on the generic parameter.
        /// </summary>
        /// <returns>The constraints</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constraints"/> property.
        /// </remarks>
        protected virtual IList<GenericParameterConstraint> GetConstraints() =>
            new OwnedCollection<GenericParameter, GenericParameterConstraint>(this);

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <inheritdoc />
        public override string ToString() => Name ?? NullName;
    }
}
