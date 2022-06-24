using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Represents a reference to a single module (object file) that was linked into a program.
/// </summary>
public class ModuleDescriptor : IWritable
{
    /// <summary>
    /// Gets or sets a description of the section within the final binary which contains code
    /// and/or data from this module.
    /// </summary>
    public SectionContribution SectionContribution
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Gets or sets the attributes assigned to this module descriptor.
    /// </summary>
    public ModuleDescriptorAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the type server for this module.
    /// </summary>
    public ushort TypeServerIndex
    {
        get => (ushort) ((ushort) Attributes >> 8);
        set => Attributes = (Attributes & ~ModuleDescriptorAttributes.TsmMask) | (ModuleDescriptorAttributes) (value << 8);
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the stream that the symbols of this module.
    /// </summary>
    public ushort SymbolStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the CodeView data within the module's symbol stream.
    /// </summary>
    public uint SymbolDataSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the C11-style CodeView data within the module's symbol stream.
    /// </summary>
    public uint SymbolC11DataSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the C13-style CodeView data within the module's symbol stream.
    /// </summary>
    public uint SymbolC13DataSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of source files that contributed to this module during the compilation.
    /// </summary>
    public ushort SourceFileCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset in the names buffer of the primary translation unit.
    /// </summary>
    /// <remarks>
    /// For most compilers this value is set to zero.
    /// </remarks>
    public uint SourceFileNameIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset in the names buffer of the PDB file.
    /// </summary>
    /// <remarks>
    /// For most modules (except the special <c>* LINKER *</c> module) this value is set to zero.
    /// </remarks>
    public uint PdbFilePathNameIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    /// <remarks>
    /// This is often a full path to the object file that was passed into <c>link.exe</c> directly, or a string in the
    /// form of <c>Import:dll_name</c>
    /// </remarks>
    public Utf8String? ModuleName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the object file name.
    /// </summary>
    /// <remarks>
    /// In the case this module is linked directly passed to <c>link.exe</c>, this is the same as <see cref="ModuleName"/>.
    /// If the module comes from an archive, this is the full path to that archive.
    /// </remarks>
    public Utf8String? ObjectFileName
    {
        get;
        set;
    }

    /// <summary>
    /// Parses a single module descriptor from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>THe parsed module descriptor.</returns>
    public static ModuleDescriptor FromReader(ref BinaryStreamReader reader)
    {
        var result = new ModuleDescriptor();

        reader.ReadUInt32();
        result.SectionContribution = SectionContribution.FromReader(ref reader);
        result.Attributes = (ModuleDescriptorAttributes) reader.ReadUInt16();
        result.SymbolStreamIndex = reader.ReadUInt16();
        result.SymbolDataSize = reader.ReadUInt32();
        result.SymbolC11DataSize = reader.ReadUInt32();
        result.SymbolC13DataSize = reader.ReadUInt32();
        result.SourceFileCount = reader.ReadUInt16();
        reader.ReadUInt16();
        reader.ReadUInt32();
        result.SourceFileNameIndex = reader.ReadUInt32();
        result.PdbFilePathNameIndex = reader.ReadUInt32();
        result.ModuleName = reader.ReadUtf8String();
        result.ObjectFileName = reader.ReadUtf8String();
        reader.Align(4);

        return result;
    }

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return (sizeof(uint) // Unused1
                + SectionContribution.GetPhysicalSize() // SectionContribution
                + sizeof(ModuleDescriptorAttributes) // Attributes
                + sizeof(ushort) // SymbolStreamIndex
                + sizeof(uint) // SymbolDataSize
                + sizeof(uint) // SymbolC11DataSize
                + sizeof(uint) // SymbolC13DataSize
                + sizeof(ushort) // SourceFileCount
                + sizeof(ushort) // Padding
                + sizeof(uint) // Unused2
                + sizeof(uint) // SourceFileNameIndex
                + sizeof(uint) // PdbFilePathNameIndex
                + (uint) (ModuleName?.ByteCount ?? 0) + 1 // ModuleName
                + (uint) (ObjectFileName?.ByteCount ?? 0) + 1 // ObjectFileName
            ).Align(4);
    }

    /// <inheritdoc />
    public void Write(IBinaryStreamWriter writer)
    {
        writer.WriteUInt32(0);
        SectionContribution.Write(writer);
        writer.WriteUInt16((ushort) Attributes);
        writer.WriteUInt16(SymbolStreamIndex);
        writer.WriteUInt32(SymbolDataSize);
        writer.WriteUInt32(SymbolC11DataSize);
        writer.WriteUInt32(SymbolC13DataSize);
        writer.WriteUInt16(SourceFileCount);
        writer.WriteUInt16(0);
        writer.WriteUInt32(0);
        writer.WriteUInt32(SourceFileNameIndex);
        writer.WriteUInt32(PdbFilePathNameIndex);
        if (ModuleName is not null)
            writer.WriteBytes(ModuleName.GetBytesUnsafe());
        writer.WriteByte(0);
        if (ObjectFileName is not null)
            writer.WriteBytes(ObjectFileName.GetBytesUnsafe());
        writer.WriteByte(0);
        writer.Align(4);
    }

    /// <inheritdoc />
    public override string ToString() => ModuleName ?? ObjectFileName ?? "?";
}
