using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Implements a Module Info (MoDi) stream that pulls its data from an input stream.
/// </summary>
public class SerializedModiStream : ModiStream
{
    private readonly BinaryStreamReader _reader;
    private readonly uint _symbolSize;
    private readonly uint _c11Size;
    private readonly uint _c13Size;

    /// <summary>
    /// Parses a Module Info stream from an input stream reader, and an accompanied module descriptor.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <param name="module">The module descriptor this modi stream is attached to.</param>
    public SerializedModiStream(BinaryStreamReader reader, ModuleDescriptor module)
        : this(reader, module.SymbolDataSize, module.SymbolC11DataSize, module.SymbolC13DataSize)
    {
    }

    /// <summary>
    /// Parses a Module Info stream from an input stream reader, and an accompanied sub-stream sizes.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <param name="symbolSize">The size of the symbol sub-stream.</param>
    /// <param name="c11Size">The size of the C11-style line information sub-stream.</param>
    /// <param name="c13Size">The size of the C13-style line information sub-stream.</param>
    public SerializedModiStream(BinaryStreamReader reader, uint symbolSize, uint c11Size, uint c13Size)
    {
        _reader = reader;

        _symbolSize = symbolSize >= 4
            ? symbolSize - 4
            : 0;

        _c11Size = c11Size;
        _c13Size = c13Size;

        Signature = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override IReadableSegment? GetSymbols() => _symbolSize > 0
        ? _reader.ForkRelative(sizeof(uint)).ReadSegment(_symbolSize)
        : null;

    /// <inheritdoc />
    protected override IReadableSegment? GetC11LineInfo() => _c11Size > 0
        ? _reader.ForkRelative(sizeof(uint) + _symbolSize).ReadSegment(_c11Size)
        : null;

    /// <inheritdoc />
    protected override C13LineInfoStream? GetC13LineInfo() => _c13Size > 0
        ? new SerializedC13LineInfoStream(_reader.ForkRelative(sizeof(uint) + _symbolSize + _c11Size, _c13Size))
        : null;

    /// <inheritdoc />
    protected override IReadableSegment? GetGlobalReferences()
    {
        var reader = _reader.ForkRelative(_symbolSize + _c11Size + _c13Size);

        uint size = reader.ReadUInt32();
        return size > 0
            ? reader.ReadSegment(size)
            : null;
    }
}
