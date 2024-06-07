using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in a metadata table.
    /// </summary>
    public interface IMetadataRow
        // .NET 4.5+ and any .NET standard compatible framework define IReadOnlyList<T>, which defines Count and the
        // indexer property.
        #if NET20 || NET35 || NET40
        : IEnumerable<uint>
        #else
        : IReadOnlyList<uint>
        #endif
    {
        /// <summary>
        /// Gets or sets the index of the table that this row is stored in.
        /// </summary>
        TableIndex TableIndex
        {
            get;
        }

        #if NET20 || NET35 || NET40
        /// <summary>
        /// Gets or sets the number of columns that the metadata row defines.
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Gets or sets the value of a column, zero-extended to an unsigned int32.
        /// </summary>
        /// <param name="index">The column index.</param>
        uint this[int index]
        {
            get;
        }
        #endif

        /// <summary>
        /// Writes the row to an output stream.
        /// </summary>
        /// <param name="writer">The output stream writer.</param>
        /// <param name="layout">The new layout of the table.</param>
        void Write(BinaryStreamWriter writer, TableLayout layout);
    }
}
