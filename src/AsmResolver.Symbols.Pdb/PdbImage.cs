using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;
using AsmResolver.Symbols.Pdb.Types;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single Program Debug Database (PDB) image.
/// </summary>
public class PdbImage
{
    private IList<CodeViewSymbol>? _symbols;
    private ConcurrentDictionary<uint, SimpleType> _simpleTypes = new();

    /// <summary>
    /// Gets a collection of all symbols stored in the PDB image.
    /// </summary>
    public IList<CodeViewSymbol> Symbols
    {
        get
        {
            if (_symbols is null)
                Interlocked.CompareExchange(ref _symbols, GetSymbols(), null);
            return _symbols;
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
    public static PdbImage FromFile(MsfFile file) => new SerializedPdbImage(file);

    /// <summary>
    /// Attempts to obtain a type record from the TPI or IPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <param name="type">The resolved type.</param>
    /// <returns><c>true</c> if the type was found, <c>false</c> otherwise.</returns>
    public virtual bool TryGetTypeRecord(uint typeIndex, [NotNullWhen(true)] out CodeViewType? type)
    {
        typeIndex &= 0x7fffffff;
        if (typeIndex < 0x1000)
        {
            type = _simpleTypes.GetOrAdd(typeIndex, i => new SimpleType(i));
            return true;
        }

        type = null;
        return false;
    }

    /// <summary>
    /// Obtains a type record from the TPI or IPI stream based on its type index.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    /// <returns>The resolved type.</returns>
    /// <exception cref="ArgumentException">Occurs when the type index is invalid.</exception>
    public CodeViewType GetTypeRecord(uint typeIndex)
    {
        if (!TryGetTypeRecord(typeIndex, out var type))
            throw new ArgumentException("Invalid type index.");
        return type;
    }

    /// <summary>
    /// Obtains a collection of symbols stored in the symbol record stream of the PDB image.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<CodeViewSymbol> GetSymbols() => new List<CodeViewSymbol>();
}
