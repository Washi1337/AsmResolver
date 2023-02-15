namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a function identifier, consisting of its name and its signature.
/// </summary>
public class FunctionIdentifier : CodeViewLeaf, IIpiLeaf
{
    private readonly LazyVariable<Utf8String?> _name;
    private readonly LazyVariable<CodeViewTypeRecord?> _functionType;

    /// <summary>
    /// Initializes an empty function identifier leaf.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected FunctionIdentifier(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String?>(GetName);
        _functionType = new LazyVariable<CodeViewTypeRecord?>(GetFunctionType);
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
        _name = new LazyVariable<Utf8String?>(name);
        _functionType = new LazyVariable<CodeViewTypeRecord?>(functionType);
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
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Gets or sets the type describing the shape of the function.
    /// </summary>
    public CodeViewTypeRecord? FunctionType
    {
        get => _functionType.Value;
        set => _functionType.Value = value;
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
