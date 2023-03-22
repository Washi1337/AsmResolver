using System.Collections.Generic;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Metadata.Modi;
using AsmResolver.Symbols.Pdb.Records;
using AsmResolver.Symbols.Pdb.Records.Serialized;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides an implementation for a PDB module that is read from an input PDB image.
/// </summary>
public class SerializedPdbModule : PdbModule
{
    private readonly PdbReaderContext _context;
    private readonly ModuleDescriptor _descriptor;
    private readonly ModiStream? _stream;

    /// <summary>
    /// Reads a module from a PDB image based on a module descriptor in the DBI stream and a MoDi stream.
    /// </summary>
    /// <param name="context">The reading context in which the module is situated in.</param>
    /// <param name="descriptor">The module descriptor as described in the DBI stream.</param>
    /// <param name="sourceFiles">The source files as described in the DBI stream.</param>
    /// <param name="stream">The MoDi stream to read symbols from.</param>
    public SerializedPdbModule(
        PdbReaderContext context,
        ModuleDescriptor descriptor,
        SourceFileCollection? sourceFiles,
        ModiStream? stream)
    {
        _context = context;
        _descriptor = descriptor;
        _stream = stream;

        if (sourceFiles is not null)
        {
            foreach (var file in sourceFiles)
                SourceFiles.Add(new PdbSourceFile(file));
        }
    }

    /// <inheritdoc />
    protected override Utf8String? GetName() => _descriptor.ModuleName;

    /// <inheritdoc />
    protected override Utf8String? GetObjectFileName() => _descriptor.ObjectFileName;

    /// <inheritdoc />
    protected override SectionContribution? GetSectionContribution() => _descriptor.SectionContribution;

    /// <inheritdoc />
    protected override IList<ICodeViewSymbol> GetSymbols()
    {
        if (_stream?.Symbols is null)
            return new List<ICodeViewSymbol>();

        var reader = _stream.Symbols.CreateReader();
        return SymbolStreamReader.ReadSymbols(_context, ref reader);
    }
}
