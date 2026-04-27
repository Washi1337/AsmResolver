using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class LocalScope : IMetadataMember, IOwnedCollectionElement<MethodDefinition>
{
    public LocalScope() : this(new MetadataToken(TableIndex.LocalScope, 0)) { }

    public LocalScope(MetadataToken token)
    {
        MetadataToken = token;
    }

    public MetadataToken MetadataToken
    {
        get;
    }

    [LazyProperty]
    public partial MethodDefinition? Owner
    {
        get;
        set;
    }

    [LazyProperty]
    public partial ImportScope? ImportScope
    {
        get;
        set;
    }

    [LazyProperty]
    public partial IList<LocalVariable> LocalVariables
    {
        get;
    }

    [LazyProperty]
    public partial IList<LocalConstant> LocalConstants
    {
        get;
    }

    protected virtual MethodDefinition? GetOwner() => null;

    protected virtual ImportScope? GetImportScope() => null;

    protected virtual IList<LocalVariable> GetLocalVariables() => new MemberCollection<LocalScope, LocalVariable>(this);

    protected virtual IList<LocalConstant> GetLocalConstants() => new MemberCollection<LocalScope, LocalConstant>(this);
}