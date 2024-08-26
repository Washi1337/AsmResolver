using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a custom or unsupported C13 sub section.
/// </summary>
public class CustomC13SubSection : C13SubSection
{
    /// <summary>
    /// Creates a new custom sub section.
    /// </summary>
    /// <param name="type">The type of section.</param>
    /// <param name="contents">The contents of the section.</param>
    public CustomC13SubSection(C13SubSectionType type, IReadableSegment contents)
        : base(type)
    {
        Contents = contents;
    }

    /// <summary>
    /// Gets or sets the contents of the section.
    /// </summary>
    public IReadableSegment Contents
    {
        get;
        set;
    }

    /// <inheritdoc />
    protected override uint GetContentsLength() => Contents.GetPhysicalSize();

    /// <inheritdoc />
    protected override void WriteContents(BinaryStreamWriter writer) => Contents.Write(writer);
}
