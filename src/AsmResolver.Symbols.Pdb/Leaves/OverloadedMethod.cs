namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a method that is overloaded by one or more functions.
/// </summary>
public class OverloadedMethod : CodeViewNamedField
{
    private readonly LazyVariable<OverloadedMethod, MethodListLeaf?> _methods;

    /// <summary>
    /// Initializes an empty overloaded method.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the method.</param>
    protected OverloadedMethod(uint typeIndex)
        : base(typeIndex)
    {
        _methods = new LazyVariable<OverloadedMethod, MethodListLeaf?>(x => x.GetMethods());
    }

    /// <summary>
    /// Creates a new empty overloaded method.
    /// </summary>
    public OverloadedMethod()
        : base(0)
    {
        _methods = new LazyVariable<OverloadedMethod, MethodListLeaf?>(new MethodListLeaf());
    }

    /// <summary>
    /// Creates a new overloaded method.
    /// </summary>
    public OverloadedMethod(MethodListLeaf methods)
        : base(0)
    {
        _methods = new LazyVariable<OverloadedMethod, MethodListLeaf?>(methods);
    }

    /// <summary>
    /// Creates a new overloaded method.
    /// </summary>
    public OverloadedMethod(params MethodListEntry[] methods)
        : base(0)
    {
        _methods = new LazyVariable<OverloadedMethod, MethodListLeaf?>(new MethodListLeaf(methods));
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Method;

    /// <summary>
    /// Gets or sets a list of methods that were overloaded.
    /// </summary>
    public MethodListLeaf? Methods
    {
        get => _methods.GetValue(this);
        set => _methods.SetValue(value);
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
