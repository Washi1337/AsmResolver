using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a basic implementation of a resource directory that can be initialized and added to another resource
    /// directory or used as a root resource directory of a PE image.
    /// </summary>
    public class ResourceDirectory : ResourceDirectoryBase
    {
        /// <summary>
        /// Creates a new named resource directory. 
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public ResourceDirectory(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <summary>
        /// Creates a new resource directory defined by a numeric identifier. 
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ResourceDirectory(uint id)
        {
            Id = id;
        }

        /// <inheritdoc />
        protected override IList<IResourceDirectoryEntry> GetEntries()
        {
            return new List<IResourceDirectoryEntry>();
        }
        
    }
}