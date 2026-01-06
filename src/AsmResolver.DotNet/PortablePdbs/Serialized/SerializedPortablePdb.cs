using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public partial class SerializedPortablePdb : PortablePdb
{
    public SerializedPortablePdb(MetadataDirectory metadata, SerializedModuleDefinition owningModule) : base(owningModule)
    {
        PdbReaderContext = new PdbReaderContext(this, metadata, owningModule);
        _factory = new CachedSerializedPdbMemberFactory(PdbReaderContext);

        var pdbStream = PdbReaderContext.PdbStream;

        Array.Copy(pdbStream.Id, PdbId, 20);

        _localVariableLists = new LazyRidListRelation<LocalScopeRow>(PdbReaderContext.Metadata, TableIndex.LocalVariable, TableIndex.LocalScope,
            (rid, _) => rid, PdbReaderContext.TablesStream.GetLocalVariableRange);
        _localConstantLists = new LazyRidListRelation<LocalScopeRow>(PdbReaderContext.Metadata, TableIndex.LocalConstant, TableIndex.LocalScope,
            (rid, _) => rid, PdbReaderContext.TablesStream.GetLocalConstantRange);
    }

    public PdbReaderContext PdbReaderContext { get; }

    public override bool TryLookupMember<T>(MetadataToken token, [MaybeNullWhen(false)] out T member) where T : class
    {
        if (_factory.TryLookupMember(token, out var metadataMember))
        {
            member = (T)metadataMember;
            return true;
        }

        member = null;
        return false;
    }

    public IList<CustomDebugInformation> GetCustomDebugInformations(IHasCustomDebugInformation owner)
    {
        var rids = GetCustomDebugInformations(owner.MetadataToken);
        var customDebugInformations = new MemberCollection<IHasCustomDebugInformation, CustomDebugInformation>(owner, rids.Count);
        foreach (var rid in rids)
        {
            if (TryLookupMember<CustomDebugInformation>(new MetadataToken(TableIndex.CustomDebugInformation, rid), out var debugInfo))
            {
                customDebugInformations.AddNoOwnerCheck(debugInfo);
            }
        }
        return customDebugInformations;
    }
}
