using System.Collections.Generic;
using System.Threading;
using AsmResolver.Shims;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf containing a list of type arguments for a function or method.
/// </summary>
public class ArgumentListLeaf : CodeViewLeaf, ITpiLeaf
{
    private IList<CodeViewTypeRecord>? _types;

    /// <summary>
    /// Initializes an empty argument list.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    protected ArgumentListLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty argument list.
    /// </summary>
    public ArgumentListLeaf()
        : base(0)
    {
    }

    /// <summary>
    /// Creates a new argument list.
    /// </summary>
    public ArgumentListLeaf(params CodeViewTypeRecord[] argumentTypes)
        : base(0)
    {
        _types = new List<CodeViewTypeRecord>(argumentTypes);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.ArgList;

    /// <summary>
    /// Gets an ordered collection of types that correspond to the types of each parameter.
    /// </summary>
    public IList<CodeViewTypeRecord> Types
    {
        get
        {
            if (_types is null)
                Interlocked.CompareExchange(ref _types, GetArgumentTypes(), null);
            return _types;
        }
    }

    /// <summary>
    /// Obtains the argument types stored in the list.
    /// </summary>
    /// <returns>The types.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Types"/> property.
    /// </remarks>
    protected virtual IList<CodeViewTypeRecord> GetArgumentTypes() => new List<CodeViewTypeRecord>();

    /// <inheritdoc />
    public override string ToString() => StringShim.Join(", ", Types);
}
