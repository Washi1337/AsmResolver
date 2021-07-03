using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a mapping that maps a method or field defined in a .NET module to an unmanaged function or
    /// global field defined in an external module through Platform Invoke (P/Invoke).
    /// </summary>
    public class ImplementationMap : MetadataMember, IFullNameProvider
    {
        private readonly LazyVariable<string?> _name;
        private readonly LazyVariable<ModuleReference?> _scope;
        private readonly LazyVariable<IMemberForwarded?> _memberForwarded;

        /// <summary>
        /// Initializes the <see cref="ImplementationMap"/> with a metadata token.
        /// </summary>
        /// <param name="token">The token of the member.</param>
        protected ImplementationMap(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<string?>(GetName);
            _scope = new LazyVariable<ModuleReference?>(GetScope);
            _memberForwarded = new LazyVariable<IMemberForwarded?>(GetMemberForwarded);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ImplementationMap"/> class.
        /// </summary>
        /// <param name="scope">The scope that declares the imported member.</param>
        /// <param name="name">The name of the imported member.</param>
        /// <param name="attributes">The attributes associated to the implementation mapping.</param>
        public ImplementationMap(ModuleReference? scope, string? name, ImplementationMapAttributes attributes)
            : this(new MetadataToken(TableIndex.ImplMap, 0))
        {
            Scope = scope;
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the attributes associated to the implementation mapping.
        /// </summary>
        public ImplementationMapAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the member that this P/Invoke implementation mapping is assigned to.
        /// </summary>
        public IMemberForwarded? MemberForwarded
        {
            get => _memberForwarded.Value;
            internal set => _memberForwarded.Value = value;
        }

        /// <inheritdoc />
        public string? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => Scope is null
            ? Name ?? NullName
            : $"{Scope.Name}!{Name}";

        /// <summary>
        /// Gets or sets the module that contains the external member.
        /// </summary>
        public ModuleReference? Scope
        {
            get => _scope.Value;
            set => _scope.Value = value;
        }

        /// <summary>
        /// Obtains the name of the imported member.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string? GetName() => null;

        /// <summary>
        /// Obtains the scope that declares the imported member.
        /// </summary>
        /// <returns>The scope.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Scope"/> property.
        /// </remarks>
        protected virtual ModuleReference? GetScope() => null;

        /// <summary>
        /// Obtains the owner of the P/Invoke implementation mapping.
        /// </summary>
        /// <returns>The owner.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="MemberForwarded"/> property.
        /// </remarks>
        protected virtual IMemberForwarded? GetMemberForwarded() => null;
    }
}
