using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a single sub section in a C13 line info sub stream.
/// </summary>
public abstract class C13SubSection : IWritable
{
    /// <summary>
    /// Initializes an empty C13 sub section.
    /// </summary>
    /// <param name="type">The type of section.</param>
    protected C13SubSection(C13SubSectionType type)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the type of data the section contains.
    /// </summary>
    public C13SubSectionType Type
    {
        get;
    }

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return sizeof(C13SubSectionType) + sizeof(uint) + GetContentsLength();
    }

    /// <summary>
    /// Measures the size in bytes of the contents of the section, excluding the section header.
    /// </summary>
    /// <returns>The size in bytes.</returns>
    protected abstract uint GetContentsLength();

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32((uint) Type);
        writer.WriteUInt32(GetContentsLength());
        WriteContents(writer);
    }

    /// <summary>
    /// Writes the contents, excluding the section header, to the provided output stream.
    /// </summary>
    /// <param name="writer">The output stream.</param>
    protected abstract void WriteContents(BinaryStreamWriter writer);

    /// <inheritdoc />
    public override string ToString() => Type.ToString();
}
