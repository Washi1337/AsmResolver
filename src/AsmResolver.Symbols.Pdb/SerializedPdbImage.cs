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
using AsmResolver.Symbols.Pdb.Records.Serialized;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides an implementation for a PDB image that is read from an input MSF file.
/// </summary>
public class SerializedPdbImage : PdbImage
{
    private const int MinimalRequiredStreamCount = 5;
    private readonly MsfFile _file;
    private readonly TpiStreamCache _tpi;
    private readonly TpiStreamCache _ipi;

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
        TpiStream = TpiStream.FromReader(file.Streams[TpiStream.TpiStreamIndex].CreateReader());
        IpiStream = TpiStream.FromReader(file.Streams[TpiStream.IpiStreamIndex].CreateReader());

        ReaderContext = new PdbReaderContext(this, readerParameters);

        _tpi = new TpiStreamCache(ReaderContext, TpiStream);
        _ipi = new TpiStreamCache(ReaderContext, IpiStream);
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

    internal TpiStream IpiStream
    {
        get;
    }

    /// <inheritdoc />
    public override bool TryGetLeafRecord(uint typeIndex, [NotNullWhen(true)] out CodeViewLeaf? leaf)
    {
        return _tpi.TryGetRecord(typeIndex, out leaf) || base.TryGetLeafRecord(typeIndex, out leaf);
    }

    /// <inheritdoc />
    public override bool TryGetIdLeafRecord(uint idIndex, [NotNullWhen(true)] out CodeViewLeaf? leaf)
    {
        return _ipi.TryGetRecord(idIndex, out leaf) || base.TryGetLeafRecord(idIndex, out leaf);
    }

    /// <inheritdoc />
    protected override IList<ICodeViewSymbol> GetSymbols()
    {
        int index = DbiStream.SymbolRecordStreamIndex;
        if (index >= _file.Streams.Count)
            return new List<ICodeViewSymbol>();

        var reader = _file.Streams[DbiStream.SymbolRecordStreamIndex].CreateReader();
        return SymbolStreamReader.ReadSymbols(ReaderContext, ref reader);
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

    private class TpiStreamCache
    {
        private readonly PdbReaderContext _context;
        private readonly TpiStream _stream;
        private CodeViewLeaf?[]? _leaves;

        public TpiStreamCache(PdbReaderContext context, TpiStream stream)
        {
            _context = context;
            _stream = stream;
        }

        [MemberNotNull(nameof(_leaves))]
        private void EnsureLeavesInitialized()
        {
            if (_leaves is null)
            {
                Interlocked.CompareExchange(ref _leaves,
                    new CodeViewLeaf?[_stream.TypeIndexEnd - _stream.TypeIndexBegin], null);
            }
        }

        public bool TryGetRecord(uint typeIndex, out CodeViewLeaf? leaf)
        {
            if (typeIndex < _stream.TypeIndexBegin)
            {
                leaf = null;
                return false;
            }

            EnsureLeavesInitialized();

            if (typeIndex >= _stream.TypeIndexBegin && typeIndex < _stream.TypeIndexEnd)
            {
                leaf = _leaves[typeIndex - _stream.TypeIndexBegin];
                if (leaf is null && _stream.TryGetLeafRecordReader(typeIndex, out var reader))
                {
                    leaf = CodeViewLeaf.FromReader(_context, typeIndex, ref reader);
                    Interlocked.CompareExchange(ref _leaves[typeIndex - _stream.TypeIndexBegin], leaf, null);
                }

                leaf = _leaves[typeIndex - _stream.TypeIndexBegin];
                return leaf is not null;
            }

            leaf = null;
            return false;
        }

    }
}
