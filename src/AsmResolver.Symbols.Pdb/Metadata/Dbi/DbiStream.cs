using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Represents the DBI Stream (also known as the Debug Information stream).
/// </summary>
public partial class DbiStream : SegmentBase
{
    /// <summary>
    /// Gets the default fixed MSF stream index for the DBI stream.
    /// </summary>
    public const int StreamIndex = 3;

    private IList<ModuleDescriptor>? _modules;
    private IList<SectionContribution>? _sectionContributions;
    private IList<SectionMap>? _sectionMaps;
    private IList<SourceFileCollection>? _sourceFiles;
    private IList<ushort>? _extraStreamIndices;

    /// <summary>
    /// Creates a new empty DBI stream.
    /// </summary>
    public DbiStream()
    {
        IsNewVersionFormat = true;
    }

    /// <summary>
    /// Gets or sets the version signature assigned to the DBI stream.
    /// </summary>
    /// <remarks>
    /// This value should always be -1 for valid PDB files.
    /// </remarks>
    public int VersionSignature
    {
        get;
        set;
    } = -1;

    /// <summary>
    /// Gets or sets the version number of the DBI header.
    /// </summary>
    /// <remarks>
    /// Modern tooling only recognize the VC7.0 file format.
    /// </remarks>
    public DbiStreamVersion VersionHeader
    {
        get;
        set;
    } = DbiStreamVersion.V70;

    /// <summary>
    /// Gets or sets the number of times the DBI stream has been written.
    /// </summary>
    public uint Age
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Gets or sets the MSF stream index of the Global Symbol Stream.
    /// </summary>
    public ushort GlobalStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a bitfield containing the major and minor version of the toolchain that was used to build the program.
    /// </summary>
    public ushort BuildNumber
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating that the DBI stream is using the new file format (NewDBI).
    /// </summary>
    public bool IsNewVersionFormat
    {
        get => (BuildNumber & 0x8000) != 0;
        set => BuildNumber = (ushort) ((BuildNumber & ~0x8000) | (value ? 0x8000 : 0));
    }

    /// <summary>
    /// Gets or sets the major version of the toolchain that was used to build the program.
    /// </summary>
    public byte BuildMajorVersion
    {
        get => (byte) ((BuildNumber >> 8) & 0x7F);
        set => BuildNumber = (ushort) ((BuildNumber & ~0x7F00) | (value << 8));
    }

