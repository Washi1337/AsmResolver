using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.File;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single Program Debug Database (PDB) image.
/// </summary>
public class PdbImage : ICodeViewSymbolProvider
{
    private readonly ConcurrentDictionary<uint, SimpleTypeRecord> _simpleTypes = new();

    private IList<ICodeViewSymbol>? _symbols;
    private IList<PdbModule>? _modules;

    private protected virtual IErrorListener ErrorListener => ThrowErrorListener.Instance;

    /// <summary>
    /// Gets or sets the time-stamp of the PDB file.
    /// </summary>
    public DateTime Timestamp
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of times the PDB file has been written.
    /// </summary>
    public uint Age
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Gets or sets the unique identifier assigned to the PDB file.
    /// </summary>
    public Guid UniqueId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the major version of the toolchain that was used to build the program.
    /// </summary>
    public byte BuildMajorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minor version of the toolchain that was used to build the program.
    /// </summary>
    public byte BuildMinorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the version number of mspdbXXXX.dll that was used to produce this PDB file.
    /// </summary>
    public ushort PdbDllVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes associated to the DBI stream.
    /// </summary>
    public DbiAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the machine type the program was compiled for.
    /// </summary>
    public MachineType Machine
    {
        get;
        set;
    }

    /// <inheritdoc />
    public IList<ICodeViewSymbol> Symbols
    {
        get
        {
            if (_symbols is null)
                Interlocked.CompareExchange(ref _symbols, GetSymbols(), null);
            return _symbols;
        }
    }

    /// <summary>
    /// Gets a collection of all modules stored in the PDB image.
    /// </summary>
    public IList<PdbModule> Modules
    {
        get
        {
            if (_modules is null)
                Interlocked.CompareExchange(ref _modules, GetModules(), null);
            return _modules;
        }
    }

    /// <summary>
    /// Reads a PDB image from the provided input file.
    /// </summary>
    /// <param name="path">The path to the PDB file.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(string path) => FromFile(MsfFile.FromFile(path));

    /// <summary>
    /// Reads a PDB image from the provided input file.
    /// </summary>
    /// <param name="path">The path to the PDB file.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(string path, PdbReaderParameters readerParameters)
        => FromFile(MsfFile.FromFile(path), readerParameters);

    /// <summary>
    /// Reads a PDB image from the provided input file.
    /// </summary>
    /// <param name="file">The input file.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(IInputFile file) => FromFile(MsfFile.FromFile(file));

    /// <summary>
    /// Reads a PDB image from the provided input file.
    /// </summary>
    /// <param name="file">The input file.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(IInputFile file, PdbReaderParameters readerParameters)
        => FromFile(MsfFile.FromFile(file), readerParameters);

    /// <summary>
    /// Interprets a byte array as a PDB image.
    /// </summary>
    /// <param name="data">The data to interpret.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromBytes(byte[] data) => FromFile(MsfFile.FromBytes(data));

    /// <summary>
    /// Interprets a byte array as a PDB image.
    /// </summary>
    /// <param name="data">The data to interpret.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromBytes(byte[] data, PdbReaderParameters readerParameters)
        => FromFile(MsfFile.FromBytes(data), readerParameters);

    /// <summary>
    /// Reads an PDB image from the provided input stream reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromReader(BinaryStreamReader reader) => FromFile(MsfFile.FromReader(reader));

    /// <summary>
    /// Reads an PDB image from the provided input stream reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromReader(BinaryStreamReader reader, PdbReaderParameters readerParameters)
        => FromFile(MsfFile.FromReader(reader), readerParameters);

    /// <summary>
    /// Loads a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(MsfFile file)
        => FromFile(file, new PdbReaderParameters(ThrowErrorListener.Instance));

    /// <summary>
    /// Loads a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(MsfFile file, PdbReaderParameters readerParameters)
        => new SerializedPdbImage(file, readerParameters);

    /// <summary>
    /// Obtains all records stored in the original TPI stream of the PDB image.
    /// </summary>
    /// <returns>An object that lazily enumerates all TPI leaf records.</returns>
    public virtual IEnumerable<ITpiLeaf> GetLeafRecords() => Enumerable.Empty<ITpiLeaf>();

    /// <summary>
    /// Obtains all records stored in the original IPI stream of the PDB image.
    /// </summary>
    /// <returns>An object that lazily enumerates all IPI leaf records.</returns>
    public virtual IEnumerable<IIpiLeaf> GetIdLeafRecords() => Enumerable.Empty<IIpiLeaf>();

    /// <summary>
    /// Attempts to obtain a type record from the TPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <param name="leaf">The resolved type.</param>
    /// <returns><c>true</c> if the type was found, <c>false</c> otherwise.</returns>
    public virtual bool TryGetLeafRecord(uint typeIndex, [NotNullWhen(true)] out ITpiLeaf? leaf)
    {
        typeIndex &= 0x7fffffff;
        if (typeIndex is > 0 and < 0x1000)
        {
            leaf = _simpleTypes.GetOrAdd(typeIndex, i => new SimpleTypeRecord(i));
            return true;
        }

        leaf = null;
        return false;
    }

