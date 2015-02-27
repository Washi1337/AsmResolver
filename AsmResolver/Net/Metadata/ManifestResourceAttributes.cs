namespace AsmResolver.Net.Metadata
{
    public enum ManifestResourceAttributes : uint
    {
        VisibilityMask = 0x0007,
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