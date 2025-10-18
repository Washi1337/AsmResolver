using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol describing a local variable in a function or method.
/// </summary>
public partial class LocalSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty local variable symbol.
    /// </summary>
    protected LocalSymbol()
    {
    }

    /// <summary>
    /// Creates a new local variable symbol.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="variableType">The type of the variable.</param>
    /// <param name="attributes">The attributes describing the variable.</param>
    public LocalSymbol(Utf8String? name, CodeViewTypeRecord? variableType, LocalAttributes attributes)
    {
        VariableType = variableType;
        Name = name;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Local;

    /// <inheritdoc />
    [LazyProperty]
    public partial CodeViewTypeRecord? VariableType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the attributes describing the variable.
    /// </summary>
    public LocalAttributes Attributes
    {
        get;
        set;
    }

    /// <inheritdoc />
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the type of the variable.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="VariableType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetVariableType() => null;

    /// <summary>
    /// Obtains the name of the variable.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_LOCAL32: {VariableType} {Name}";
}
