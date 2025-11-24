using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an object that constrains a generic parameter to only be instantiated with a specific type.
    /// </summary>
    public partial class GenericParameterConstraint :
        MetadataMember,
        IHasCustomAttribute,
        IModuleProvider,
        IOwnedCollectionElement<GenericParameter>
    {
        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes the generic parameter constraint with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected GenericParameterConstraint(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new constraint for a generic parameter.
        /// </summary>
        /// <param name="constraint">The type to constrain the generic parameter to.</param>
        public GenericParameterConstraint(ITypeDefOrRef? constraint)
            : this(new MetadataToken(TableIndex.GenericParamConstraint, 0))
        {
            Constraint = constraint;
        }

        /// <summary>
        /// Gets the generic parameter that was constrained.
        /// </summary>
        [LazyProperty]
        public partial GenericParameter? Owner
        {
            get;
            private set;
        }

        /// <inheritdoc />
        GenericParameter? IOwnedCollectionElement<GenericParameter>.Owner
        {
            get => Owner;
            set => Owner = value;
        }

        /// <summary>
        /// Gets or sets the type that the generic parameter was constrained to.
        /// </summary>
        [LazyProperty]
        public partial ITypeDefOrRef? Constraint
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ModuleDefinition? DeclaringModule => Owner?.DeclaringModule;

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributesInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get{
                if (CustomAttributesInternal is null)
                    Interlocked.CompareExchange(ref CustomAttributesInternal, GetCustomAttributes(), null);
                return CustomAttributesInternal;
            }
        }

        /// <summary>
        /// Obtains the generic parameter that was constrained.
        /// </summary>
        /// <returns>The generic parameter</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Owner"/> property.
        /// </remarks>
        protected virtual GenericParameter? GetOwner() => null;

        /// <summary>
        /// Obtains the type that the generic parameter was constrained to.
        /// </summary>
        /// <returns>The type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constraint"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef? GetConstraint() => null;

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);
    }
}
