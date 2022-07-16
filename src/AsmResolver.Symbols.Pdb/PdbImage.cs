using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single Program Debug Database (PDB) image.
/// </summary>
public class PdbImage
{
    private IList<CodeViewSymbol>? _symbols;

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
    /// Obtains a collection of symbols stored in the symbol record stream of the PDB image.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<CodeViewSymbol> GetSymbols() => new List<CodeViewSymbol>();
}