    /// <summary>
    /// Gets or sets the minor version of the toolchain that was used to build the program.
    /// </summary>
    public byte BuildMinorVersion
    {
        get => (byte) (BuildNumber & 0xFF);
        set => BuildNumber = (ushort) ((BuildNumber & ~0x00FF) | value);
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the Public Symbol Stream.
    /// </summary>
    public ushort PublicStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the version number of mspdbXXXX.dll that was used to produce this PDB file.
    /// </summary>
    public ushort PdbDllVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the Symbol Record Stream.
    /// </summary>
    public ushort SymbolRecordStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Unknown.
    /// </summary>
    public ushort PdbDllRbld
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the MFC type server.
    /// </summary>
    public uint MfcTypeServerIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes associated to the DBI stream.
    /// </summary>
    public DbiAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the machine type the program was compiled for.
    /// </summary>
    public MachineType Machine
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of modules (object files) that were linked together into the program.
    /// </summary>
    public IList<ModuleDescriptor> Modules
    {
        get
        {
            if (_modules is null)
                Interlocked.CompareExchange(ref _modules, GetModules(), null);
            return _modules;
        }
    }

    /// <summary>
    /// Gets a collection of section contributions describing the layout of the sections of the final executable file.
    /// </summary>
    public IList<SectionContribution> SectionContributions
    {
        get
        {
            if (_sectionContributions is null)
                Interlocked.CompareExchange(ref _sectionContributions, GetSectionContributions(), null);
            return _sectionContributions;
        }
    }

    /// <summary>
    /// Gets a collection of section mappings stored in the section mapping sub stream.
    /// </summary>
    /// <remarks>
    /// The exact purpose of this is unknown, but it seems to be always containing a copy of the sections in the final
    /// executable file.
    /// </remarks>
    public IList<SectionMap> SectionMaps
    {
        get
        {
            if (_sectionMaps is null)
                Interlocked.CompareExchange(ref _sectionMaps, GetSectionMaps(), null);
            return _sectionMaps;
        }
    }

    /// <summary>
    /// Gets or sets the contents of the type server map sub stream.
    /// </summary>
    /// <remarks>
    /// The exact purpose and layout of this sub stream is unknown, hence this property exposes the stream as
    /// a raw segment.
    /// </remarks>
    [LazyProperty]
    public partial ISegment? TypeServerMapStream
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the contents of the Edit-and-Continue sub stream.
    /// </summary>
    /// <remarks>
    /// The exact purpose and layout of this sub stream is unknown, hence this property exposes the stream as
    /// a raw segment.
    /// </remarks>
    [LazyProperty]
    public partial ISegment? ECStream
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of source files assigned to each module in <see cref="Modules"/>.
    /// </summary>
    /// <remarks>
    /// Every collection of source files within this list corresponds to exactly the module within <see cref="Modules"/>
    /// at the same index. For example, the first source file list in this collection is the source file list of the
    /// first module.
    /// </remarks>
    public IList<SourceFileCollection> SourceFiles
    {
        get
        {
            if (_sourceFiles is null)
                Interlocked.CompareExchange(ref _sourceFiles, GetSourceFiles(), null);
            return _sourceFiles;
        }
    }

    /// <summary>
    /// Gets a collection of indices referring to additional debug streams in the MSF file.
    /// </summary>
    public IList<ushort> ExtraStreamIndices
    {
        get
        {
            if (_extraStreamIndices is null)
                Interlocked.CompareExchange(ref _extraStreamIndices, GetExtraStreamIndices(), null);
            return _extraStreamIndices;
        }
    }

    /// <summary>
    /// Reads a single DBI stream from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed DBI stream.</returns>
    public static DbiStream FromReader(BinaryStreamReader reader) => new SerializedDbiStream(reader);

    /// <summary>
    /// Obtains the list of module descriptors.
    /// </summary>
    /// <returns>The module descriptors</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Modules"/> property.
    /// </remarks>
    protected virtual IList<ModuleDescriptor> GetModules() => new List<ModuleDescriptor>();

    /// <summary>
    /// Obtains the list of section contributions.
    /// </summary>
    /// <returns>The section contributions.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SectionContributions"/> property.
    /// </remarks>
    protected virtual IList<SectionContribution> GetSectionContributions() => new List<SectionContribution>();

    /// <summary>
    /// Obtains the list of section maps.
    /// </summary>
    /// <returns>The section maps.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SectionMaps"/> property.
    /// </remarks>
    protected virtual IList<SectionMap> GetSectionMaps() => new List<SectionMap>();

    /// <summary>
    /// Obtains the contents of the type server map sub stream.
    /// </summary>
    /// <returns>The contents of the sub stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="TypeServerMapStream"/> property.
    /// </remarks>
    protected virtual ISegment? GetTypeServerMapStream() => null;

    /// <summary>
    /// Obtains the contents of the EC sub stream.
    /// </summary>
    /// <returns>The contents of the sub stream.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ECStream"/> property.
    /// </remarks>
    protected virtual ISegment? GetECStream() => null;

    /// <summary>
    /// Obtains a table that assigns a list of source files to every module referenced in the PDB file.
    /// </summary>
    /// <returns>The table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SourceFiles"/> property.
    /// </remarks>
    protected virtual IList<SourceFileCollection> GetSourceFiles() => new List<SourceFileCollection>();

    /// <summary>
    /// Obtains the list of indices referring to additional debug streams in the MSF file.
    /// </summary>
    /// <returns>The list of indices.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ExtraStreamIndices"/> property.
    /// </remarks>
    protected virtual IList<ushort> GetExtraStreamIndices() => new List<ushort>();

    /// <inheritdoc />
    public override uint GetPhysicalSize()
    {
        return GetHeaderSize()
               + GetModuleStreamSize()
               + GetSectionContributionStreamSize()
               + GetSectionMapStreamSize()
               + GetSourceInfoStreamSize()
               + GetTypeServerMapStreamSize()
               + GetECStreamSize()
               + GetOptionalDebugStreamSize()
            ;
    }

    private static uint GetHeaderSize()
    {
        return sizeof(int) // VersionSignature
               + sizeof(DbiStreamVersion) // VersionHeader
               + sizeof(uint) // Age
               + sizeof(ushort) // GlobalStreamIndex
               + sizeof(ushort) // BuildNumber
               + sizeof(ushort) // PublicStreamIndex
               + sizeof(ushort) // PdbDllVersion
               + sizeof(ushort) // SymbolRecordStreamIndex
               + sizeof(ushort) // PdbDllRbld
               + sizeof(uint) // ModuleInfoSize
               + sizeof(uint) // SectionContributionSize
               + sizeof(uint) // SectionMapSize
               + sizeof(uint) // SourceInfoSize
               + sizeof(uint) // TypeServerMapSize
               + sizeof(uint) // MfcTypeServerIndex
               + sizeof(uint) // OptionalDebugStreamSize
               + sizeof(uint) // ECStreamSize
               + sizeof(DbiAttributes) // Attributes
               + sizeof(MachineType) // MachineType
               + sizeof(uint) // Padding
            ;
    }

    private uint GetModuleStreamSize()
    {
        return ((uint) Modules.Sum(m => m.GetPhysicalSize())).Align(sizeof(uint));
    }

    private uint GetSectionContributionStreamSize()
    {
        return sizeof(uint) // version
               + SectionContribution.EntrySize * (uint) SectionContributions.Count;
    }

    private uint GetSectionMapStreamSize()
    {
        return sizeof(ushort) // Count
               + sizeof(ushort) // LogCount
               + SectionMap.EntrySize * (uint) SectionMaps.Count;
    }

    private uint GetSourceInfoStreamSize()
    {
        uint totalFileCount = 0;
        uint nameBufferSize = 0;
        var stringOffsets = new Dictionary<Utf8String, uint>();

        // Simulate the construction of name buffer
        for (int i = 0; i < SourceFiles.Count; i++)
        {
            var collection = SourceFiles[i];
            totalFileCount += (uint) collection.Count;

            for (int j = 0; j < collection.Count; j++)
            {
                // If name is not added yet, "append" to the name buffer.
                var name = collection[j];
                if (!stringOffsets.ContainsKey(name))
                {
                    stringOffsets[name] = nameBufferSize;
                    nameBufferSize += (uint) name.ByteCount + 1u;
                }
            }
        }

        return (sizeof(ushort) // ModuleCount
                + sizeof(ushort) // SourceFileCount
                + sizeof(ushort) * (uint) SourceFiles.Count // ModuleIndices
                + sizeof(ushort) * (uint) SourceFiles.Count // SourceFileCounts
                + sizeof(uint) * totalFileCount // NameOffsets
                + nameBufferSize // NameBuffer
            ).Align(4);
    }

    private uint GetTypeServerMapStreamSize()
    {
        return TypeServerMapStream?.GetPhysicalSize().Align(sizeof(uint)) ?? 0u;
    }

    private uint GetOptionalDebugStreamSize()
    {
        return (uint) (ExtraStreamIndices.Count * sizeof(ushort));
    }

    private uint GetECStreamSize()
    {
        return ECStream?.GetPhysicalSize() ?? 0u;
    }

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        WriteHeader(writer);
        WriteModuleStream(writer);
        WriteSectionContributionStream(writer);
        WriteSectionMapStream(writer);
        WriteSourceInfoStream(writer);
        WriteTypeServerMapStream(writer);
        WriteECStream(writer);
        WriteOptionalDebugStream(writer);
    }

