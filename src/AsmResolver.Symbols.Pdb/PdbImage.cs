using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;
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
    /// <param name="file">The input file.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(IInputFile file) => FromFile(MsfFile.FromFile(file));

    /// <summary>
    /// Interprets a byte array as a PDB image.
    /// </summary>
    /// <param name="data">The data to interpret.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromBytes(byte[] data) => FromFile(MsfFile.FromBytes(data));

    /// <summary>
    /// Reads an PDB image from the provided input stream reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromReader(BinaryStreamReader reader) => FromFile(MsfFile.FromReader(reader));

    /// <summary>
    /// Loads a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(MsfFile file)
    {
        return FromFile(file, new PdbReaderParameters(ThrowErrorListener.Instance));
    }

    /// <summary>
    /// Loads a PDB image from the provided MSF file.
    /// </summary>
    /// <param name="file">The MSF file.</param>
    /// <param name="readerParameters">The parameters to use while reading the PDB image.</param>
    /// <returns>The read PDB image.</returns>
    public static PdbImage FromFile(MsfFile file, PdbReaderParameters readerParameters)
    {
        return new SerializedPdbImage(file, readerParameters);
    }

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
    /// <exception cref="ArgumentException">Occurs when the type index is invalid.</exception>
    public ITpiLeaf GetLeafRecord(uint typeIndex)
    {
        if (!TryGetLeafRecord(typeIndex, out var type))
            throw new ArgumentException("Invalid type index.");
        return type;
    }

    /// <summary>
    /// Obtains a type record from the TPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <returns>The resolved type.</returns>
    /// <exception cref="ArgumentException">Occurs when the type index is invalid.</exception>
    public TLeaf GetLeafRecord<TLeaf>(uint typeIndex)
        where TLeaf : ITpiLeaf
    {
        return (TLeaf) GetLeafRecord(typeIndex);
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
    /// <exception cref="ArgumentException">Occurs when the ID index is invalid.</exception>
    public IIpiLeaf GetIdLeafRecord(uint idIndex)
    {
        if (!TryGetIdLeafRecord(idIndex, out var leaf))
            throw new ArgumentException("Invalid ID index.");
        return leaf;
    }

    /// <summary>
    /// Obtains an ID record from the IPI stream based on its ID index.
    /// </summary>
    /// <param name="idIndex">The ID index.</param>
    /// <returns>The resolved leaf</returns>
    /// <exception cref="ArgumentException">Occurs when the ID index is invalid.</exception>
    public TLeaf GetIdLeafRecord<TLeaf>(uint idIndex)
        where TLeaf : CodeViewLeaf
    {
        return (TLeaf) GetIdLeafRecord(idIndex);
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
