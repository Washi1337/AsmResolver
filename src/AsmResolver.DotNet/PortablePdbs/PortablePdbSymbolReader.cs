using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public class PortablePdbSymbolReader : ISymbolReader
{
    public SerializedPortablePdb Pdb { get; }

    public PortablePdbSymbolReader(SerializedPortablePdb pdb)
    {
        Pdb = pdb;
    }

    public IEnumerable<Document> GetDocuments()
    {
        var documentsTable = Pdb.PdbReaderContext.TablesStream.GetTable<DocumentRow>();
        for (uint i = 0; i < documentsTable.Count; i++)
        {
            if (Pdb.TryLookupMember<Document>(new MetadataToken(TableIndex.Document, i + 1), out var document))
            {
                yield return document;
            }
        }
    }

    public MethodDebugInformation? GetMethodDebugInformation(SerializedMethodDefinition method)
    {
        Pdb.TryLookupMember<MethodDebugInformation>(new MetadataToken(TableIndex.MethodDebugInformation, method.MetadataToken.Rid), out var mdi);
        return mdi;
    }

    public IEnumerable<LocalScope> GetLocalScopes(SerializedMethodDefinition method)
    {
        var rids = Pdb.GetLocalScopes(method.MetadataToken.Rid);
        foreach (var localScope in rids)
        {
            if (Pdb.TryLookupMember<LocalScope>(
                    new MetadataToken(TableIndex.LocalScope, localScope), out var scope))
            {
                yield return scope;
            }
        }
    }

    public MethodDefinition? GetKickoffMethod(SerializedMethodDefinition method)
    {
        Pdb.PdbReaderContext.OwningModule.TryLookupMember<MethodDefinition>(
            new MetadataToken(TableIndex.Method, Pdb.GetKickoffMethod(method.MetadataToken.Rid)),
            out var kickoffMethod
        );
        return kickoffMethod;
    }

    public MethodDefinition? GetMoveNextMethod(SerializedMethodDefinition method)
    {
        Pdb.PdbReaderContext.OwningModule.TryLookupMember<MethodDefinition>(
            new MetadataToken(TableIndex.Method, Pdb.GetMoveNextMethod(method.MetadataToken.Rid)),
            out var moveNextMethod
        );
        return moveNextMethod;
    }

    public IEnumerable<CustomDebugInformation> GetCustomDebugInformations(IHasCustomDebugInformation member)
    {
        var rids = Pdb.GetCustomDebugInformations(member.MetadataToken.Rid);
        foreach (var rid in rids)
        {
            if (Pdb.TryLookupMember<CustomDebugInformation>(new MetadataToken(TableIndex.CustomDebugInformation, rid), out var cdi))
            {
                yield return cdi;
            }
        }
    }
}
