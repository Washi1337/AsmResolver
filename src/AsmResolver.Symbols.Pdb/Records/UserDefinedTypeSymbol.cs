using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a user-defined type symbol in a PDB symbol stream, mapping a symbol to a type in the TPI stream.
/// </summary>
public partial class UserDefinedTypeSymbol : CodeViewSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new empty user-defined type symbol.
    /// </summary>
    protected UserDefinedTypeSymbol()
    {
    }

    /// <summary>
    /// Defines a new user-defined type.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="type">The type.</param>
    public UserDefinedTypeSymbol(Utf8String name, CodeViewTypeRecord type)
    {
        Name = name;
        SymbolType = type;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Udt;

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index associated to the type.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord SymbolType
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the new name of the type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the type that is referenced by this symbol.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SymbolType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetSymbolType() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_UDT: {SymbolType} {Name}";
}
