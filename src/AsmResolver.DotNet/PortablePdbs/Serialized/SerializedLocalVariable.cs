using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public class SerializedLocalVariable : LocalVariable
{
    private readonly PdbReaderContext _context;
    private readonly LocalVariableRow _row;

    public SerializedLocalVariable(PdbReaderContext context, MetadataToken token, in LocalVariableRow row)
        : base(token)
    {
        _context = context;
        _row = row;

        Index = _row.Index;
        Attributes = _row.Attributes;
    }

    protected override LocalScope? GetScope()
    {
        var ownerToken = new MetadataToken(TableIndex.Method, _context.Pdb.GetLocalVariableOwner(MetadataToken.Rid));
        return _context.Pdb.TryLookupMember<LocalScope>(ownerToken, out var member)
            ? member
            : _context.BadImageAndReturn<LocalScope>(
                $"Parameter {MetadataToken.ToString()} is not in a range of a method.");
    }

    protected override Utf8String? GetName() => _context.StringsStream!.GetStringByIndex(_row.Name);
}