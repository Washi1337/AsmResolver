using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides information about the signature of an indirect call.
/// </summary>
public class CallSiteSymbol : CodeViewSymbol
{
    private readonly LazyVariable<CodeViewTypeRecord?> _functionType;

    /// <summary>
    /// Initializes an empty call site symbol.
    /// </summary>
    protected CallSiteSymbol()
    {
        _functionType = new LazyVariable<CodeViewTypeRecord?>(GetFunctionType);
    }

    /// <summary>
    /// Creates a new call site info symbol.
    /// </summary>
    /// <param name="sectionIndex">The index of the section the call resides in.</param>
    /// <param name="offset">The offset to the call.</param>
    /// <param name="functionType">The type describing the shape of the function.</param>
    public CallSiteSymbol(ushort sectionIndex, int offset, CodeViewTypeRecord functionType)
    {
        SectionIndex = sectionIndex;
        Offset = offset;
        _functionType = new LazyVariable<CodeViewTypeRecord?>(functionType);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.CallSiteInfo;

    /// <summary>
    /// Gets or sets the index of the section the call resides in.
    /// </summary>
    public ushort SectionIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset to the call.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type describing the shape of the function that is called.
    /// </summary>
    public CodeViewTypeRecord? FunctionType
    {
        get => _functionType.Value;
        set => _functionType.Value = value;
    }

    /// <summary>
    /// Obtains the function type of the call.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="FunctionType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetFunctionType() => null;
}
