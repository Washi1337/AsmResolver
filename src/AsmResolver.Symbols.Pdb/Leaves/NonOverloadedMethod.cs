namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a single method in a type.
/// </summary>
public partial class NonOverloadedMethod : CodeViewNamedField
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty non-overloaded method.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the method.</param>
    protected NonOverloadedMethod(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new overloaded method.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="attributes">The attributes associated to the method.</param>
    /// <param name="function">The function that is referenced by the method.</param>
    public NonOverloadedMethod(Utf8String name, CodeViewFieldAttributes attributes, MemberFunctionLeaf function)
        : base(0)
    {
        Function = function;
        Attributes = attributes;
        Name = name;
    }

    /// <summary>
    /// Creates a new overloaded method.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="attributes">The attributes associated to the method.</param>
    /// <param name="vTableOffset">The offset to the slot the virtual function table that this method occupies.</param>
    /// <param name="function">The function that is referenced by the method.</param>
    public NonOverloadedMethod(Utf8String name, CodeViewFieldAttributes attributes, uint vTableOffset, MemberFunctionLeaf function)
        : base(0)
    {
        Function = function;
        Attributes = attributes;
        Name = name;
        VTableOffset = vTableOffset;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.OneMethod;

    /// <summary>
    /// Gets or sets the function that is referenced by this method.
    /// </summary>
    [LazyProperty]
    public partial MemberFunctionLeaf? Function
    {
        get;
        set;
    }

    /// <summary>
    /// When this method is an introducing virtual method, gets or sets the offset to the slot the virtual function
    /// table that this method occupies.
    /// </summary>
    public uint VTableOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the function that this method references.
    /// </summary>
    /// <returns>The function.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Function"/> property.
    /// </remarks>
    protected virtual MemberFunctionLeaf? GetFunction() => null;
}
