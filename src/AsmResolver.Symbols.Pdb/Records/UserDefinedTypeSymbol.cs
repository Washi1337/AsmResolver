using AsmResolver.Symbols.Pdb.Types;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a user-defined type symbol in a PDB symbol stream, mapping a symbol to a type in the TPI stream.
/// </summary>
public class UserDefinedTypeSymbol : CodeViewSymbol
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewType> _type;

    /// <summary>
    /// Initializes a new empty user-defined type symbol.
    /// </summary>
    protected UserDefinedTypeSymbol()
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _type = new LazyVariable<CodeViewType>(GetSymbolType);
    }

    /// <summary>
    /// Defines a new user-defined type.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="type">The type.</param>
    public UserDefinedTypeSymbol(Utf8String name, CodeViewType type)
    {
        _name = new LazyVariable<Utf8String>(name);
        _type = new LazyVariable<CodeViewType>(type);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Udt;

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Gets or sets the index associated to the type.
    /// </summary>
    public CodeViewType Type
    {
        get => _type.Value;
        set => _type.Value = value;
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
    /// This method is called upon initialization of the <see cref="Type"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetSymbolType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{CodeViewSymbolType}: {Type} {Name}";
}
