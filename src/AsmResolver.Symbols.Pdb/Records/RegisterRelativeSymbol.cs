using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol that is defined by a register+offset pair.
/// </summary>
public partial class RegisterRelativeSymbol : CodeViewSymbol, IRegisterRelativeSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes an empty relative register symbol.
    /// </summary>
    protected RegisterRelativeSymbol()
    {
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
        Name = name;
        BaseRegister = baseRegister;
        Offset = offset;
        VariableType = variableType;
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
    [LazyProperty]
    public partial CodeViewTypeRecord? VariableType
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
