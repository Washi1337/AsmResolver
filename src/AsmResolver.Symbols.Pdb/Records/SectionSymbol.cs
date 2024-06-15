using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides information about a section within a PE file.
/// </summary>
public class SectionSymbol : CodeViewSymbol
{
    private readonly LazyVariable<SectionSymbol, Utf8String?> _name;

    /// <summary>
    /// Initializes an empty section symbol.
    /// </summary>
    protected SectionSymbol()
    {
        _name = new LazyVariable<SectionSymbol, Utf8String?>(x => x.GetName());
    }

    /// <summary>
    /// Creates a new section symbol.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    public SectionSymbol(Utf8String name)
    {
        _name = new LazyVariable<SectionSymbol, Utf8String?>(name);
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
    public Utf8String? Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
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
