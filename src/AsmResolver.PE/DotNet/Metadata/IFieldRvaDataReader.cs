using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides members for reading data referenced by a row in the FieldRVA table.
    /// </summary>
    public interface IFieldRvaDataReader
    {
        /// <summary>
        /// Reads a data segment referenced by a row in the FieldRVA table. 
        /// </summary>
        /// <param name="fieldRvaRow">The row referencing the data.</param>
        /// <returns>The data segment, or <c>null</c> if no data was referenced.</returns>
        ISegment ResolveFieldData(FieldRvaRow fieldRvaRow);
    }
}