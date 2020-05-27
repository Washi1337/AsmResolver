using System.Collections.Generic;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides an implementation of the <see cref="ITypeDiscoveryStrategy"/> interface that performs a traversal
    /// on the member tree.
    /// </summary>
    public class TreeTraversalTypeDiscoveryStrategy : ITypeDiscoveryStrategy
    {
        /// <summary>
        /// Gets a reusable instance of the <see cref="TreeTraversalTypeDiscoveryStrategy"/> class.
        /// </summary>
        public static TreeTraversalTypeDiscoveryStrategy Instance
        {
            get;
        } = new TreeTraversalTypeDiscoveryStrategy();
        
        /// <inheritdoc />
        public IEnumerable<TypeDefinition> CollectTypes(ModuleDefinition module) => module.GetAllTypes();
    }
}