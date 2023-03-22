namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides extra information about a procedure and its frame layout.
/// </summary>
public class FrameProcedureSymbol : CodeViewSymbol
{
    /// <summary>
    /// Initializes an empty frame procedure symbol.
    /// </summary>
    protected FrameProcedureSymbol()
    {
    }

    /// <summary>
    /// Creates a new frame procedure symbol.
    /// </summary>
    /// <param name="frameSize">The size of the frame.</param>
    /// <param name="attributes">The attributes associated to the procedure.</param>
    public FrameProcedureSymbol(uint frameSize, FrameProcedureAttributes attributes)
    {
        FrameSize = frameSize;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.FrameProc;

    /// <summary>
    /// Gets or sets the size (in bytes) of the procedure's frame.
    /// </summary>
    public uint FrameSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size (in bytes) of the procedure's frame padding.
    /// </summary>
    public uint PaddingSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset relative to the frame pointer where the padding starts.
    /// </summary>
    public int PaddingOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size (in bytes) of the callee register saves storage.
    /// </summary>
    public uint CalleeSavesSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset to the exception handler (if available).
    /// </summary>
    public int ExceptionHandlerOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the section index to the exception handler (if available).
    /// </summary>
    public ushort ExceptionHandlerSection
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the attributes associated to the procedure.
    /// </summary>
    public FrameProcedureAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the local base register pointer for the procedure.
    /// </summary>
    public byte LocalBasePointer
    {
        get => (byte) (((int) Attributes >> 14) & 0b11);
        set => Attributes = (Attributes & ~FrameProcedureAttributes.EncodedLocalBasePointerMask)
                            | (FrameProcedureAttributes) ((value & 0b11) << 14);
    }

    /// <summary>
    /// Gets or sets the parameter base register pointer for the procedure.
    /// </summary>
    public byte ParameterBasePointer
    {
        get => (byte) (((int) Attributes >> 14) & 0b11);
        set => Attributes = (Attributes & ~FrameProcedureAttributes.EncodedParamBasePointerMask)
                            | (FrameProcedureAttributes) ((value & 0b11) << 16);
    }
}
