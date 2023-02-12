using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a variable symbol in a PDB file.
/// </summary>
public interface IVariableSymbol : ICodeViewSymbol
{
    /// <summary>
    /// Gets or sets the name of the variable.
    /// </summary>
    public Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the value type of the variable.
    /// </summary>
    public CodeViewTypeRecord? VariableType
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
}
