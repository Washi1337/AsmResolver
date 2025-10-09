using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class ImportScope : IMetadataMember, IOwnedCollectionElement<ImportScope>
{
    public MetadataToken MetadataToken
    {
        get;
    }

    [LazyProperty]
    public partial ImportScope? Owner
    {
        get;
        set;
    }
}
