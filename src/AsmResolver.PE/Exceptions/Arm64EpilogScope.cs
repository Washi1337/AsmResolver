using AsmResolver.IO;

namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Represents a single scope in an ARM64 .xdata unwind record.
/// </summary>
public struct Arm64EpilogScope : IWritable
{
    /// <summary>
    /// The size of a single epilog scope record.
    /// </summary>
    public const uint Size = sizeof(uint);

    private const int StartOffsetBitIndex = 0;
    private const int StartOffsetBitLength = 18;
    private const uint StartOffsetBitMask = ((1u << StartOffsetBitLength) - 1) << StartOffsetBitIndex;

    private const int StartIndexBitIndex = 22;
    private const int StartIndexBitLength = 10;
    private const uint StartIndexBitMask = ((1u << StartIndexBitLength) - 1) << StartIndexBitIndex;

    private uint _value;

    /// <summary>
    /// Creates a new epilog scope from the provided raw value.
    /// </summary>
    /// <param name="value">The raw value.</param>
    public Arm64EpilogScope(uint value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new epilog scope from the provided starting offset and index.
    /// </summary>
    /// <param name="startOffset">The start offset of the epilog, relative to the start of the function.</param>
    /// <param name="startIndex">The index of the first unwind code to execute.</param>
    public Arm64EpilogScope(uint startOffset, ushort startIndex)
    {
        StartOffset = startOffset;
        StartIndex = startIndex;
    }

    /// <summary>
    /// Gets or sets the start offset of the epilog, relative to the start of the function.
    /// </summary>
    public uint StartOffset
    {
        get => _value.GetFlags(StartOffsetBitIndex, StartOffsetBitMask) * sizeof(uint);
        set => _value =_value.SetFlags(StartOffsetBitIndex, StartOffsetBitMask, value / sizeof(uint));
    }

    /// <summary>
    /// Gets or sets the index of the first unwind code to execute.
    /// </summary>
    public ushort StartIndex
    {
        get => (ushort) _value.GetFlags(StartIndexBitIndex, StartIndexBitMask);
        set => _value =_value.SetFlags(StartIndexBitIndex, StartIndexBitMask, value);
    }

    /// <summary>
    /// Reads a single epilog scope from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The epilog scope.</returns>
    public static Arm64EpilogScope FromReader(ref BinaryStreamReader reader) => new(reader.ReadUInt32());

    /// <inheritdoc />
    public uint GetPhysicalSize() => sizeof(uint);

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer) => writer.WriteUInt32(_value);
}
