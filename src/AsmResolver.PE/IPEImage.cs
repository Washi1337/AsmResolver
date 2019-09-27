using System.Collections.Generic;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    /// <summary>
    /// Represents an image of a portable executable (PE) file, exposing high level mutable structures. 
    /// </summary>
    public interface IPEImage
    {
        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        IList<IModuleImportEntry> Imports
        {
            get;
        }

        /// <summary>
        /// Gets or sets the root resource directory in the PE, if available.
        /// </summary>
        IResourceDirectory Resources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of base relocations that are to be applied when loading the PE into memory for execution. 
        /// </summary>
        IList<IRelocationBlock> Relocations
        {
            get;
        }
    }
}