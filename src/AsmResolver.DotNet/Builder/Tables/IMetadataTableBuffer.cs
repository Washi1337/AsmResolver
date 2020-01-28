using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    public interface IMetadataTableBuffer
    {
        /// <summary>
        /// Gets the number of rows that were added to the buffer.
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Submits all rows to the underlying table stream.
        /// </summary>
        void FlushToTable();
    }

    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
    public interface IMetadataTableBuffer<TRow> : IMetadataTableBuffer
        where TRow : struct, IMetadataRow
    {
        TRow this[uint rid]
        {
            get;
            set;
        }
        
        /// <summary>
        /// Adds a row to the metadata table.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <returns>The RID that this row was assigned to.</returns>
        /// <remarks>
        /// For some metadata table buffers, the RID that the row was assigned to might not be definitive. Sorted
        /// metadata table buffers will reorder the table once all rows have been added to the buffer.
        /// </remarks>
        uint Add(in TRow row);
    }
}