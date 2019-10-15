namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in a metadata table.
    /// </summary>
    public interface IMetadataRow
    {
        TableIndex TableIndex
        {
            get;
        }
    }
}