    private void WriteHeader(BinaryStreamWriter writer)
    {
        writer.WriteInt32(VersionSignature);
        writer.WriteUInt32((uint) VersionHeader);
        writer.WriteUInt32(Age);
        writer.WriteUInt16(GlobalStreamIndex);
        writer.WriteUInt16(BuildNumber);
        writer.WriteUInt16(PublicStreamIndex);
        writer.WriteUInt16(PdbDllVersion);
        writer.WriteUInt16(SymbolRecordStreamIndex);
        writer.WriteUInt16(PdbDllRbld);

        writer.WriteUInt32(GetModuleStreamSize());
        writer.WriteUInt32(GetSectionContributionStreamSize());
        writer.WriteUInt32(GetSectionMapStreamSize());
        writer.WriteUInt32(GetSourceInfoStreamSize());
        writer.WriteUInt32(GetTypeServerMapStreamSize());

        writer.WriteUInt32(MfcTypeServerIndex);

        writer.WriteUInt32(GetOptionalDebugStreamSize());
        writer.WriteUInt32(GetECStreamSize());

        writer.WriteUInt16((ushort) Attributes);
        writer.WriteUInt16((ushort) Machine);

        writer.WriteUInt32(0);
    }

    private void WriteModuleStream(BinaryStreamWriter writer)
    {
        var modules = Modules;
        for (int i = 0; i < modules.Count; i++)
            modules[i].Write(writer);

        writer.Align(sizeof(uint));
    }

