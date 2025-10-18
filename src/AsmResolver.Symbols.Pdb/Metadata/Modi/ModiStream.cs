using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a single Module Info (MoDi) stream in a PDB image.
/// </summary>
public partial class ModiStream : SegmentBase
{
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new empty module info stream.
    /// </summary>
    public ModiStream()
    {
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
    [LazyProperty]
    public partial IReadableSegment? Symbols
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the sub-stream containing the C11-style line information.
    /// </summary>
    [LazyProperty]
    public partial IReadableSegment? C11LineInfo
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the sub-stream containing the C13-style line information.
    /// </summary>
    [LazyProperty]
    public partial IReadableSegment? C13LineInfo
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the global references sub-stream.
    /// </summary>
    /// <remarks>
    /// The exact meaning of this sub-stream is not well understood.
    /// </remarks>
    [LazyProperty]
    public partial IReadableSegment? GlobalReferences
    {
        get;
        set;
    }

    /// <summary>
    /// Reads a Module Info stream from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="module">The module this modi stream is associated with.</param>
    /// <returns>The stream.</returns>
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
    public override void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(Signature);
        Symbols?.Write(writer);
        C11LineInfo?.Write(writer);
        C13LineInfo?.Write(writer);
        GlobalReferences?.Write(writer);
    }
}
