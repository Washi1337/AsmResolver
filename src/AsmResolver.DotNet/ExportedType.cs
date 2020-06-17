using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition that was exported to another external .NET module.
    /// </summary>
    public class ExportedType :
        IImplementation,
        ITypeDescriptor,
        IHasCustomAttribute,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<string> _namespace;
        private readonly LazyVariable<IImplementation> _implementation;
        private IList<CustomAttribute> _customAttributes;
        private MetadataToken _token;

        /// <summary>
        /// Initializes an exported type with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ExportedType(MetadataToken token)
        {
            _token = token;
            _name = new LazyVariable<string>(GetName);
            _namespace = new LazyVariable<string>(GetNamespace);
            _implementation = new LazyVariable<IImplementation>(GetImplementation);
        }

        /// <summary>
        /// Creates a new exported type reference. 
        /// </summary>
        /// <param name="implementation">The file containing the type.</param>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        public ExportedType(IImplementation implementation, string ns, string name)
            : this(new MetadataToken(TableIndex.ExportedType, 1))
        {
            Implementation = implementation;
            Namespace = ns;
            Name = name;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken => _token;

        MetadataToken IMetadataMember.MetadataToken
        {
            get => _token;
            set => _token = value;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the type.
        /// </summary>
        public TypeAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hint to the row identifier of the type definition in the external module.
        /// </summary>
        public uint TypeDefId
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string Namespace
        {
            get => _namespace.Value;
            set => _namespace.Value = value;
        }
        
        /// <inheritdoc />
        public string FullName => this.GetTypeFullName();

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
            private set;
        }

        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }

        /// <summary>
        /// Gets or sets the new location this type is exported to.
        /// </summary>
        public IImplementation Implementation
        {
            get => _implementation.Value;
            set => _implementation.Value = value;
        }

        /// <summary>
        /// When this exported type is nested, gets the enclosing type.
        /// </summary>
        public ExportedType DeclaringType => Implementation as ExportedType;

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public IResolutionScope Scope => Module;

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
        public bool IsValueType => Resolve()?.IsValueType ?? false;

        /// <inheritdoc />
        public TypeDefinition Resolve() => Module?.MetadataResolver?.ResolveType(this);
        
        IMemberDefinition IMemberDescriptor.Resolve() => Resolve();

        /// <inheritdoc />
        public ITypeDefOrRef ToTypeDefOrRef() => new TypeReference(Module, Scope, Namespace, Name);

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() => new TypeDefOrRefSignature(ToTypeDefOrRef());

        /// <summary>
        /// Obtains the namespace of the exported type.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual string GetNamespace() => null;

        /// <summary>
        /// Obtains the name of the exported type.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the implementation of the exported type.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Implementation"/> property.
        /// </remarks>
        protected virtual IImplementation GetImplementation() => null;

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
        public override string ToString() => FullName;
    }
}