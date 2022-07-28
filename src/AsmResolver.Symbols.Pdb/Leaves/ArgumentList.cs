using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a leaf containing a list of type arguments for a function or method.
/// </summary>
public class ArgumentList : CodeViewLeaf
{
    private IList<CodeViewType>? _types;

    /// <summary>
    /// Initializes an empty argument list.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    protected ArgumentList(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new empty argument list.
    /// </summary>
    public ArgumentList()
        : base(0)
    {
    }

    /// <summary>
    /// Creates a new argument list.
    /// </summary>
    public ArgumentList(params CodeViewType[] argumentTypes)
        : base(0)
    {
        _types = new List<CodeViewType>(argumentTypes);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.ArgList;

    /// <summary>
    /// Gets an ordered collection of types that correspond to the types of each parameter.
    /// </summary>
    public IList<CodeViewType> Types
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
    protected virtual IList<CodeViewType> GetArgumentTypes() => new List<CodeViewType>();
}