    private void WriteSectionContributionStream(BinaryStreamWriter writer)
    {
        // TODO: make customizable
        writer.WriteUInt32((uint) SectionContributionStreamVersion.Ver60);

        var contributions = SectionContributions;
        for (int i = 0; i < contributions.Count; i++)
            contributions[i].Write(writer);

        writer.Align(sizeof(uint));
    }

    private void WriteSectionMapStream(BinaryStreamWriter writer)
    {
        var maps = SectionMaps;

        // Count and LogCount.
        writer.WriteUInt16((ushort) maps.Count);
        writer.WriteUInt16((ushort) maps.Count);

        // Entries.
        for (int i = 0; i < maps.Count; i++)
            maps[i].Write(writer);

        writer.Align(sizeof(uint));
    }

    private void WriteSourceInfoStream(BinaryStreamWriter writer)
    {
        var sourceFiles = SourceFiles;
        int totalFileCount = sourceFiles.Sum(x => x.Count);

        // Module and total file count (truncated to 16 bits)
        writer.WriteUInt16((ushort) (sourceFiles.Count & 0xFFFF));
        writer.WriteUInt16((ushort) (totalFileCount & 0xFFFF));

        // Module indices. Unsure if this is correct, but this array does not seem to be really used by the ref impl.
        for (ushort i = 0; i < sourceFiles.Count; i++)
            writer.WriteUInt16(i);

        // Source file counts.
        for (int i = 0; i < sourceFiles.Count; i++)
            writer.WriteUInt16((ushort) sourceFiles[i].Count);

        // Build up string buffer and name offset table.
        using var stringBuffer = new MemoryStream();
        var stringWriter = new BinaryStreamWriter(stringBuffer);
        var stringOffsets = new Dictionary<Utf8String, uint>();

        for (int i = 0; i < sourceFiles.Count; i++)
        {
            var collection = sourceFiles[i];
            for (int j = 0; j < collection.Count; j++)
            {
                // If not present already, append to string buffer.
                var name = collection[j];
                if (!stringOffsets.TryGetValue(name, out uint offset))
                {
                    offset = (uint) stringWriter.Offset;
                    stringOffsets[name] = offset;
                    stringWriter.WriteBytes(name.GetBytesUnsafe());
                    stringWriter.WriteByte(0);
                }

                // Write name offset
                writer.WriteUInt32(offset);
            }
        }

        // Write string buffer.
        writer.WriteBytes(stringBuffer.ToArray());

        writer.Align(sizeof(uint));
    }

    private void WriteTypeServerMapStream(BinaryStreamWriter writer)
    {
        TypeServerMapStream?.Write(writer);
        writer.Align(sizeof(uint));
    }

    private void WriteOptionalDebugStream(BinaryStreamWriter writer)
    {
        var extraIndices = ExtraStreamIndices;

        for (int i = 0; i < extraIndices.Count; i++)
            writer.WriteUInt16(extraIndices[i]);
    }

    private void WriteECStream(BinaryStreamWriter writer) => ECStream?.Write(writer);
}
