using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

public class SerializedBuildInfoSymbol : BuildInfoSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;

    public SerializedBuildInfoSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        _typeIndex = reader.ReadUInt32();
    }
}
