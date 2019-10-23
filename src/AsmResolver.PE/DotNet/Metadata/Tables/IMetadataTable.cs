using System;
using System.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a metadata table stored in the tables stream of a managed executable.
    /// </summary>
    public interface IMetadataTable : ICollection
    {
        /// <summary>
        /// Gets the layout of the table.
        /// </summary>
        TableLayout Layout
        {
            get;
        }
        
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

        /// <summary>
        /// Updates the table layout.
        /// </summary>
        /// <param name="layout">The new table layout.</param>
        /// <remarks>
        /// This method is used to update the sizes of each column, and therefore requires the new layout to have the
        /// same names and column types as the original one.
        /// </remarks>
        void UpdateTableLayout(TableLayout layout);
    }
}