using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides a description of the layout of a table in the tables metadata stream. This includes information about
    /// the name, data type and size in bytes of each column.
    /// </summary>
    public class TableLayout
    {
        public TableLayout(params ColumnLayout[] columns)
        {
            Columns = columns;
            RowSize = columns.Sum(c => c.Size);
        }
        
        /// <summary>
        /// Gets a collection of columns that this table defines.
        /// </summary>
        public ColumnLayout[] Columns
        {
            get;
        }

        /// <summary>
        /// Gets the total size in bytes of a single row in the table.
        /// </summary>
        public int RowSize
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({string.Join(", ", Columns.Select(c => c.Name))})";
        }
        
    }
}