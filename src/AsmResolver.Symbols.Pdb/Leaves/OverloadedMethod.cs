namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a method that is overloaded by one or more functions.
/// </summary>
public class OverloadedMethod : CodeViewField
{
    private readonly LazyVariable<MethodListLeaf?> _methods;

    /// <summary>
    /// Initializes an empty overloaded method.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the method.</param>
    protected OverloadedMethod(uint typeIndex)
        : base(typeIndex)
    {
        _methods = new LazyVariable<MethodListLeaf?>(GetMethods);
    }

    /// <summary>
    /// Creates a new empty overloaded method.
    /// </summary>
    public OverloadedMethod()
        : this(0)
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Method;

    /// <summary>
    /// Gets or sets a list of methods that were overloaded.
    /// </summary>
    public MethodListLeaf? Methods
    {
        get => _methods.Value;
        set => _methods.Value = value;
    }

    /// <summary>
    /// Obtains the list of methods that were overloaded.
    /// </summary>
    /// <returns>The methods.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Methods"/> property.
    /// </remarks>
    protected virtual MethodListLeaf? GetMethods() => null;
}
