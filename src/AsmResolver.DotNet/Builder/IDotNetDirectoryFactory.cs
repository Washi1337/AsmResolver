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
        /// <returns>The serialized data directory.</returns>
        IDotNetDirectory CreateDotNetDirectory(ModuleDefinition module);
    }
}