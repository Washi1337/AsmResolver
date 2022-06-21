using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Implements a DBI stream that pulls its data from an input stream.
/// </summary>
public class SerializedDbiStream : DbiStream
{
    private readonly BinaryStreamReader _moduleInfoReader;
    private readonly BinaryStreamReader _sectionContributionReader;
    private readonly BinaryStreamReader _sectionMapReader;
    private readonly BinaryStreamReader _sourceInfoReader;
    private readonly BinaryStreamReader _typeServerMapReader;
    private readonly BinaryStreamReader _optionalDebugHeaderReader;
    private readonly BinaryStreamReader _ecReader;

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

        uint moduleInfoSize = reader.ReadUInt32();
        uint sectionContributionSize = reader.ReadUInt32();
        uint sectionMapSize = reader.ReadUInt32();
        uint sourceInfoSize = reader.ReadUInt32();
        uint typeServerMapSize = reader.ReadUInt32();

        MfcTypeServerIndex = reader.ReadUInt32();

        uint optionalDebugHeaderSize = reader.ReadUInt32();
        uint ecSize = reader.ReadUInt32();

        Attributes = (DbiAttributes) reader.ReadUInt16();
        Machine = (MachineType) reader.ReadUInt16();

        _ = reader.ReadUInt32();

        _moduleInfoReader = reader.ForkRelative(reader.RelativeOffset, moduleInfoSize);
        reader.Offset += moduleInfoSize;
        _sectionContributionReader = reader.ForkRelative(reader.RelativeOffset, sectionContributionSize);
        reader.Offset += sectionContributionSize;
        _sectionMapReader = reader.ForkRelative(reader.RelativeOffset, sectionMapSize);
        reader.Offset += sectionMapSize;
        _sourceInfoReader = reader.ForkRelative(reader.RelativeOffset, sourceInfoSize);
        reader.Offset += sourceInfoSize;
        _typeServerMapReader = reader.ForkRelative(reader.RelativeOffset, typeServerMapSize);
        reader.Offset += typeServerMapSize;
        _ecReader = reader.ForkRelative(reader.RelativeOffset, ecSize);
        reader.Offset += ecSize;
        _optionalDebugHeaderReader = reader.ForkRelative(reader.RelativeOffset, optionalDebugHeaderSize);
        reader.Offset += optionalDebugHeaderSize;
    }

    /// <inheritdoc />
    protected override IList<ModuleDescriptor> GetModules()
    {
        var result = new List<ModuleDescriptor>();

        var reader = _moduleInfoReader.Fork();
        while (reader.CanRead(1))
            result.Add(ModuleDescriptor.FromReader(ref reader));

        return result;
    }

    /// <inheritdoc />
    protected override IList<SectionContribution> GetSectionContributions()
    {
        var result = new List<SectionContribution>();

        var reader = _sectionContributionReader.Fork();
        var version = (SectionContributionStreamVersion) reader.ReadUInt32();

        while (reader.CanRead(1))
        {
            result.Add(SectionContribution.FromReader(ref reader));
            if (version == SectionContributionStreamVersion.V2)
                reader.ReadUInt32();
        }

        return result;
    }

    /// <inheritdoc />
    protected override IList<SectionMap> GetSectionMaps()
    {
        var result = new List<SectionMap>();

        var reader = _sectionMapReader.Fork();

        ushort count = reader.ReadUInt16();
        ushort logCount = reader.ReadUInt16();

        for (int i = 0; i < count; i++)
            result.Add(SectionMap.FromReader(ref reader));

        return result;
    }

    /// <inheritdoc />
    protected override ISegment? GetTypeServerMap()
    {
        var reader = _typeServerMapReader;
        return reader.ReadSegment(reader.Length);
    }
}
