using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Represents an unpacked ARM64 .xdata unwind info record associated to a RUNTIME_FUNCTION of an ARM64 executable file.
/// </summary>
public class Arm64XDataUnwindInfo : SegmentBase, IArm64UnwindInfo
{
    private const int FunctionLengthBitIndex = 0;
    private const int FunctionLengthBitLength = 17;
    private const uint FunctionLengthBitMask = ((1u << FunctionLengthBitLength) - 1) << FunctionLengthBitIndex;

    private const int VersionBitIndex = FunctionLengthBitIndex + FunctionLengthBitLength;
    private const int VersionBitLength = 2;
    private const uint VersionBitMask = ((1u << VersionBitLength) - 1) << VersionBitIndex;

    private const int XBitIndex = VersionBitIndex + VersionBitLength;
    private const int XBitLength = 1;
    private const uint XBitMask = ((1u << XBitLength) - 1) << XBitIndex;

    private const int EBitIndex = XBitIndex + XBitLength;
    private const int EBitLength = 1;
    private const uint EBitMask = ((1u << EBitLength) - 1) << EBitIndex;

    private const int EpilogCountBitIndex = EBitIndex + EBitLength;
    private const int EpilogCountBitLength = 5;
    private const uint EpilogCountBitMask = ((1u << EpilogCountBitLength) - 1) << EpilogCountBitIndex;

    private const int CodeWordsBitIndex = EpilogCountBitIndex + EpilogCountBitLength;
    private const int CodeWordsBitLength = 5;
    private const uint CodeWordsBitMask = ((1u << CodeWordsBitIndex) - 1) << CodeWordsBitLength;

    private uint _header;
    private ushort _extendedEpilogCount;
    private byte _extendedCodeWords;

    uint IArm64UnwindInfo.Data => Rva;

    /// <inheritdoc />
    public uint FunctionLength
    {
        get => _header.GetFlags(FunctionLengthBitIndex, FunctionLengthBitMask);
        set => _header = _header.SetFlags(FunctionLengthBitIndex, FunctionLengthBitMask, value);
    }

    /// <summary>
    /// Gets or sets the version of the xdata unwind info format.
    /// </summary>
    /// <remarks>
    /// Currently, only version 0 is defined. Values of 1-3 are not permitted.
    /// </remarks>
    public byte Version
    {
        get => (byte) _header.GetFlags(VersionBitIndex, VersionBitMask);
        set => _header = _header.SetFlags(VersionBitIndex, VersionBitMask, value);
    }

    private bool X
    {
        get => (_header & XBitMask) != 0;
        set => _header = _header.SetFlags(XBitIndex, XBitMask, value ? 1u : 0u);
    }

    private bool E
    {
        get => (_header & EBitMask) != 0;
        set => _header = _header.SetFlags(EBitIndex, EBitMask, value ? 1u : 0u);
    }

    private byte EpilogCount
    {
        get => (byte) _header.GetFlags(EpilogCountBitIndex, EpilogCountBitMask);
        set => _header = _header.SetFlags(EpilogCountBitIndex, EpilogCountBitMask, value);
    }

    private byte CodeWordCount
    {
        get => (byte) _header.GetFlags(CodeWordsBitIndex, CodeWordsBitMask);
        set => _header = _header.SetFlags(CodeWordsBitIndex, CodeWordsBitMask, value);
    }

    private bool HasExtendedHeader => EpilogCount == 0 && CodeWordCount == 0;

    /// <summary>
    /// Gets a collection of epilog scopes defined in this xdata unwind info.
    /// </summary>
    public IList<Arm64EpilogScope> EpilogScopes
    {
        get;
    } = new List<Arm64EpilogScope>();

    /// <summary>
    /// Gets or sets the raw bytes of the unwind codes.
    /// </summary>
    public byte[] UnwindCodes
    {
        get;
        set;
    } = ArrayShim.Empty<byte>();

    /// <summary>
    /// When available, gets or sets a reference to optional exception handler data.
    /// </summary>
    public ISegmentReference ExceptionHandlerData
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Reads an .xdata unwind information record from the provided input stream.
    /// </summary>
    /// <param name="context">The reader context.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The read unwind record.</returns>
    public static Arm64XDataUnwindInfo FromReader(PEReaderContext context, ref BinaryStreamReader reader)
    {
        var result = new Arm64XDataUnwindInfo();
        result.UpdateOffsets(context.GetRelocation(reader.Offset, reader.Rva));

        result._header = reader.ReadUInt32();

        int epilogCount = result.E ? result.EpilogCount : 0;
        int wordCount = result.CodeWordCount;

        if (result.HasExtendedHeader)
        {
            epilogCount = result._extendedEpilogCount = reader.ReadUInt16();
            wordCount = result._extendedCodeWords = reader.ReadByte();
            _ = reader.ReadByte(); // Reserved
        }

        for (int i = 0; i < epilogCount; i++)
            result.EpilogScopes.Add(Arm64EpilogScope.FromReader(ref reader));

        result.UnwindCodes = reader.ReadBytes(wordCount * sizeof(uint));

        if (result.X)
            result.ExceptionHandlerData = context.File.GetReferenceToRva(reader.ReadUInt32());

        return result;
    }

    /// <inheritdoc />
    public override void UpdateOffsets(in RelocationParameters parameters)
    {
        uint actualWordCount = ((uint) UnwindCodes.Length).Align(sizeof(uint)) / sizeof(uint);
        int actualEpilogCount = EpilogScopes.Count;

        if (actualWordCount < (1 << CodeWordsBitLength) && actualEpilogCount < (1 << EpilogCountBitLength))
        {
            CodeWordCount = (byte) actualWordCount;
            EpilogCount = (byte) actualEpilogCount;
            _extendedEpilogCount = 0;
            _extendedCodeWords = 0;
        }
        else
        {
            CodeWordCount = 0;
            EpilogCount = 0;
            _extendedEpilogCount = (ushort) actualWordCount;
            _extendedCodeWords = (byte) actualEpilogCount;
        }

        X = ExceptionHandlerData != SegmentReference.Null;

        base.UpdateOffsets(in parameters);
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize()
    {
        return sizeof(uint) // header
            + (HasExtendedHeader ? sizeof(uint) : 0u) // Extended header
            + Arm64EpilogScope.Size * (uint) EpilogScopes.Count // Epilog Scopes
            + ((uint) UnwindCodes.Length).Align(sizeof(uint)) // Unwind Codes
            + (X ? sizeof(uint) : 0u) // ExceptionHandler RVA
            ;
    }

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(_header);

        if (HasExtendedHeader)
        {
            writer.WriteUInt16(_extendedEpilogCount);
            writer.WriteByte(_extendedCodeWords);
            writer.WriteByte(0);
        }

        foreach (var scope in EpilogScopes)
            scope.Write(writer);

        writer.WriteBytes(UnwindCodes);
        writer.Align(sizeof(uint));

        if (X)
            writer.WriteUInt32(ExceptionHandlerData.Rva);
    }
}
