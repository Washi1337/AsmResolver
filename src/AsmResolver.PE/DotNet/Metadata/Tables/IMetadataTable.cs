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
        /// Gets the size of an index into this table.
        /// </summary>
        IndexSize IndexSize
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
        /// Gets the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <returns>The row.</returns>
        IMetadataRow GetByRid(uint rid);
        
        /// <summary>
        /// Attempts to get the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">When successful, the read row.</param>
        /// <returns><c>true</c> if the RID existed an the row was obtained successfully, <c>false</c> otherwise.</returns>
        bool TryGetByRid(uint rid, out IMetadataRow row);
        
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
        /// Serializes the table to an output stream, according to the table layout provided in <see cref="Layout" />.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        void Write(IBinaryStreamWriter writer);
    }
}