using System.Collections.Generic;

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// Represents one block of relocations to be applied when the PE is loaded into memory.
    /// </summary>
    public interface IRelocationBlock
    {
        /// <summary>
        /// Gets or sets the RVA of the page to relocate.
        /// </summary>
        uint PageRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of relocations that need to be applied in this block.
        /// </summary>
        IList<RelocationEntry> Entries
        {
            get;
        }
    }
}