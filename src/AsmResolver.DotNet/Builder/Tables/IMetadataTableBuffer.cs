using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Represents a mutable buffer for a metadata table stored in the tables stream of a .NET module.
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    public interface IMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// Gets the number of rows this buffer contains.
        /// </summary>
        /// <remarks>
        /// This number of rows might include additional padding rows.
        /// </remarks>
        int Count
        {
            get;
        }
    
        /// <summary>
        /// Adds a metadata table row to the buffer.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <returns>A handle to the metadata row that can be used later to resolve to a definitive row identifier (RID).</returns>
        MetadataRowHandle Add(TRow row);

        /// <summary>
        /// Constructs a new metadata table from the buffer.
        /// </summary>
        /// <returns>An object containing the constructed metadata table, as well as a mechanism for resolving row handles
        /// to their definitive row identifiers (RIDs)</returns>
        IConstructedTableInfo<TRow> CreateTable();
    }
}