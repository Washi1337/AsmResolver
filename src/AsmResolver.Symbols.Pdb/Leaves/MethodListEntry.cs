namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents one single entry in a list of overloaded methods.
/// </summary>
public partial class MethodListEntry
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty method list entry.
    /// </summary>
    protected MethodListEntry()
    {
    }

    /// <summary>
    /// Creates a new method list entry.
    /// </summary>
    /// <param name="attributes">The attributes associated to this method.</param>
    /// <param name="function">The referenced function.</param>
    public MethodListEntry(CodeViewFieldAttributes attributes, MemberFunctionLeaf function)
    {
        Attributes = attributes;
        Function = function;
        VTableOffset = 0;
    }

    /// <summary>
    /// Creates a new method list entry.
    /// </summary>
    /// <param name="attributes">The attributes associated to this method.</param>
    /// <param name="function">The referenced function.</param>
    /// <param name="vTableOffset">The offset to the slot the virtual function table that this method occupies.</param>
    public MethodListEntry(CodeViewFieldAttributes attributes, MemberFunctionLeaf function, uint vTableOffset)
    {
        Attributes = attributes;
        Function = function;
        VTableOffset = vTableOffset;
    }

    /// <summary>
    /// Gets or sets the attributes associated to this method.
    /// </summary>
    public CodeViewFieldAttributes Attributes
    {
        get;
        set;
    }

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
    /// Gets a value indicating whether the function is a newly introduced virtual function.
    /// </summary>
    public bool IsIntroducingVirtual =>
        (Attributes & CodeViewFieldAttributes.IntroducingVirtual) != 0
        || (Attributes & CodeViewFieldAttributes.PureIntroducingVirtual) != 0;

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

    /// <inheritdoc />
    public override string ToString()
    {
        return IsIntroducingVirtual
            ? $"{nameof(Attributes)}: {Attributes}, {nameof(Function)}: {Function}, {nameof(VTableOffset)}: {VTableOffset}"
            : $"{nameof(Attributes)}: {Attributes}, {nameof(Function)}: {Function}";
    }
}
