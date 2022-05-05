namespace AsmResolver.DotNet.Code.Native;

public class NativeLocalSymbol : ISymbol
{
    public NativeLocalSymbol(NativeMethodBody body, uint offset)
    {
        Body = body;
        Offset = offset;
    }

    public NativeMethodBody Body
    {
        get;
    }

    public uint Offset
    {
        get;
    }

    /// <inheritdoc />
    public ISegmentReference? GetReference() => Body.Address is not null
        ? new RelativeReference(Body.Address, (int) Offset)
        : null;
}
