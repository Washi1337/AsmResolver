using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol describing a register variable in a function or method.
/// </summary>
public class RegisterSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly LazyVariable<RegisterSymbol, CodeViewTypeRecord?> _variableType;
    private readonly LazyVariable<RegisterSymbol, Utf8String?> _name;

    /// <summary>
    /// Initializes an empty register variable symbol.
    /// </summary>
    protected RegisterSymbol()
    {
        _variableType = new LazyVariable<RegisterSymbol, CodeViewTypeRecord?>(x => x.GetVariableType());
        _name = new LazyVariable<RegisterSymbol, Utf8String?>(x => x.GetName());
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
        _variableType = new LazyVariable<RegisterSymbol, CodeViewTypeRecord?>(variableType);
        _name = new LazyVariable<RegisterSymbol, Utf8String?>(name);
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
    public Utf8String? Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
    }

    /// <inheritdoc />
    public CodeViewTypeRecord? VariableType
    {
        get => _variableType.GetValue(this);
        set => _variableType.SetValue(value);
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
