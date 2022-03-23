namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Provides members defining all possible file types that can be stored in a bundled .NET application.
    /// </summary>
    public enum BundleFileType
    {
        /// <summary>
        /// Indicates the file type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates the file is a .NET assembly.
        /// </summary>
        Assembly,

        /// <summary>
        /// Indicates the file is a native binary.
        /// </summary>
        NativeBinary,

        /// <summary>
        /// Indicates the file is the deps.json file associated to a .NET assembly.
        /// </summary>
        DepsJson,

        /// <summary>
        /// Indicates the file is the runtimeconfig.json file associated to a .NET assembly.
        /// </summary>
        RuntimeConfigJson,

        /// <summary>
        /// Indicates the file contains symbols.
        /// </summary>
        Symbols
    }
}
