using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external module. This module can be managed or unmanaged.
    /// </summary>
    public class ModuleReference :
        IResolutionScope,
        IMemberRefParent,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<string> _name;

        /// <summary>
        /// Initializes the module reference with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ModuleReference(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
        }

        /// <summary>
        /// Creates a new reference to an external module.
        /// </summary>
        /// <param name="name">The file name of the module.</param>
        public ModuleReference(string name)
            : this(new MetadataToken(TableIndex.ModuleRef, 0))
        {
            Name = name;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

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
        /// Obtains the name of the module.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        AssemblyDescriptor IResolutionScope.GetAssembly() => Module.Assembly;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}