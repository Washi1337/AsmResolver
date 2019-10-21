namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in a metadata table.
    /// </summary>
    public interface IMetadataRow
    {
        /// <summary>
        /// Gets the index of the table that this row is stored in.
        /// </summary>
        TableIndex TableIndex
        {
            get;
        }

        /// <summary>
        /// Writes the row to an output stream.
        /// </summary>
        /// <param name="writer">The output stream writer.</param>
        /// <param name="layout">The new layout of the table.</param>
        void Write(IBinaryStreamWriter writer, TableLayout layout);
    }
}