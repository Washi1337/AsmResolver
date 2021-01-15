using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public readonly struct BlobReadContext
    {
        public BlobReadContext(ModuleReadContext moduleReadContext)
            : this(moduleReadContext, Enumerable.Empty<MetadataToken>())
        {
        }

        public BlobReadContext(ModuleReadContext moduleReadContext, IEnumerable<MetadataToken> traversedTokens)
        {
            ModuleReadContext = moduleReadContext;
            TraversedTokens = new HashSet<MetadataToken>(traversedTokens);
        }

        /// <summary>
        /// Gets the module reader context.
        /// </summary>
        public ModuleReadContext ModuleReadContext
        {
            get;
        }

        /// <summary>
        /// Gets a collection of metadata tokens that were traversed during the parsing of metadata.
        /// </summary>
        public ISet<MetadataToken> TraversedTokens
        {
            get;
        }
    }
}