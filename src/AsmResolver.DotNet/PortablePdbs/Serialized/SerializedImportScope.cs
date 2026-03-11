using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public class SerializedImportScope : ImportScope
{
    private readonly PdbReaderContext _context;
    private readonly ImportScopeRow _row;

    public SerializedImportScope(PdbReaderContext context, MetadataToken token, in ImportScopeRow row)
        : base(token)
    {
        _context = context;
        _row = row;
    }

    protected override ImportScope? GetParent() => _context.Pdb.TryLookupMember<ImportScope>(new MetadataToken(TableIndex.ImportScope, _row.Parent), out var parent) ? parent : null;

    protected override IList<ImportScope> GetChildren()
    {
        var rids = _context.Pdb.GetImportScopeChildren(MetadataToken.Rid);
        var children = new MemberCollection<ImportScope, ImportScope>(this);

        foreach (var rid in rids)
        {
            if (_context.Pdb.TryLookupMember<ImportScope>(new MetadataToken(TableIndex.ImportScope, rid), out var scope))
                children.AddNoOwnerCheck(scope);
        }

        return children;
    }

    protected override ImportsCollection GetImports()
    {
        if (_context.BlobStream!.TryGetBlobReaderByIndex(_row.Imports, out var reader))
        {
            return ImportsCollection.FromReader(_context, this, ref reader);
        }
        return new ImportsCollection();
    }
}