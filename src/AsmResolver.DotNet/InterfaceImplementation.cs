using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents extra metadata added to a type indicating the type is implementing a particular interface.
    /// </summary>
    public class InterfaceImplementation :
        MetadataMember,
        IModuleProvider,
        IOwnedCollectionElement<TypeDefinition>,
        IHasCustomAttribute
    {
        private readonly LazyVariable<TypeDefinition?> _class;
        private readonly LazyVariable<ITypeDefOrRef?> _interface;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes the <see cref="InterfaceImplementation"/> object with a metadata token.
        /// </summary>
        /// <param name="token"></param>
        protected InterfaceImplementation(MetadataToken token)
            : base(token)
        {
            _class = new LazyVariable<TypeDefinition?>(GetClass);
            _interface = new LazyVariable<ITypeDefOrRef?>(GetInterface);
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
        public TypeDefinition? Class
        {
            get => _class.Value;
            private set => _class.Value = value;
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
        public ITypeDefOrRef? Interface
        {
            get => _interface.Value;
            set => _interface.Value = value;
        }

        /// <inheritdoc />
        public ModuleDefinition? Module => Class?.Module;

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
