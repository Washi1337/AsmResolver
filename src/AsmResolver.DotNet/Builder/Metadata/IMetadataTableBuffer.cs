using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder.Metadata
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

        /// <summary>
        /// Clears the table buffer.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
    public interface IMetadataTableBuffer<TRow> : IMetadataTableBuffer
        where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// Gets or sets a row in the metadata table.
        /// </summary>
        /// <param name="rid">The identifier of the metadata row.</param>
        TRow this[uint rid]
        {
            get;
            set;
        }

        /// <summary>
        /// Ensures the capacity of the table buffer is at least the provided amount of elements.
        /// </summary>
        /// <param name="capacity">The number of elements to store.</param>
        void EnsureCapacity(int capacity);

        /// <summary>
        /// Gets or sets a reference to a row in the metadata table.
        /// </summary>
        /// <param name="rid">The identifier of the metadata row.</param>
        ref TRow GetRowRef(uint rid);

        /// <summary>
        /// Adds a row to the metadata table.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <returns>The metadata token that this row was assigned to.</returns>
        MetadataToken Add(in TRow row);

        /// <summary>
        /// Inserts a row into the metadata table at the provided row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">The row to add.</param>
        /// <returns>The metadata token that this row was assigned to.</returns>
        MetadataToken Insert(uint rid, in TRow row);
    }
}
