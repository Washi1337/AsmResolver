using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external module. This module can be managed or unmanaged.
    /// </summary>
    public class ModuleReference :
        MetadataMember,
        IResolutionScope,
        IMemberRefParent,
        IHasCustomAttribute,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<Utf8String?> _name;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes the module reference with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ModuleReference(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<Utf8String?>(GetName);
        }

        /// <summary>
        /// Creates a new reference to an external module.
        /// </summary>
        /// <param name="name">The file name of the module.</param>
        public ModuleReference(Utf8String? name)
            : this(new MetadataToken(TableIndex.ModuleRef, 0))
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the module definition table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public ModuleDefinition? Module
        {
            get;
            private set;
        }

        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
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
        public bool IsImportedInModule(ModuleDefinition module) => Module == module;

        /// <summary>
        /// Imports the module reference using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported module.</returns>
        public ModuleReference ImportWith(ReferenceImporter importer) => importer.ImportModule(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <summary>
        /// Obtains the name of the module.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        AssemblyDescriptor? IResolutionScope.GetAssembly() => Module?.Assembly;

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
