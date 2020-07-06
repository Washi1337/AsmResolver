namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Provides members defining all flags that can be assigned to a manifest resource.
    /// </summary>
    public enum ManifestResourceAttributes : uint
    {
        /// <summary>
        /// Specifies the resource is exported from the asembly.
        /// </summary>
        Public = 0x0001,
        /// <summary>
        /// Specifies the resource is private to the assembly.
        /// </summary>
        Private = 0x0002,
    }
}