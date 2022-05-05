namespace AsmResolver.DotNet.Code.Native;

public class NativeLocalSymbol : ISymbol
{
    public NativeLocalSymbol(NativeMethodBody body, int offset)
    {
        Body = body;
        Offset = offset;
    }

    public NativeMethodBody Body
    {
        get;
    }

    public int Offset
    {
        get;
    }

    /// <inheritdoc />
    public ISegmentReference? GetReference() => Body.Address is not null
        ? new RelativeReference(Body.Address, Offset)
        : null;
}
