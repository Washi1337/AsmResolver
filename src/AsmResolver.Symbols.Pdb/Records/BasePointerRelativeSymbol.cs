using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol that is defined by an offset relative to the base pointer register.
/// </summary>
public class BasePointerRelativeSymbol : CodeViewSymbol, IRegisterRelativeSymbol
{
    private readonly LazyVariable<CodeViewTypeRecord?> _variableType;
    private readonly LazyVariable<Utf8String?> _name;

    /// <summary>
    /// Initializes an empty base-pointer relative symbol.
    /// </summary>
    protected BasePointerRelativeSymbol()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(GetVariableType);
    }

    /// <summary>
    /// Creates a new base-pointer relative symbol.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="offset">The offset relative to the base pointer.</param>
    /// <param name="variableType">The type of variable the symbol stores.</param>
    public BasePointerRelativeSymbol(Utf8String name, CodeViewTypeRecord variableType, int offset)
    {
        _name = new LazyVariable<Utf8String?>(name);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(variableType);
        Offset = offset;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.BPRel32;

    /// <summary>
    /// Gets or sets the offset relative to the base pointer.
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
        get => _variableType.Value;
        set => _variableType.Value = value;
    }

    /// <summary>
    /// Gets or sets the name of the symbol.
    /// </summary>
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
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
}
