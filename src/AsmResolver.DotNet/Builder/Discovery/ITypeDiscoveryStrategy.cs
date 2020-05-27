using System.Collections.Generic;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides members for discovering type definitions within a single module.
    /// </summary>
    public interface ITypeDiscoveryStrategy
    {
        /// <summary>
        /// Collects all types in a single module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>The types.</returns>
        IEnumerable<TypeDefinition> CollectTypes(ModuleDefinition module);
    }
}