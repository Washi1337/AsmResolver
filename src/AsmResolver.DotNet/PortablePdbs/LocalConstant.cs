using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class LocalConstant : IMetadataMember, IOwnedCollectionElement<LocalScope>, IHasCustomDebugInformation
{
    public LocalConstant(MetadataToken token)
    {
        MetadataToken = token;
    }

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

    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    [LazyProperty]
    public partial LocalConstantSignature? Signature
    {
        get;
        set;
    }

    [LazyProperty]
    public partial IList<CustomDebugInformation> CustomDebugInformations
    {
        get;
    }

    protected virtual LocalScope? GetOwner() => null;

    protected virtual Utf8String? GetName() => null;

    protected virtual LocalConstantSignature? GetSignature() => null;

    protected virtual IList<CustomDebugInformation> GetCustomDebugInformations() => new MemberCollection<IHasCustomDebugInformation, CustomDebugInformation>(this);
}
