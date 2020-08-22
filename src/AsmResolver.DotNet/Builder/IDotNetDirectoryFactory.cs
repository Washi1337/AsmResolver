using AsmResolver.PE;
using AsmResolver.PE.DotNet;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides members for constructing a .NET data directory that can be inserted into a <see cref="IPEImage"/>.
    /// </summary>
    public interface IDotNetDirectoryFactory
    {
        /// <summary>
        /// Constructs a .NET data directory based on the provided .NET module.
        /// </summary>
        /// <param name="module">The module to serialize to a .NET data directory.</param>
        /// <param name="diagnosticBag">The bag that is used to collect all diagnostic information during the building process. </param>
        /// <returns>The serialized data directory.</returns>
        /// <exception cref="MetadataBuilderException">Occurs when the metadata builder encounters an error.</exception>
        IDotNetDirectory CreateDotNetDirectory(ModuleDefinition module, DiagnosticBag diagnosticBag);
    }
}