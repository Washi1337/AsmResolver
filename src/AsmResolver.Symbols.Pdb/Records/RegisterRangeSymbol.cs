namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Defines an address range in which a local variable or parameter is defined that is fully defined by a register.
/// </summary>
public class RegisterRangeSymbol : DefinitionRangeSymbol
{
    /// <summary>
    /// Initializes an empty register range symbol.
    /// </summary>
    protected RegisterRangeSymbol()
    {
    }

    /// <summary>
    /// Creates a new register range symbol.
    /// </summary>
    /// <param name="register">The register.</param>
    /// <param name="range">The range in which the symbol is valid.</param>
    public RegisterRangeSymbol(ushort register, LocalAddressRange range)
    {
        Register = register;
        Range = range;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.DefRangeRegister;

    /// <summary>
    /// Gets or sets the register that defines the symbol.
    /// </summary>
    public ushort Register
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol may have no user name on one of the control flow paths
    /// within the function.
    /// </summary>
    public bool IsMaybe
    {
        get;
        set;
    }
}
