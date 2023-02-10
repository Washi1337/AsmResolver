using System.Collections.Generic;
using System.Threading;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides information about a function and the amount of times it is referenced.
/// </summary>
/// <param name="Function">The function that is referenced.</param>
/// <param name="Count">The number of references this function has.</param>
public record struct FunctionCountPair(FunctionIdLeaf? Function, int Count);

/// <summary>
/// Represents a symbol containing a list of callers or callees.
/// </summary>
public class FunctionListSymbol : CodeViewSymbol
{
    private IList<FunctionCountPair>? _entries;

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => IsCallersList
        ? CodeViewSymbolType.Callers
        : CodeViewSymbolType.Callees;

    /// <summary>
    /// Gets or sets a value indicating the functions in this list are callers.
    /// </summary>
    public bool IsCallersList
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating the functions in this list are callees.
    /// </summary>
    public bool IsCalleesList
    {
        get => !IsCallersList;
        set => IsCallersList = !value;
    }

    /// <summary>
    /// Gets a collection of functions stored in the list.
    /// </summary>
    public IList<FunctionCountPair> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    /// <summary>
    /// Obtains the list of functions.
    /// </summary>
    /// <returns>The functions.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<FunctionCountPair> GetEntries() => new List<FunctionCountPair>();
}
