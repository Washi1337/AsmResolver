using System.Collections.Generic;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Represents a single module that was imported into a portable executable as part of the imports data directory.
    /// Each instance represents one entry in the imports directory.
    /// </summary>
    public interface IImportedModule
    {
        /// <summary>
        /// Gets or sets the name of the module that was imported.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp that the module was loaded into memory.
        /// </summary>
        /// <remarks>
        /// This field is always 0 if the PE was read from the disk.
        /// </remarks>
        uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the first member that is a forwarder.
        /// </summary>
        uint ForwarderChain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of members from the module that were imported.
        /// </summary>
        IList<ImportedSymbol> Symbols
        {
            get;
        }
    }
}