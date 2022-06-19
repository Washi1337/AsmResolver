using System.Text;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Describes the section in the final executable file that a particular object or module is stored at.
/// </summary>
public class SectionContribution : IWritable
{
    /// <summary>
    /// Gets or sets the index of the section.
    /// </summary>
    public ushort Section
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the section that this contribution starts at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the section contribution.
    /// </summary>
    public uint Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the section flags and permissions associated to this section contribution.
    /// </summary>
    public SectionFlags Characteristics
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the module.
    /// </summary>
    public ushort ModuleIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a cyclic redundancy code that can be used to verify the data section of this contribution.
    /// </summary>
    public uint DataCrc
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a cyclic redundancy code that can be used to verify the relocation section of this contribution.
    /// </summary>
    public uint RelocCrc
    {
        get;
        set;
    }

    /// <summary>
    /// Parses a single section contribution from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed section contribution.</returns>
    public static SectionContribution FromReader(ref BinaryStreamReader reader)
    {
        var result = new SectionContribution();

        result.Section = reader.ReadUInt16();
        reader.ReadUInt16();
        result.Offset = reader.ReadUInt32();
        result.Size = reader.ReadUInt32();
        result.Characteristics = (SectionFlags) reader.ReadUInt32();
        result.ModuleIndex = reader.ReadUInt16();
        reader.ReadUInt16();
        result.DataCrc = reader.ReadUInt32();
        result.RelocCrc = reader.ReadUInt32();

        return result;
    }

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return sizeof(ushort) // Section
               + sizeof(ushort) // Padding1
               + sizeof(uint) // Offset
               + sizeof(uint) // Size
               + sizeof(uint) // Characteristics
               + sizeof(ushort) // ModuleIndex
               + sizeof(ushort) // Padding2
               + sizeof(uint) // DataCrc
               + sizeof(uint) // RelocCrc
            ;
    }

    /// <inheritdoc />
    public void Write(IBinaryStreamWriter writer)
    {
        writer.WriteUInt16(Section);
        writer.WriteUInt16(0);
        writer.WriteUInt32(Offset);
        writer.WriteUInt32(Size);
        writer.WriteUInt32((uint) Characteristics);
        writer.WriteUInt16(ModuleIndex);
        writer.WriteUInt16(0);
        writer.WriteUInt32(DataCrc);
        writer.WriteUInt32(RelocCrc);
    }
}
