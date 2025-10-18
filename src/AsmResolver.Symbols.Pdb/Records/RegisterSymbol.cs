using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol describing a register variable in a function or method.
/// </summary>
public partial class RegisterSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty register variable symbol.
    /// </summary>
    protected RegisterSymbol()
    {
    }

    /// <summary>
    /// Creates a new register variable symbol.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="variableType">The type of the variable.</param>
    /// <param name="register">The register that defines the variable.</param>
    public RegisterSymbol(Utf8String? name, CodeViewTypeRecord? variableType, ushort register)
    {
        Register = register;
        VariableType = variableType;
        Name = name;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Register;

    /// <summary>
    /// Gets or sets the register that defines the variable.
    /// </summary>
    public ushort Register
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

    /// <inheritdoc />
    [LazyProperty]
    public partial CodeViewTypeRecord? VariableType
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
    public override string ToString() => $"S_REGISTER: {VariableType} {Name}";
}
