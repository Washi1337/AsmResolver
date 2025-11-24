using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents extra metadata added to a type indicating the type is implementing a particular interface.
    /// </summary>
    public partial class InterfaceImplementation :
        MetadataMember,
        IModuleProvider,
        IOwnedCollectionElement<TypeDefinition>,
        IHasCustomAttribute
    {
        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes the <see cref="InterfaceImplementation"/> object with a metadata token.
        /// </summary>
        /// <param name="token"></param>
        protected InterfaceImplementation(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InterfaceImplementation"/> class.
        /// </summary>
        /// <param name="interfaceType">The interface to be implemented.</param>
        public InterfaceImplementation(ITypeDefOrRef? interfaceType)
            : this(new MetadataToken(TableIndex.InterfaceImpl, 0))
        {
            Interface = interfaceType;
        }

        /// <summary>
        /// Gets the type that implements the interface.
        /// </summary>
        [LazyProperty]
        public partial TypeDefinition? Class
        {
            get;
            private set;
        }

        /// <inheritdoc />
        TypeDefinition? IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => Class;
            set => Class = value;
        }

        /// <summary>
        /// Gets or sets the interface type that was implemented.
        /// </summary>
        [LazyProperty]
        public partial ITypeDefOrRef? Interface
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ModuleDefinition? DeclaringModule => Class?.DeclaringModule;

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

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

        /// <summary>
        /// Obtains the type that implements the interface.
        /// </summary>
        /// <returns>The type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Class"/> property.
        /// </remarks>
        protected virtual TypeDefinition? GetClass() => null;

        /// <summary>
        /// Obtains the interface that is implemented.
        /// </summary>
        /// <returns>The interface.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Interface"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef? GetInterface() => null;

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
