using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

[DebuggerDisplay("{Name}")]
public partial class Document : IMetadataMember, IOwnedCollectionElement<ModuleDefinition>, IHasCustomDebugInformation
{
    public Document() : this(new MetadataToken(TableIndex.Document, 0)) { }

    public Document(MetadataToken token)
    {
        MetadataToken = token;
    }

    public MetadataToken MetadataToken { get; }

    [LazyProperty]
    public partial ModuleDefinition? Owner
    {
        get;
        private set;
    }

    ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
    {
        get => Owner;
        set => Owner = value;
    }

    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    [LazyProperty]
    public partial Guid HashAlgorithm
    {
        get;
        set;
    }

    [LazyProperty]
    public partial byte[]? Hash
    {
        get;
        set;
    }

    [LazyProperty]
    public partial Guid Language
    {
        get;
        set;
    }

    [LazyProperty]
    public partial IList<CustomDebugInformation> CustomDebugInformations
    {
        get;
    }

    protected virtual ModuleDefinition? GetOwner() => null;

    protected virtual Utf8String? GetName() => null;

    protected virtual Guid GetHashAlgorithm() => Guid.Empty;

    protected virtual byte[]? GetHash() => null;

    protected virtual Guid GetLanguage() => Guid.Empty;

    protected virtual IList<CustomDebugInformation> GetCustomDebugInformations() => new MemberCollection<IHasCustomDebugInformation, CustomDebugInformation>(this);
}
