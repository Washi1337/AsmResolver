using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="Compile3Symbol"/> that is read from a PDB image.
/// </summary>
public class SerializedCompile3Symbol : Compile3Symbol
{
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a compile symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedCompile3Symbol(BinaryStreamReader reader)
    {
        uint flags = reader.ReadUInt32();
        Language = (SourceLanguage) (flags & 0xF);
        Attributes = (CompileAttributes) (flags >> 8);
        Machine = (CpuType) reader.ReadUInt16();
        FrontEndMajorVersion = reader.ReadUInt16();
        FrontEndMinorVersion = reader.ReadUInt16();
        FrontEndBuildVersion = reader.ReadUInt16();
        FrontEndQfeVersion = reader.ReadUInt16();
        BackEndMajorVersion = reader.ReadUInt16();
        BackEndMinorVersion = reader.ReadUInt16();
        BackEndBuildVersion = reader.ReadUInt16();
        BackEndQfeVersion = reader.ReadUInt16();
        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetCompilerVersion() => _nameReader.Fork().ReadUtf8String();
}
