namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Describes a variable symbol that is defined by a register and an offset relative to that register.
/// </summary>
public interface IRegisterRelativeSymbol : IVariableSymbol
{
    /// <summary>
    /// Gets or sets the offset relative to the base register.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }
}
