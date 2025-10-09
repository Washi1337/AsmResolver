using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public partial class LocalVariable : IMetadataMember, IOwnedCollectionElement<LocalScope>
    {
        public MetadataToken MetadataToken
        {
            get;
        }

        [LazyProperty]
        public partial LocalScope? Owner
        {
            get;
            set;
        }
    }
}
