using AsmResolver.DotNet.PortablePdbs.CustomRecords;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public class SerializedCustomDebugInformation : CustomDebugInformation
{
    private readonly PdbReaderContext _context;
    private readonly CustomDebugInformationRow _row;

    public SerializedCustomDebugInformation(PdbReaderContext context, MetadataToken token, CustomDebugInformationRow row)
        : base(token)
    {
        _context = context;
        _row = row;
    }

    protected override CustomDebugRecord GetValue() => CustomDebugRecord.FromRow(_context, in _row);
}
