using System.Collections.Generic;
using AsmResolver.Symbols.Pdb.Records;

namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Describes an object that defines a list of symbols.
/// </summary>
public interface ICodeViewSymbolProvider
{
    /// <summary>
    /// Gets the list of defined symbols.
    /// </summary>
    public IList<ICodeViewSymbol> Symbols
    {
        get;
    }
}
