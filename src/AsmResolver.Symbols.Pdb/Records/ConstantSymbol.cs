using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single constant symbol.
/// </summary>
public partial class ConstantSymbol : CodeViewSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a named constant
    /// </summary>
    protected ConstantSymbol()
    {
    }

    /// <summary>
    /// Defines a new named constant.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="type">The type.</param>
    /// <param name="value">The value to assign to the constant.</param>
    public ConstantSymbol(Utf8String name, CodeViewTypeRecord type, ushort value)
    {
        Name = name;
        ConstantType = type;
        Value = value;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Constant;

    /// <summary>
    /// Gets or sets the value type of the constant.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord ConstantType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the numerical value assigned to the constant.
    /// </summary>
    public ushort Value
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the constant.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the name of the constant.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the value type of the constant.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ConstantType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetConstantType() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_CONSTANT: {ConstantType} {Name} = {Value}";
}
