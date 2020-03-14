using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides a working space for a member cloning procedure.
    /// </summary>
    public class MetadataCloneContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MetadataCloneContext"/> class.
        /// </summary>
        /// <param name="module">The target module to copy the cloned members into.</param>
        public MetadataCloneContext(ModuleDefinition module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Importer = new CloneContextAwareReferenceImporter(this);
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