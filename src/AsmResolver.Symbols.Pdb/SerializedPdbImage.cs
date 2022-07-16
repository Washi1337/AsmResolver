using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Metadata.Tpi;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;
using AsmResolver.Symbols.Pdb.Types;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides an implementation for a PDB image that is read from an input MSF file.
/// </summary>
public class SerializedPdbImage : PdbImage
{
    private const int MinimalRequiredStreamCount = 5;
    private readonly MsfFile _file;
    private CodeViewType?[]? _types;

    /// <summary>
    /// Interprets a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file to read from.</param>
    public SerializedPdbImage(MsfFile file)
    {
        _file = file;

        if (file.Streams.Count < MinimalRequiredStreamCount)
            throw new BadImageFormatException("MSF does not contain the minimal required amount of streams.");

        InfoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());
        DbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());
        TpiStream = TpiStream.FromReader(file.Streams[TpiStream.StreamIndex].CreateReader());
     }

    internal InfoStream InfoStream
    {
        get;
    }

    internal DbiStream DbiStream
    {
        get;
    }

    internal TpiStream TpiStream
    {
        get;
    }

    [MemberNotNull(nameof(_types))]
    private void EnsureTypeArrayInitialized()
    {
        if (_types is null)
        {
            Interlocked.CompareExchange(ref _types,
                new CodeViewType?[TpiStream.TypeIndexEnd - TpiStream.TypeIndexBegin], null);
        }
    }

    public override bool TryGetTypeRecord(uint typeIndex, [NotNullWhen(true)] out CodeViewType? type)
    {
        EnsureTypeArrayInitialized();

        if (typeIndex >= TpiStream.TypeIndexBegin && typeIndex < TpiStream.TypeIndexEnd)
        {
            type = _types[typeIndex - TpiStream.TypeIndexBegin];
            if (type is null && TpiStream.TryGetTypeRecordReader(typeIndex, out var reader))
            {
                type = CodeViewType.FromReader(this, reader);
                type.TypeIndex = typeIndex;
                Interlocked.CompareExchange(ref _types[typeIndex - TpiStream.TypeIndexBegin], type, null);
            }

            type = _types[typeIndex - TpiStream.TypeIndexBegin];
            return type is not null;
        }

        type = null;
        return false;
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
            result.Add(CodeViewSymbol.FromReader(ref reader));

        return result;
    }
}
