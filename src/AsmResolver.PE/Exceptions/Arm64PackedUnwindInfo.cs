namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Represents a packed ARM64 unwind info record associated to a RUNTIME_FUNCTION of an ARM64 executable file.
/// </summary>
public class Arm64PackedUnwindInfo : IArm64UnwindInfo
{
    private const int FunctionLengthBitIndex = 2;
    private const int FunctionLengthBitLength = 11;
    private const uint FunctionLengthBitMask = ((1u << FunctionLengthBitLength) - 1) << FunctionLengthBitIndex;

    private const int RegFBitIndex = FunctionLengthBitIndex + FunctionLengthBitLength;
    private const int RegFBitLength = 3;
    private const uint RegFBitMask = ((1u << RegFBitLength) - 1) << RegFBitIndex;

    private const int RegIBitIndex = RegFBitIndex + RegFBitLength;
    private const int RegIBitLength = 4;
    private const uint RegIBitMask = ((1u << RegIBitLength) - 1) << RegIBitIndex;

    private const int HBitIndex = RegIBitIndex + RegIBitLength;
    private const int HBitLength = 1;
    private const uint HBitMask = ((1u << HBitLength) - 1) << HBitIndex;

    private const int CrBitIndex = HBitIndex + HBitLength;
    private const int CrBitLength = 2;
    private const uint CrBitMask = ((1u << CrBitLength) - 1) << CrBitIndex;

    private const int FrameSizeBitIndex = CrBitIndex + CrBitLength;
    private const int FrameSizeBitLength = 9;
    private const uint FrameSizeBitMask = ((1u << FrameSizeBitLength) - 1) << FrameSizeBitIndex;

    private uint _flags;

    /// <summary>
    /// Creates an empty packed unwind info record.
    /// </summary>
    public Arm64PackedUnwindInfo()
    {
    }

    /// <summary>
    /// Constructs a packed unwind info from the provided raw value.
    /// </summary>
    /// <param name="flags">The raw value.</param>
    public Arm64PackedUnwindInfo(uint flags)
    {
        _flags = flags;
    }

    uint IArm64UnwindInfo.Data => _flags;

    /// <inheritdoc />
    public uint FunctionLength
    {
        get => _flags.GetFlags(FunctionLengthBitIndex, FunctionLengthBitMask);
        set => _flags = _flags.SetFlags(FunctionLengthBitIndex, FunctionLengthBitMask, value);
    }

    /// <summary>
    /// Gets the number of non-volatile FP registers (d8-15) that are saved by this function.
    /// </summary>
    public byte FPRegisterCount
    {
        get => (byte) _flags.GetFlags(RegFBitIndex, RegFBitMask);
        set => _flags = _flags.SetFlags(RegFBitIndex, RegFBitMask, value);
    }

    /// <summary>
    /// Gets the number of non-volatile integer registers (x19-x28) that are saved by this function.
    /// </summary>
    public byte IntegerRegisterCount
    {
        get => (byte) _flags.GetFlags(RegIBitIndex, RegIBitMask);
        set => _flags = _flags.SetFlags(RegIBitIndex, RegIBitMask, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the function homes the integer parameter registers (x0-x7) by storing them
    /// at the start of the function.
    /// </summary>
    public bool HomesRegisters
    {
        get => (_flags & HBitMask) != 0;
        set => _flags = _flags.SetFlags(HBitIndex, HBitMask, value ? 1u: 0u);
    }

    /// <summary>
    /// Gets or sets flags indicating whether the function includes extra instructions to set up a stack frame chain.
    /// </summary>
    public Arm64PackedUnwindInfoCR CR
    {
        get => (Arm64PackedUnwindInfoCR) _flags.GetFlags(CrBitIndex, CrBitMask);
        set => _flags = _flags.SetFlags(CrBitIndex, CrBitMask, (byte) value);
    }

    /// <summary>
    /// Gets or sets the number of bytes of the stack that is allocated for this function.
    /// </summary>
    public uint FrameSize
    {
        get => _flags.GetFlags(FrameSizeBitIndex, FrameSizeBitMask) * 16u;
        set => _flags = _flags.SetFlags(FrameSizeBitIndex, FrameSizeBitMask, value / 16u);
    }
}
