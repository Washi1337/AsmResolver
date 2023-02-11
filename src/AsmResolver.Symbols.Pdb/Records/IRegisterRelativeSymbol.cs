using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Describes a variable symbol that is defined by a register and an offset relative to that register.
/// </summary>
public interface IRegisterRelativeSymbol : ICodeViewSymbol
{
    /// <summary>
    /// Gets or sets the offset relative to the base register.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of values the symbol stores.
    /// </summary>
    public CodeViewTypeRecord? VariableType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the symbol.
    /// </summary>
    public Utf8String? Name
    {
        get;
        set;
    }
}
