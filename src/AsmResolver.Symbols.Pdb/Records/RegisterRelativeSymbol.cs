using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol that is defined by a register+offset pair.
/// </summary>
public class RegisterRelativeSymbol : CodeViewSymbol, IRegisterRelativeSymbol
{
    private readonly LazyVariable<RegisterRelativeSymbol, CodeViewTypeRecord?> _variableType;
    private readonly LazyVariable<RegisterRelativeSymbol, Utf8String?> _name;

    /// <summary>
    /// Initializes an empty relative register symbol.
    /// </summary>
    protected RegisterRelativeSymbol()
    {
        _variableType = new LazyVariable<RegisterRelativeSymbol, CodeViewTypeRecord?>(x => x.GetVariableType());
        _name = new LazyVariable<RegisterRelativeSymbol, Utf8String?>(x => x.GetName());
    }

    /// <summary>
    /// Creates a new relative register symbol.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="baseRegister">The base register.</param>
    /// <param name="offset">The offset to the register.</param>
    /// <param name="variableType">The type of variable the register+offset pair stores.</param>
    public RegisterRelativeSymbol(Utf8String name, ushort baseRegister, int offset, CodeViewTypeRecord variableType)
    {
        _name = new LazyVariable<RegisterRelativeSymbol, Utf8String?>(name);
        BaseRegister = baseRegister;
        Offset = offset;
        _variableType = new LazyVariable<RegisterRelativeSymbol, CodeViewTypeRecord?>(variableType);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.RegRel32;

    /// <summary>
    /// Gets or sets the base register.
    /// </summary>
    public ushort BaseRegister
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset relative to the base register.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }

    /// <inheritdoc />
    public CodeViewTypeRecord? VariableType
    {
        get => _variableType.GetValue(this);
        set => _variableType.SetValue(value);
    }

    /// <inheritdoc />
    public Utf8String? Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
    }

    /// <summary>
    /// Obtains the variable type of the symbol.
    /// </summary>
    /// <returns>The variable type</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="VariableType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetVariableType() => null;

    /// <summary>
    /// Obtains the name of the symbol.
    /// </summary>
    /// <returns>The name</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_REGREL32: [+{Offset:X}] {VariableType} {Name}";
}
