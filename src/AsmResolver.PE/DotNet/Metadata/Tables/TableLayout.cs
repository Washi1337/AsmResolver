using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides a description of the layout of a table in the tables metadata stream. This includes information about
    /// the name, data type and size in bytes of each column.
    /// </summary>
    public readonly struct TableLayout
    {
        /// <summary>
        /// Defines a new layout for a metadata table.
        /// </summary>
        /// <param name="columns">The column layouts.</param>
        public TableLayout(params ColumnLayout[] columns)
        {
            Columns = new ReadOnlyCollection<ColumnLayout>(columns);
            RowSize = (uint) columns.Sum(c => c.Size);
        }

        /// <summary>
        /// Gets a collection of columns that this table defines.
        /// </summary>
        public IList<ColumnLayout> Columns
        {
            get;
        }

        /// <summary>
        /// Gets the total size in bytes of a single row in the table.
        /// </summary>
        public uint RowSize
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({string.Join(", ", Columns.Select(c => c.Name).ToArray())})";
        }

    }
}
