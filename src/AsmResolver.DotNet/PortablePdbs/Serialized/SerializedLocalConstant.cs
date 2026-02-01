using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public class SerializedLocalConstant : LocalConstant
{
    private readonly PdbReaderContext _context;
    private readonly LocalConstantRow _row;

    public SerializedLocalConstant(PdbReaderContext context, MetadataToken token, in LocalConstantRow row)
        : base(token)
    {
        _context = context;
        _row = row;
    }

    protected override LocalScope? GetOwner() => _context.Pdb.TryLookupMember<LocalScope>(new MetadataToken(TableIndex.LocalScope, _context.Pdb.GetLocalConstantOwner(MetadataToken.Rid)), out var scope) ? scope : null;

    protected override Utf8String? GetName() => _context.StringsStream!.GetStringByIndex(_row.Name);

    protected override LocalConstantSignature? GetSignature()
    {
        if (_context.BlobStream?.TryGetBlobReaderByIndex(_row.Signature, out var reader) ?? false)
        {
            var context = new BlobReaderContext(_context.OwningModule.ReaderContext);
            return LocalConstantSignature.FromReader(ref context, ref reader);
        }
        return null;
    }

    protected override IList<CustomDebugInformation> GetCustomDebugInformations() => _context.Pdb.GetCustomDebugInformations(this);
}
