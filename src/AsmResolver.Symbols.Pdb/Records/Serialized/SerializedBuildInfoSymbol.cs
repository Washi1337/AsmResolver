using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

public class SerializedBuildInfoSymbol : BuildInfoSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _idIndex;

    public SerializedBuildInfoSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        _idIndex = reader.ReadUInt32();
    }

    protected override BuildInfoLeaf? GetInfo()
    {
        return base.GetInfo();
    }
}
