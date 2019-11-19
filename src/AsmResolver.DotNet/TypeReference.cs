using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a type defined in a .NET assembly.
    /// </summary>
    public class TypeReference : ITypeDefOrRef, IResolutionScope
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<string> _namespace;
        private readonly LazyVariable<IResolutionScope> _scope;
        private string _fullName;

        /// <summary>
        /// Initializes a new empty type reference.
        /// </summary>
        /// <param name="token">The token of the type reference.</param>
        protected TypeReference(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _namespace = new LazyVariable<string>(GetNamespace);
            _scope = new LazyVariable<IResolutionScope>(GetScope);
        }

        /// <summary>
        /// Creates a new reference to a type.
        /// </summary>
        /// <param name="scope">The scope that defines the type.</param>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        public TypeReference(IResolutionScope scope, string ns, string name)
            : this(new MetadataToken(TableIndex.TypeRef, 0))
        {
            _scope.Value = scope;
            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Creates a new reference to a type.
        /// </summary>
        /// <param name="module">The module that references the type.</param>
        /// <param name="scope">The scope that defines the type.</param>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        public TypeReference(ModuleDefinition module, IResolutionScope scope, string ns, string name)
            : this(new MetadataToken(TableIndex.TypeRef, 0))
        {
            _scope.Value = scope;
            Module = module;
            Namespace = ns;
            Name = name;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the name of the type that this object is referencing.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <summary>
        /// Gets or sets the namespace the type is residing in.
        /// </summary>
        public string Namespace
        {
            get => _namespace.Value;
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName => _fullName ?? (_fullName = this.GetFullName());

        /// <inheritdoc />
        public IResolutionScope Scope => _scope.Value;

        /// <inheritdoc />
        public bool IsValueType => false; // TODO: resolve. 

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public ITypeDefOrRef DeclaringType => Scope as ITypeDefOrRef;

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        /// <summary>
        /// Obtains the name of the type reference.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the namespace of the type reference.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual string GetNamespace() => null;

        /// <summary>
        /// Obtains the scope of the type reference.
        /// </summary>
        /// <returns>The scope.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Scope"/> property.
        /// </remarks>
        protected virtual IResolutionScope GetScope() => null;
    }
}