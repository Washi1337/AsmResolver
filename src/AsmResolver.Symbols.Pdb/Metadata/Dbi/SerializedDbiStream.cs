using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Implements a DBI stream that pulls its data from an input stream.
/// </summary>
public class SerializedDbiStream : DbiStream
{
    private readonly uint _moduleInfoSize;
    private readonly uint _sectionContributionSize;
    private readonly uint _sectionMapSize;
    private readonly uint _sourceInfoSize;
    private readonly uint _typeServerMapSize;
    private readonly uint _optionalDebugHeaderSize;
    private readonly uint _ecSize;

    /// <summary>
    /// Parses a DBI stream from an input stream reader.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    public SerializedDbiStream(BinaryStreamReader reader)
    {
        VersionSignature = reader.ReadInt32();
        VersionHeader = (DbiStreamVersion) reader.ReadUInt32();
        Age = reader.ReadUInt32();
        GlobalStreamIndex = reader.ReadUInt16();
        BuildNumber = reader.ReadUInt16();
        PublicStreamIndex = reader.ReadUInt16();
        PdbDllVersion = reader.ReadUInt16();
        SymbolRecordStreamIndex = reader.ReadUInt16();
        PdbDllRbld = reader.ReadUInt16();

        _moduleInfoSize = reader.ReadUInt32();
        _sectionContributionSize = reader.ReadUInt32();
        _sectionMapSize = reader.ReadUInt32();
        _sourceInfoSize = reader.ReadUInt32();
        _typeServerMapSize = reader.ReadUInt32();

        MfcTypeServerIndex = reader.ReadUInt32();

        _optionalDebugHeaderSize = reader.ReadUInt32();
        _ecSize = reader.ReadUInt32();

        Attributes = (DbiAttributes) reader.ReadUInt16();
        Machine = (MachineType) reader.ReadUInt16();

        _ = reader.ReadUInt32();
    }
}
