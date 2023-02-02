using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Info;
using AsmResolver.Symbols.Pdb.Metadata.Modi;
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
    private void EnsureLeavesInitialized()
    {
        if (_leaves is null)
        {
            Interlocked.CompareExchange(ref _leaves,
                new CodeViewLeaf?[TpiStream.TypeIndexEnd - TpiStream.TypeIndexBegin], null);
        }
    }

    /// <inheritdoc />
    public override bool TryGetLeafRecord(uint typeIndex, [NotNullWhen(true)] out CodeViewLeaf? leaf)
    {
        if (typeIndex < TpiStream.TypeIndexBegin)
            return base.TryGetLeafRecord(typeIndex, out leaf);

        EnsureLeavesInitialized();

        if (typeIndex >= TpiStream.TypeIndexBegin && typeIndex < TpiStream.TypeIndexEnd)
        {
            leaf = _leaves[typeIndex - TpiStream.TypeIndexBegin];
            if (leaf is null && TpiStream.TryGetLeafRecordReader(typeIndex, out var reader))
            {
                leaf = CodeViewLeaf.FromReader(ReaderContext, typeIndex, ref reader);
                Interlocked.CompareExchange(ref _leaves[typeIndex - TpiStream.TypeIndexBegin], leaf, null);
            }

            leaf = _leaves[typeIndex - TpiStream.TypeIndexBegin];
            return leaf is not null;
        }

        leaf = null;
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

    /// <inheritdoc />
    protected override IList<PdbModule> GetModules()
    {
        var result = new List<PdbModule>();

        foreach (var descriptor in DbiStream.Modules)
        {
            int index = descriptor.SymbolStreamIndex;
            if (index >= _file.Streams.Count)
                continue;

            var stream = _file.Streams[index];
            var modi = ModiStream.FromReader(stream.CreateReader(), descriptor);
            result.Add(new SerializedPdbModule(ReaderContext, descriptor, modi));
        }

        return result;
    }
}
