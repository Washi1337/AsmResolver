using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Provides information about a metadata table that was constructed by a metadata table buffer.
    /// </summary>
    /// <typeparam name="TRow">The type of rows the table contains.</typeparam>
    public interface IConstructedTableInfo<TRow> where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// Gets the constructed metadata table.
        /// </summary>
        MetadataTable<TRow> ConstructedTable
        {
            get;
        }

        /// <summary>
        /// Resolves a metadata row handle to its row identifier.
        /// </summary>
        /// <param name="handle">The handle to the row.</param>
        /// <returns>The row identifier (RID) that corresponds to the handle.</returns>
        uint GetRid(MetadataRowHandle handle);
    }
}