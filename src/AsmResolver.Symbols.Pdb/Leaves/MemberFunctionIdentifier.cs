namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a member function identifier, consisting of its name, parent type and its signature.
/// </summary>
public partial class MemberFunctionIdentifier : CodeViewLeaf, IIpiLeaf
{
    /// <summary>
    /// Initializes an empty member function identifier leaf.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected MemberFunctionIdentifier(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new member function identifier leaf.
    /// </summary>
    /// <param name="parentType">The parent type of the member function.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="functionType">The type describing the shape of the function.</param>
    public MemberFunctionIdentifier(CodeViewTypeRecord parentType, Utf8String name, CodeViewTypeRecord functionType)
        : base(0)
    {
        ParentType = parentType;
        Name = name;
        FunctionType = functionType;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.MFuncId;

    /// <summary>
    /// Gets or sets the parent type of the member function.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? ParentType
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
    /// Obtains the parent type of the member function.
    /// </summary>
    /// <returns>The parent type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ParentType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetParentType() => null;

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
