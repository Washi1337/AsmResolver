using System.Collections.Generic;
using System.Threading;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single module stored in a PDB image.
/// </summary>
public partial class PdbModule : ICodeViewSymbolProvider
{
    private readonly object _lock = new();
    private IList<PdbSourceFile>? _sourceFiles;
    private IList<ICodeViewSymbol>? _symbols;

    /// <summary>
    /// Initialize an empty module in a PDB image.
    /// </summary>
    protected PdbModule()
    {
    }

    /// <summary>
    /// Creates a new linked module in a PDB image.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    public PdbModule(Utf8String name)
        : this(name, name)
    {
    }

    /// <summary>
    /// Creates a new module in a PDB image that was constructed from an object file.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    /// <param name="objectFileName">The path to the object file.</param>
    public PdbModule(Utf8String name, Utf8String objectFileName)
    {
        Name = name;
        ObjectFileName = objectFileName;
        SectionContribution = new SectionContribution();
    }

    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    /// <remarks>
    /// This is often a full path to the object file that was passed into <c>link.exe</c> directly, or a string in the
    /// form of <c>Import:dll_name</c>
    /// </remarks>
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the object file name.
    /// </summary>
    /// <remarks>
    /// In the case this module is linked directly passed to <c>link.exe</c>, this is the same as <see cref="Name"/>.
    /// If the module comes from an archive, this is the full path to that archive.
    /// </remarks>
    [LazyProperty]
    public partial Utf8String? ObjectFileName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of source files that were used to compile the module.
    /// </summary>
    public IList<PdbSourceFile> SourceFiles
    {
        get
        {
            if (_sourceFiles is null)
                Interlocked.CompareExchange(ref _sourceFiles, GetSourceFiles(), null);
            return _sourceFiles;
        }
    }

    /// <summary>
    /// Gets or sets a description of the section within the final binary which contains code
    /// and/or data from this module.
    /// </summary>
    [LazyProperty]
    public partial SectionContribution SectionContribution
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
    /// Obtains the name of the module.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <summary>
    /// Obtains the object file name of the module.
    /// </summary>
    /// <returns>The object file name.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="ObjectFileName"/> property.
    /// </remarks>
    protected virtual Utf8String? GetObjectFileName() => null;

    /// <summary>
    /// Obtains the source file names associated to the module.
    /// </summary>
    /// <returns>The source file names.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="SourceFiles"/> property.
    /// </remarks>
    protected virtual IList<PdbSourceFile> GetSourceFiles() => new List<PdbSourceFile>();

    /// <summary>
    /// Obtains the section contribution of the module.
    /// </summary>
    /// <returns>The section contribution.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="SectionContribution"/> property.
    /// </remarks>
    protected virtual SectionContribution? GetSectionContribution() => new();

    /// <summary>
    /// Obtains the symbols stored in the module.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<ICodeViewSymbol> GetSymbols() => new List<ICodeViewSymbol>();

    /// <inheritdoc />
    public override string ToString() => Name ?? ObjectFileName ?? "<<<NULL NAME>>>";
}
