using System;
using System.Collections;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a metadata table stored in the tables stream of a managed executable.
    /// </summary>
    public interface IMetadataTable : ICollection, ISegment
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
        /// Gets or sets a value indicating whether the table is considered sorted.
        /// </summary>
        bool IsSorted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <returns>The row.</returns>
        IMetadataRow GetByRid(uint rid);

        /// <summary>
        /// Attempts to get the contents of a cell in the table by its row identifier and column index.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="column">The column index.</param>
        /// <param name="value">When successful, the contents of the cell, converted to an unsigned integer.</param>
        /// <returns><c>true</c> if the cell existed and was obtained successfully, <c>false</c> otherwise.</returns>
        bool TryGetCell(uint rid, int column, out uint value);

        /// <summary>
        /// Attempts to get the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">When successful, the read row.</param>
        /// <returns><c>true</c> if the RID existed and the row was obtained successfully, <c>false</c> otherwise.</returns>
        bool TryGetByRid(uint rid, out IMetadataRow row);

        /// <summary>
        /// Attempts to find a row index in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <param name="rid">When this functions returns <c>true</c>, this parameter contains the RID of the row that
        /// contains the given key.</param>
        /// <returns><c>true</c> if the row was found, <c>false</c> otherwise.</returns>
        bool TryGetRidByKey(int keyColumnIndex, uint key, out uint rid);

        /// <summary>
        /// Gets a single row in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <param name="row">When this functions returns <c>true</c>, this parameter contains the first row that
        /// contains the given key.</param>
        /// <returns><c>true</c> if the row was found, <c>false</c> otherwise.</returns>
        bool TryGetRowByKey(int keyColumnIndex, uint key, out IMetadataRow row);

        /// <summary>
        /// Sets the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">The new contents of the row.</param>
        void SetByRid(uint rid, IMetadataRow row);

        /// <summary>
        /// Updates the table layout.
        /// </summary>
        /// <param name="layout">The new table layout.</param>
        /// <remarks>
        /// This method is used to update the sizes of each column, and therefore requires the new layout to have the
        /// same names and column types as the original one.
        /// </remarks>
        void UpdateTableLayout(TableLayout layout);

        /// <summary>
        /// Calculates the offset range of a row within the table.
        /// </summary>
        /// <param name="rid">The identifier of the row to get the bounds of.</param>
        /// <returns>The bounds.</returns>
        /// <remarks>
        /// This method does not do any verification on whether <paramref name="rid"/> is a valid row in the table.
        /// </remarks>
        OffsetRange GetRowBounds(uint rid);
    }
}
