namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a function identifier, consisting of its name and its signature.
/// </summary>
public partial class FunctionIdentifier : CodeViewLeaf, IIpiLeaf
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty function identifier leaf.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected FunctionIdentifier(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new function identifier leaf.
    /// </summary>
    /// <param name="scopeId">The identifier of the scope defining the function (if available).</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="functionType">The type describing the shape of the function.</param>
    public FunctionIdentifier(uint scopeId, Utf8String name, CodeViewTypeRecord functionType)
        : base(0)
    {
        ScopeId = scopeId;
        Name = name;
        FunctionType = functionType;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.FuncId;

    /// <summary>
    /// Gets or sets the identifier of the scope defining the function (if available).
    /// </summary>
    public uint ScopeId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type describing the shape of the function.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? FunctionType
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the name of the function.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <summary>
    /// Obtains the type of the function.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="FunctionType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetFunctionType() => null;

    /// <inheritdoc />
    public override string ToString() => Name ?? "<<<NULL NAME>>>";
}
