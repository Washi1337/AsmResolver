using System;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public partial class MethodDebugInformation : IMetadataMember, IOwnedCollectionElement<MethodDefinition>
{
    public MethodDebugInformation(MethodDefinition owner) : this(new MetadataToken(TableIndex.MethodDebugInformation, 0))
    {
        Owner = owner;
    }

    public MethodDebugInformation(MetadataToken token)
    {
        MetadataToken = token;
    }

    public MetadataToken MetadataToken { get; }

    [LazyProperty]
    public partial MethodDefinition Owner
    {
        get;
        set;
    }

    [LazyProperty]
    public partial Document? Document
    {
        get;
        set;
    }

    [LazyProperty]
    public partial SequencePointCollection SequencePoints
    {
        get;
    }

    protected virtual MethodDefinition GetOwner() => throw new NotSupportedException();

    protected virtual Document? GetDocument() => null;

    protected virtual SequencePointCollection GetSequencePoints() => new(this);
}