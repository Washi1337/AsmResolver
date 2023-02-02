using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a single Module Info (MoDi) stream in a PDB image.
/// </summary>
public class ModiStream : SegmentBase
{
    private readonly LazyVariable<IReadableSegment?> _symbols;
    private readonly LazyVariable<IReadableSegment?> _c11LineInfo;
    private readonly LazyVariable<IReadableSegment?> _c13LineInfo;
    private readonly LazyVariable<IReadableSegment?> _globalReferences;

    /// <summary>
    /// Creates a new empty module info stream.
    /// </summary>
    public ModiStream()
    {
        _symbols = new LazyVariable<IReadableSegment?>(GetSymbols);
        _c11LineInfo = new LazyVariable<IReadableSegment?>(GetC11LineInfo);
        _c13LineInfo = new LazyVariable<IReadableSegment?>(GetC13LineInfo);
        _globalReferences = new LazyVariable<IReadableSegment?>(GetGlobalReferences);
    }

    /// <summary>
    /// Gets or sets the signature the stream starts with.
    /// </summary>
    /// <remarks>
    /// This value is usually set to <c>4</c> by most compilers.
    /// </remarks>
    public uint Signature
    {
        get;
        set;
    } = 4;

    /// <summary>
    /// Gets or sets the sub-stream containing the CodeView symbol records defined in this module.
    /// </summary>
    public IReadableSegment? Symbols
    {
        get => _symbols.Value;
        set => _symbols.Value = value;
    }

    /// <summary>
    /// Gets or sets the sub-stream containing the C11-style line information.
    /// </summary>
    public IReadableSegment? C11LineInfo
    {
        get => _c11LineInfo.Value;
        set => _c11LineInfo.Value = value;
    }

    /// <summary>
    /// Gets or sets the sub-stream containing the C13-style line information.
    /// </summary>
    public IReadableSegment? C13LineInfo
    {
        get => _c13LineInfo.Value;
        set => _c13LineInfo.Value = value;
    }

    /// <summary>
    /// Gets or sets the global references sub-stream.
    /// </summary>
    /// <remarks>
    /// The exact meaning of this sub-stream is not well understood.
    /// </remarks>
    public IReadableSegment? GlobalReferences
    {
        get => _globalReferences.Value;
        set => _globalReferences.Value = value;
    }

    public static ModiStream FromReader(BinaryStreamReader reader, ModuleDescriptor module)
    {
        return new SerializedModiStream(reader, module);
    }

    /// <summary>
    /// Obtains the sub-stream containing the CodeView symbol records defined in this module.
    /// </summary>
    /// <returns>The sub-stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IReadableSegment? GetSymbols() => null;

    /// <summary>
    /// Obtains the sub-stream containing the C11-style line information.
    /// </summary>
    /// <returns>The sub-stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="C11LineInfo"/> property.
    /// </remarks>
    protected virtual IReadableSegment? GetC11LineInfo() => null;

    /// <summary>
    /// Obtains the sub-stream containing the C13-style line information.
    /// </summary>
    /// <returns>The sub-stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="C13LineInfo"/> property.
    /// </remarks>
    protected virtual IReadableSegment? GetC13LineInfo() => null;

    /// <summary>
    /// Obtains the sub-stream containing the global references.
    /// </summary>
    /// <returns>The sub-stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GlobalReferences"/> property.
    /// </remarks>
    protected virtual IReadableSegment? GetGlobalReferences() => null;

    /// <inheritdoc />
    public override uint GetPhysicalSize()
    {
        return sizeof(uint)
               + (Symbols?.GetPhysicalSize() ?? 0)
               + (C11LineInfo?.GetPhysicalSize() ?? 0)
               + (C13LineInfo?.GetPhysicalSize() ?? 0)
               + (GlobalReferences?.GetPhysicalSize() ?? 0);
    }

    /// <inheritdoc />
    public override void Write(IBinaryStreamWriter writer)
    {
        writer.WriteUInt32(Signature);
        Symbols?.Write(writer);
        C11LineInfo?.Write(writer);
        C13LineInfo?.Write(writer);
        GlobalReferences?.Write(writer);
    }
}
