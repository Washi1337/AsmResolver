using AsmResolver.IO;

namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Represents a single runtime function in a Structured Exception Handler (SEH) table of a portable executable
/// targeting the ARM64 architecture.
/// </summary>
public class Arm64RuntimeFunction : IRuntimeFunction, IWritable
{
    /// <summary>
    /// The size of a single entry.
    /// </summary>
    public const int EntrySize = sizeof(uint) * 2;

    /// <summary>
    /// Creates a new runtime function for the ARM64 architecture.
    /// </summary>
    /// <param name="begin">The reference to the beginning of the function.</param>
    /// <param name="unwindInfo">The unwind information associated to the function.</param>
    public Arm64RuntimeFunction(ISegmentReference begin, IArm64UnwindInfo unwindInfo)
    {
        Begin = begin;
        End = new RelativeReference(begin, (int) unwindInfo.FunctionLength * sizeof(uint));
        UnwindInfo = unwindInfo;
    }

    /// <summary>
    /// Reads a single <see cref="Arm64RuntimeFunction"/> from the provided input stream.
    /// </summary>
    /// <param name="context">The reader context.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The read function entry.</returns>
    public static Arm64RuntimeFunction FromReader(PEReaderContext context, ref BinaryStreamReader reader)
    {
        uint startRva = reader.ReadUInt32();
        uint unwindInfoRva = reader.ReadUInt32();
        var begin = context.File.GetReferenceToRva(startRva);

        if ((unwindInfoRva & 0b11) != 0)
            return new Arm64RuntimeFunction(begin, new Arm64PackedUnwindInfo(unwindInfoRva));

        var info = context.File.TryCreateReaderAtRva(unwindInfoRva, out var unwindReader)
            ? Arm64UnpackedUnwindInfo.FromReader(context, ref unwindReader)
            : new Arm64UnpackedUnwindInfo();

        return new Arm64RuntimeFunction(begin, info);
    }

    /// <inheritdoc />
    public ISegmentReference Begin
    {
        get;
        set;
    }

    /// <inheritdoc />
    public ISegmentReference End
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the unwind info associated with this runtime function.
    /// </summary>
    public IArm64UnwindInfo? UnwindInfo
    {
        get;
        set;
    }

    IUnwindInfo? IRuntimeFunction.UnwindInfo => UnwindInfo;

    /// <inheritdoc />
    public uint GetPhysicalSize() => sizeof(uint) * 2;

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(Begin.Rva);
        writer.WriteUInt32(UnwindInfo?.FieldValue ?? 0u);
    }
}
