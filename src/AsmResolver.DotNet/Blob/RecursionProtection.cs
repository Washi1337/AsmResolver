using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Provides a mechanism for detecting infinite recursion within the structures of .NET metadata.
    /// </summary>
    public struct RecursionProtection
    {
        /// <summary>
        /// Creates a new, empty recursion protection instance.
        /// </summary>
        public static RecursionProtection CreateNew() => new RecursionProtection(Enumerable.Empty<MetadataToken>());

        /// <summary>
        /// Creates a new recursion protection instance.
        /// </summary>
        /// <param name="traversedTokens">The tokens that were traversed.</param>
        public RecursionProtection(IEnumerable<MetadataToken> traversedTokens)
        {
            TraversedTokens = new HashSet<MetadataToken>(traversedTokens);
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