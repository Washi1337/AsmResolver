using System;
using System.Collections.Generic;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides an implementation for a PDB image that is read from an input MSF file.
/// </summary>
public class SerializedPdbImage : PdbImage
{
    private readonly MsfFile _file;

    /// <summary>
    /// Interprets a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file to read from.</param>
    public SerializedPdbImage(MsfFile file)
    {
        _file = file;

        if (InfoStream.StreamIndex >= file.Streams.Count)
            throw new BadImageFormatException("MSF file does not contain a PDB Info stream.");
        if (InfoStream.StreamIndex >= file.Streams.Count)
            throw new BadImageFormatException("MSF file does not contain a PDB Info stream.");

        InfoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());
        DbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());
    }

    internal InfoStream InfoStream
    {
        get;
    }

    internal DbiStream DbiStream
    {
        get;
    }

    /// <inheritdoc />
    protected override IList<CodeViewSymbol> GetSymbols()
    {
        var result = new List<CodeViewSymbol>();

        int index = DbiStream.SymbolRecordStreamIndex;
        if (index >= _file.Streams.Count)
            return result;

        var reader = _file.Streams[DbiStream.SymbolRecordStreamIndex].CreateReader();
        while (reader.CanRead(sizeof(ushort) * 2))
        {
            result.Add(CodeViewSymbol.FromReader(ref reader));
        }

        return result;
    }
}
