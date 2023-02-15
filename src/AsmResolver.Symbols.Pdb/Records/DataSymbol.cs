using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a global or local data symbol.
/// </summary>
public class DataSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly LazyVariable<Utf8String?> _name;
    private readonly LazyVariable<CodeViewTypeRecord?> _variableType;

    /// <summary>
    /// Initializes an empty data symbol.
    /// </summary>
    protected DataSymbol()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(GetVariableType);
    }

    /// <summary>
    /// Creates a new named data symbol.
    /// </summary>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="variableType">The data type of the symbol.</param>
    public DataSymbol(Utf8String name, CodeViewTypeRecord variableType)
    {
        _name = new LazyVariable<Utf8String?>(name);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(variableType);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => IsGlobal
        ? CodeViewSymbolType.GData32
        : CodeViewSymbolType.LData32;

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a global data symbol.
    /// </summary>
    public bool IsGlobal
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a local data symbol.
    /// </summary>
    public bool IsLocal
    {
        get => !IsGlobal;
        set => IsGlobal = !value;
    }

    /// <summary>
    /// Gets or sets the file segment index this symbol is located in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the file that this symbol is defined at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <inheritdoc />
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <inheritdoc />
    public CodeViewTypeRecord? VariableType
    {
        get => _variableType.Value;
        set => _variableType.Value = value;
    }

    /// <summary>
    /// Obtains the name of the symbol.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <summary>
    /// Obtains the type of the variable.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="VariableType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetVariableType() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"S_{CodeViewSymbolType.ToString().ToUpper()}: [{SegmentIndex:X4}:{Offset:X8}] {VariableType} {Name}";
    }
}
