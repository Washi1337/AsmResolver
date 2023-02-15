using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol describing a local variable in a function or method.
/// </summary>
public class LocalSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly LazyVariable<CodeViewTypeRecord?> _variableType;
    private readonly LazyVariable<Utf8String?> _name;

    /// <summary>
    /// Initializes an empty local variable symbol.
    /// </summary>
    protected LocalSymbol()
    {
        _variableType = new LazyVariable<CodeViewTypeRecord?>(GetVariableType);
        _name = new LazyVariable<Utf8String?>(GetName);
    }

    /// <summary>
    /// Creates a new local variable symbol.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="variableType">The type of the variable.</param>
    /// <param name="attributes">The attributes describing the variable.</param>
    public LocalSymbol(Utf8String? name, CodeViewTypeRecord? variableType, LocalAttributes attributes)
    {
        _variableType = new LazyVariable<CodeViewTypeRecord?>(variableType);
        _name = new LazyVariable<Utf8String?>(name);
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Local;

    /// <inheritdoc />
    public CodeViewTypeRecord? VariableType
    {
        get => _variableType.Value;
        set => _variableType.Value = value;
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
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
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
