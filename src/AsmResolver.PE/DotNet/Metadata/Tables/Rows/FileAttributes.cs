namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    public enum FileAttributes : uint
    {
        /// <summary>
        /// Specifies the file reference contains metadata.
        /// </summary>
        ContainsMetadata,
        /// <summary>
        /// Specifies the file references doesn't contain metadata.
        /// </summary>
        ContainsNoMetadata,
    }
}