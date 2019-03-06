using System.Collections.Generic;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public class RecursionProtection
    {
        public ISet<MetadataToken> TraversedTokens
        {
            get;
        } = new HashSet<MetadataToken>();

        public static RecursionProtection Create()
        {
            return new RecursionProtection();
        }
    }
}