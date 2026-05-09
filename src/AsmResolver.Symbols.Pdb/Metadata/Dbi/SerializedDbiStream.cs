using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Implements a DBI stream that pulls its data from an input stream.
/// </summary>
public class SerializedDbiStream : DbiStream
{
    private readonly BinaryStreamReaderState _moduleInfoReaderState;
    private readonly BinaryStreamReaderState _sectionContributionReaderState;
    private readonly BinaryStreamReaderState _sectionMapReaderState;
    private readonly BinaryStreamReaderState _sourceInfoReaderState;
    private readonly BinaryStreamReaderState _typeServerMapReaderState;
    private readonly BinaryStreamReaderState _optionalDebugHeaderReaderState;
    private readonly BinaryStreamReaderState _ecReaderState;

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
        if (!IsNewVersionFormat)
            throw new NotSupportedException("The DBI stream uses the legacy file format, which is not supported.");

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

        var state = reader.GetState();
        _moduleInfoReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, moduleInfoSize);
        state.CurrentOffset += moduleInfoSize;
        _sectionContributionReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, sectionContributionSize);
        state.CurrentOffset += sectionContributionSize;
        _sectionMapReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, sectionMapSize);
        state.CurrentOffset += sectionMapSize;
        _sourceInfoReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, sourceInfoSize);
        state.CurrentOffset += sourceInfoSize;
        _typeServerMapReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, typeServerMapSize);
        state.CurrentOffset += typeServerMapSize;
        _ecReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, ecSize);
        state.CurrentOffset += ecSize;
        _optionalDebugHeaderReaderState = state.WithRelativeOffsetSize(reader.RelativeOffset, optionalDebugHeaderSize);
        state.CurrentOffset += optionalDebugHeaderSize;
    }

    /// <inheritdoc />
    protected override IList<ModuleDescriptor> GetModules()
    {
        var result = new List<ModuleDescriptor>();

        var reader = _moduleInfoReaderState.CreateReader();
        while (reader.CanRead(1))
            result.Add(ModuleDescriptor.FromReader(ref reader));

        return result;
    }

    /// <inheritdoc />
    protected override IList<SectionContribution> GetSectionContributions()
    {
        var result = new List<SectionContribution>();

        var reader = _sectionContributionReaderState.CreateReader();
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
        var reader = _sectionMapReaderState.CreateReader();

        ushort count = reader.ReadUInt16();
        ushort logCount = reader.ReadUInt16();

        var result = new List<SectionMap>(count);
        for (int i = 0; i < count; i++)
            result.Add(SectionMap.FromReader(ref reader));

        return result;
    }

    /// <inheritdoc />
    protected override ISegment? GetTypeServerMapStream()
    {
        var reader = _typeServerMapReaderState.CreateReader();
        return reader.Length != 0
            ? reader.ReadSegment(reader.Length)
            : null;
    }

    /// <inheritdoc />
    protected override ISegment? GetECStream()
    {
        var reader = _ecReaderState.CreateReader();
        return reader.Length != 0
            ? reader.ReadSegment(reader.Length)
            : null;
    }

    /// <inheritdoc />
    protected override IList<SourceFileCollection> GetSourceFiles()
    {
        var reader = _sourceInfoReaderState.CreateReader();

        ushort moduleCount = reader.ReadUInt16();
        ushort sourceFileCount = reader.ReadUInt16();

        // Read module indices.
        ushort[] moduleIndices = new ushort[moduleCount];
        for (int i = 0; i < moduleCount; i++)
            moduleIndices[i] = reader.ReadUInt16();

        // Read module source file counts.
        int actualFileCount = 0;
        ushort[] moduleFileCounts = new ushort[moduleCount];
        for (int i = 0; i < moduleCount; i++)
        {
            ushort count = reader.ReadUInt16();
            moduleFileCounts[i] = count;
            actualFileCount += count;
        }

        // Scope on the name buffer.
        var stringReaderBuffer = reader.ForkRelative((uint) (reader.RelativeOffset + actualFileCount * sizeof(uint)));

        // Construct source file lists.
        var result = new List<SourceFileCollection>(moduleCount);
        for (int i = 0; i < moduleCount; i++)
        {
            var files = new SourceFileCollection(moduleIndices[i]);
            ushort fileCount = moduleFileCounts[i];

            // Read all file paths for this module.
            for (int j = 0; j < fileCount; j++)
            {
                uint nameOffset = reader.ReadUInt32();
                var nameReader = stringReaderBuffer.ForkRelative(nameOffset);
                files.Add(nameReader.ReadUtf8String());
            }

            result.Add(files);
        }

        return result;
    }

    /// <inheritdoc />
    protected override IList<ushort> GetExtraStreamIndices()
    {
        var reader = _optionalDebugHeaderReaderState.CreateReader();

        var result = new List<ushort>((int) (reader.Length / sizeof(ushort)));

        while (reader.CanRead(sizeof(ushort)))
            result.Add(reader.ReadUInt16());

        return result;
    }
}
