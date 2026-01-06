using AsmResolver.Collections;
using AsmResolver.DotNet.PortablePdbs.CustomRecords;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class CustomDebugInformation : IMetadataMember, IOwnedCollectionElement<IHasCustomDebugInformation>
{
    public CustomDebugInformation(MetadataToken token)
    {
        MetadataToken = token;
    }

    public MetadataToken MetadataToken
    {
        get;
    }

    [LazyProperty]
    public partial IHasCustomDebugInformation? Owner
    {
        get;
        set;
    }

    [LazyProperty]
    public partial CustomDebugRecord? Value
    {
        get;
        set;
    }

    protected virtual IHasCustomDebugInformation? GetOwner() => null;

    protected virtual CustomDebugRecord? GetValue() => null;
}
