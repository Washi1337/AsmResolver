namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Provides members defining all flags that can be assigned to a file reference.
    /// </summary>
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