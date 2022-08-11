using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Metadata.Tpi;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides an implementation for a PDB image that is read from an input MSF file.
/// </summary>
public class SerializedPdbImage : PdbImage
{
    private const int MinimalRequiredStreamCount = 5;
    private readonly MsfFile _file;
    private CodeViewLeaf?[]? _leaves;

    /// <summary>
    /// Interprets a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file to read from.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    public SerializedPdbImage(MsfFile file, PdbReaderParameters readerParameters)
    {
        _file = file;

        if (file.Streams.Count < MinimalRequiredStreamCount)
            throw new BadImageFormatException("MSF does not contain the minimal required amount of streams.");

        InfoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());
        DbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());
        TpiStream = TpiStream.FromReader(file.Streams[TpiStream.StreamIndex].CreateReader());

        ReaderContext = new PdbReaderContext(this, readerParameters);
    }

    internal PdbReaderContext ReaderContext
    {
        get;
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

    [MemberNotNull(nameof(_leaves))]
    private void EnsureTypeArrayInitialized()
    {
        if (_leaves is null)
        {
            Interlocked.CompareExchange(ref _leaves,
                new CodeViewLeaf?[TpiStream.TypeIndexEnd - TpiStream.TypeIndexBegin], null);
        }
    }

    /// <inheritdoc />
    public override bool TryGetLeafRecord(uint typeIndex, [NotNullWhen(true)] out CodeViewLeaf? type)
    {
        if (typeIndex < TpiStream.TypeIndexBegin)
            return base.TryGetLeafRecord(typeIndex, out type);

        EnsureTypeArrayInitialized();

        if (typeIndex >= TpiStream.TypeIndexBegin && typeIndex < TpiStream.TypeIndexEnd)
        {
            type = _leaves[typeIndex - TpiStream.TypeIndexBegin];
            if (type is null && TpiStream.TryGetLeafRecordReader(typeIndex, out var reader))
            {
                type = CodeViewLeaf.FromReader(ReaderContext, typeIndex, ref reader);
                Interlocked.CompareExchange(ref _leaves[typeIndex - TpiStream.TypeIndexBegin], type, null);
            }

            type = _leaves[typeIndex - TpiStream.TypeIndexBegin];
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
            result.Add(CodeViewSymbol.FromReader(ReaderContext, ref reader));

        return result;
    }
}
