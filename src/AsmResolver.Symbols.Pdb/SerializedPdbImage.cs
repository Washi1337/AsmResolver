using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private readonly LeafStreamCache _tpi;
    private readonly LeafStreamCache _ipi;

    /// <summary>
    /// Interprets a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file to read from.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    public SerializedPdbImage(MsfFile file, PdbReaderParameters readerParameters)
    {
        _file = file;

        // Obtain relevant core PDB streams.
        if (file.Streams.Count < MinimalRequiredStreamCount)
            throw new BadImageFormatException("MSF does not contain the minimal required amount of streams.");

        InfoStream = InfoStream.FromReader(file.Streams[InfoStream.StreamIndex].CreateReader());
        DbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());
        TpiStream = TpiStream.FromReader(file.Streams[TpiStream.TpiStreamIndex].CreateReader());
        IpiStream = TpiStream.FromReader(file.Streams[TpiStream.IpiStreamIndex].CreateReader());

        // Initialize TPI/IPI caches.
        ReaderContext = new PdbReaderContext(this, readerParameters);
        _tpi = new LeafStreamCache(ReaderContext, TpiStream);
        _ipi = new LeafStreamCache(ReaderContext, IpiStream);

        // Copy over INFO stream metadata.
        Timestamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(InfoStream.Signature);
        Age = InfoStream.Age;
        UniqueId = InfoStream.UniqueId;

        // Copy over DBI stream metadata.
        BuildMajorVersion = DbiStream.BuildMajorVersion;
        BuildMinorVersion = DbiStream.BuildMinorVersion;
        PdbDllVersion = DbiStream.PdbDllVersion;
        Attributes = DbiStream.Attributes;
        Machine = DbiStream.Machine;
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
    public override IEnumerable<ITpiLeaf> GetLeafRecords() => _tpi.GetRecords().Cast<ITpiLeaf>();

    /// <inheritdoc />
    public override IEnumerable<IIpiLeaf> GetIdLeafRecords() => _ipi.GetRecords().Cast<IIpiLeaf>();

    /// <inheritdoc />
    public override bool TryGetLeafRecord(uint typeIndex, [NotNullWhen(true)] out ITpiLeaf? leaf)
    {
        if (_tpi.TryGetRecord(typeIndex, out var x) && x is ITpiLeaf y)
        {
            leaf = y;
            return true;
        }

        return base.TryGetLeafRecord(typeIndex, out leaf);
    }

    /// <inheritdoc />
    public override bool TryGetIdLeafRecord(uint idIndex, [NotNullWhen(true)] out IIpiLeaf? leaf)
    {
        if (_ipi.TryGetRecord(idIndex, out var x) && x is IIpiLeaf y)
        {
            leaf = y;
            return true;
        }

        return base.TryGetIdLeafRecord(idIndex, out leaf);
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

        for (int i = 0; i < DbiStream.Modules.Count; i++)
        {
            var descriptor = DbiStream.Modules[i];
            var modi = GetModiStream(descriptor);
            var sourceFiles = i < DbiStream.SourceFiles.Count
                ? DbiStream.SourceFiles[i]
                : null;

            result.Add(new SerializedPdbModule(ReaderContext, descriptor, sourceFiles, modi));
        }

        return result;
    }

    private ModiStream? GetModiStream(ModuleDescriptor descriptor)
    {
        int index = descriptor.SymbolStreamIndex;
        if (index >= _file.Streams.Count)
            return null;

        var stream = _file.Streams[index];
        return ModiStream.FromReader(stream.CreateReader(), descriptor);
    }

    private class LeafStreamCache
    {
        private readonly PdbReaderContext _context;
        private readonly TpiStream _stream;
        private ICodeViewLeaf?[]? _leaves;

        public LeafStreamCache(PdbReaderContext context, TpiStream stream)
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
                    new ICodeViewLeaf?[_stream.TypeIndexEnd - _stream.TypeIndexBegin], null);
            }
        }

        public IEnumerable<ICodeViewLeaf> GetRecords()
        {
            for (uint typeIndex = _stream.TypeIndexBegin; typeIndex < _stream.TypeIndexEnd; typeIndex++)
            {
                if (TryGetRecord(typeIndex, out var leaf))
                    yield return leaf;
            }
        }

        public bool TryGetRecord(uint typeIndex, [NotNullWhen(true)] out ICodeViewLeaf? leaf)
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
