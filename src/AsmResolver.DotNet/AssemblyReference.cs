using System;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external .NET assembly, hosted by a common language runtime (CLR). 
    /// </summary>
    public class AssemblyReference : AssemblyDescriptor, IResolutionScope, IOwnedCollectionElement<ModuleDefinition>
    {
        /// <summary>
        /// Initializes a new assembly reference.
        /// </summary>
        /// <param name="token">The token of the assembly reference.</param>
        protected AssemblyReference(MetadataToken token) 
            : base(token)
        {
        }
        
        /// <summary>
        /// Creates a new assembly reference.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        public AssemblyReference(string name, Version version)
            : base(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            Name = name;
            Version = version;
        }

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
            private set;
        }

        /// <inheritdoc />
        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }
    }
}