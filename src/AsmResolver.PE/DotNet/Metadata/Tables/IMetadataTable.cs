using System;
using System.Collections;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a metadata table stored in the tables stream of a managed executable.
    /// </summary>
    public interface IMetadataTable : ICollection
    {
        /// <summary>
        /// Gets or sets the row at the provided index.
        /// </summary>
        /// <param name="index">The index of the row to get.</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when the index is too small or too large for this table.</exception>
        IMetadataRow this[int index]
        {
            get;
            set;
        }
        
    }
}