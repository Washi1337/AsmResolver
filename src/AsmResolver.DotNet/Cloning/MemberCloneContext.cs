using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides a working space for a member cloning procedure.
    /// </summary>
    public class MemberCloneContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloneContext"/> class.
        /// </summary>
        /// <param name="module">The target module to copy the cloned members into.</param>
        public MemberCloneContext(ModuleDefinition module) : this(module, null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloneContext"/> class.
        /// </summary>
        /// <param name="module">The target module to copy the cloned members into.</param>
        /// <param name="importerInstantiator">The instantiator for creating the reference importer</param>
        public MemberCloneContext(ModuleDefinition module,
            Func<MemberCloneContext, CloneContextAwareReferenceImporter>? importerInstantiator)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Importer = importerInstantiator?.Invoke(this) ?? new CloneContextAwareReferenceImporter(this);
        }

        /// <summary>
        /// Gets the target module to copy the cloned members into.
        /// </summary>
        public ModuleDefinition Module
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for importing references into the target mdoule. 
        /// </summary>
        public ReferenceImporter Importer
        {
            get;
        }

        /// <summary>
        /// Gets a mapping of original members to cloned members.
        /// </summary>
        public IDictionary<IMemberDescriptor, IMemberDescriptor> ClonedMembers
        {
            get;
        } = new Dictionary<IMemberDescriptor, IMemberDescriptor>();
    }
}
