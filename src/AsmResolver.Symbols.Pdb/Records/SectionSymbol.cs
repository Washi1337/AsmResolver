using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides information about a section within a PE file.
/// </summary>
public partial class SectionSymbol : CodeViewSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty section symbol.
    /// </summary>
    protected SectionSymbol()
    {
    }

    /// <summary>
    /// Creates a new section symbol.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    public SectionSymbol(Utf8String name)
    {
        Name = name;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Section;

    /// <summary>
    /// Gets or sets the section number within the PE file.
    /// </summary>
    public ushort SectionNumber
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the alignment of the section.
    /// </summary>
    /// <remarks>
    /// This should be a power of 2.
    /// </remarks>
    public uint Alignment
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the starting relative virtual address (RVA) of the section.
    /// </summary>
    public uint Rva
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size in bytes of the section.
    /// </summary>
    public uint Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the section flags describing the nature of the section.
    /// </summary>
    public SectionFlags Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the section.
    /// </summary>
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the name of the section.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_SECTION: [{SectionNumber:X4}] {Name}";
}
