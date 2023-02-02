using System.Collections.Generic;
using System.Threading;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single module stored in a PDB image.
/// </summary>
public class PdbModule
{
    private readonly LazyVariable<Utf8String?> _name;
    private readonly LazyVariable<Utf8String?> _objectFileName;
    private readonly LazyVariable<SectionContribution> _sectionContribution;

    private IList<CodeViewSymbol>? _symbols;

    /// <summary>
    /// Initialize an empty module in a PDB image.
    /// </summary>
    protected PdbModule()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
        _objectFileName = new LazyVariable<Utf8String?>(GetObjectFileName);
        _sectionContribution = new LazyVariable<SectionContribution>(GetSectionContribution);
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
        _name = new LazyVariable<Utf8String?>(name);
        _objectFileName = new LazyVariable<Utf8String?>(objectFileName);
        _sectionContribution = new LazyVariable<SectionContribution>(new SectionContribution());
    }

    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    /// <remarks>
    /// This is often a full path to the object file that was passed into <c>link.exe</c> directly, or a string in the
    /// form of <c>Import:dll_name</c>
    /// </remarks>
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Gets or sets the name of the object file name.
    /// </summary>
    /// <remarks>
    /// In the case this module is linked directly passed to <c>link.exe</c>, this is the same as <see cref="ModuleName"/>.
    /// If the module comes from an archive, this is the full path to that archive.
    /// </remarks>
    public Utf8String? ObjectFileName
    {
        get => _objectFileName.Value;
        set => _objectFileName.Value = value;
    }

    /// <summary>
    /// Gets or sets a description of the section within the final binary which contains code
    /// and/or data from this module.
    /// </summary>
    public SectionContribution SectionContribution
    {
        get => _sectionContribution.Value;
        set => _sectionContribution.Value = value;
    }

    /// <summary>
    /// Gets a collection of symbols that are defined in the module.
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
    protected virtual IList<CodeViewSymbol> GetSymbols() => new List<CodeViewSymbol>();
}