    /// <summary>
    /// Attempts to obtain a type record from the TPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <param name="leaf">The resolved type.</param>
    /// <returns><c>true</c> if the type was found, <c>false</c> otherwise.</returns>
    public bool TryGetLeafRecord<TLeaf>(uint typeIndex, [NotNullWhen(true)] out TLeaf? leaf)
        where TLeaf : ITpiLeaf
    {
        if (TryGetLeafRecord(typeIndex, out var x) && x is TLeaf resolved)
        {
            leaf = resolved;
            return true;
        }

        leaf = default;
        return false;
    }

    /// <summary>
    /// Obtains a type record from the TPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <returns>The resolved type.</returns>
    public ITpiLeaf? GetLeafRecord(uint typeIndex)
    {
        if (!TryGetLeafRecord(typeIndex, out var type))
            return ErrorListener.BadImageAndReturn<ITpiLeaf>($"The leaf index {typeIndex:X8} is invalid.");
        return type;
    }

    /// <summary>
    /// Obtains a type record from the TPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <returns>The resolved type.</returns>
    public TLeaf? GetLeafRecord<TLeaf>(uint typeIndex)
        where TLeaf : ITpiLeaf
    {
        if (!TryGetLeafRecord(typeIndex, out var type))
            return ErrorListener.BadImageAndReturn<TLeaf>($"The leaf index {typeIndex:X8} is invalid.");

        return type switch
        {
            TLeaf resolved => resolved,
            UnknownCodeViewLeaf => ErrorListener.BadImageAndReturn<TLeaf>(
                $"The leaf index {typeIndex:X8} has an unknown type {type.LeafKind}."),
            _ => ErrorListener.BadImageAndReturn<TLeaf>(
                $"The leaf index {typeIndex:X8} has an unexpected type ({type.LeafKind}).")
        };
    }

    /// <summary>
    /// Attempts to obtain an ID record from the IPI stream based on its ID index.
    /// </summary>
    /// <param name="idIndex">The ID index.</param>
    /// <param name="leaf">The resolved leaf.</param>
    /// <returns><c>true</c> if the leaf was found, <c>false</c> otherwise.</returns>
    public virtual bool TryGetIdLeafRecord(uint idIndex, [NotNullWhen(true)] out IIpiLeaf? leaf)
    {
        leaf = null;
        return false;
    }

    /// <summary>
    /// Attempts to obtain an ID record from the IPI stream based on its ID index.
    /// </summary>
    /// <param name="idIndex">The ID index.</param>
    /// <param name="leaf">The resolved leaf.</param>
    /// <returns><c>true</c> if the leaf was found, <c>false</c> otherwise.</returns>
    public bool TryGetIdLeafRecord<TLeaf>(uint idIndex, [NotNullWhen(true)] out TLeaf? leaf)
        where TLeaf : IIpiLeaf
    {
        if (TryGetIdLeafRecord(idIndex, out var x) && x is TLeaf resolved)
        {
            leaf = resolved;
            return true;
        }

        leaf = default;
        return false;
    }

    /// <summary>
    /// Obtains an ID record from the IPI stream based on its ID index.
    /// </summary>
    /// <param name="idIndex">The ID index.</param>
    /// <returns>The resolved leaf</returns>
    public IIpiLeaf? GetIdLeafRecord(uint idIndex)
    {
        if (!TryGetIdLeafRecord(idIndex, out var leaf))
            return ErrorListener.BadImageAndReturn<IIpiLeaf>($"The ID index {idIndex:X8} is invalid.");
        return leaf;
    }

    /// <summary>
    /// Obtains an ID record from the IPI stream based on its ID index.
    /// </summary>
    /// <param name="idIndex">The ID index.</param>
    /// <returns>The resolved leaf</returns>
    public TLeaf? GetIdLeafRecord<TLeaf>(uint idIndex)
        where TLeaf : CodeViewLeaf
    {
        if (!TryGetLeafRecord(idIndex, out var leaf))
            return ErrorListener.BadImageAndReturn<TLeaf>($"The ID index {idIndex:X8} is invalid.");

        return leaf switch
        {
            TLeaf resolved => resolved,
            UnknownCodeViewLeaf => ErrorListener.BadImageAndReturn<TLeaf>(
                $"The ID index {idIndex:X8} has an unknown type {leaf.LeafKind}."),
            _ => ErrorListener.BadImageAndReturn<TLeaf>(
                $"The ID index {idIndex:X8} has an unexpected type ({leaf.LeafKind}).")
        };
    }

    /// <summary>
    /// Obtains a collection of symbols stored in the symbol record stream of the PDB image.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<ICodeViewSymbol> GetSymbols() => new List<ICodeViewSymbol>();

    /// <summary>
    /// Obtains a collection of modules stored in the DBI stream of the PDB image.
    /// </summary>
    /// <returns>The modules.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Modules"/> property.
    /// </remarks>
    protected virtual IList<PdbModule> GetModules() => new List<PdbModule>();
}
