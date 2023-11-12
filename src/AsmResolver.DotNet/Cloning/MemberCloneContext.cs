using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;

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
        /// <param name="importerFactory">The factory for creating the reference importer</param>
        public MemberCloneContext(ModuleDefinition module,
            Func<MemberCloneContext, CloneContextAwareReferenceImporter>? importerFactory)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Importer = importerFactory?.Invoke(this) ?? new CloneContextAwareReferenceImporter(this);
        }

        /// <summary>
        /// Gets the target module to copy the cloned members into.
        /// </summary>
        public ModuleDefinition Module
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for importing references into the target module.
        /// </summary>
        public CloneContextAwareReferenceImporter Importer
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

        /// <summary>
        /// Gets a mapping of original types to their cloned counterparts.
        /// </summary>
        /// <remarks>
        /// This dictionary performs lookups based on value using a <see cref="SignatureComparer"/> instead of object
        /// identity, and can thus be used to translate type references to included type definitions.
        /// </remarks>
        public IDictionary<ITypeDescriptor, ITypeDescriptor> ClonedTypes
        {
            get;
        } = new Dictionary<ITypeDescriptor, ITypeDescriptor>(SignatureComparer.Default);
    }
}
