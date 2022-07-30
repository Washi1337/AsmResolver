namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents one single entry in a list of overloaded methods.
/// </summary>
public class MethodListEntry
{
    private readonly LazyVariable<MemberFunctionLeaf?> _function;

    /// <summary>
    /// Initializes an empty method list entry.
    /// </summary>
    protected MethodListEntry()
    {
        _function = new LazyVariable<MemberFunctionLeaf?>(GetFunction);
    }

    /// <summary>
    /// Creates a new method list entry.
    /// </summary>
    /// <param name="attributes">The attributes associated to this method.</param>
    /// <param name="function">The referenced function.</param>
    public MethodListEntry(CodeViewFieldAttributes attributes, MemberFunctionLeaf function)
    {
        Attributes = attributes;
        _function = new LazyVariable<MemberFunctionLeaf?>(function);
        VFTableOffset = 0;
    }

    /// <summary>
    /// Creates a new method list entry.
    /// </summary>
    /// <param name="attributes">The attributes associated to this method.</param>
    /// <param name="function">The referenced function.</param>
    /// <param name="vfTableOffset">The offset to the slot the virtual function table that this method occupies.</param>
    public MethodListEntry(CodeViewFieldAttributes attributes, MemberFunctionLeaf function, uint vfTableOffset)
    {
        Attributes = attributes;
        _function = new LazyVariable<MemberFunctionLeaf?>(function);
        VFTableOffset = vfTableOffset;
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
    public MemberFunctionLeaf? Function
    {
        get => _function.Value;
        set => _function.Value = value;
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
    public uint VFTableOffset
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
            ? $"{nameof(Attributes)}: {Attributes}, {nameof(Function)}: {Function}, {nameof(VFTableOffset)}: {VFTableOffset}"
            : $"{nameof(Attributes)}: {Attributes}, {nameof(Function)}: {Function}";
    }
}
