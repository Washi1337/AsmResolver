using AsmResolver.Symbols.Pdb.Types;

namespace AsmResolver.Symbols.Pdb.Records;

public class ConstantSymbol : CodeViewSymbol
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewType> _type;

    /// <summary>
    /// Initializes a named constant
    /// </summary>
    protected ConstantSymbol()
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _type = new LazyVariable<CodeViewType>(GetConstantType);
    }

    /// <summary>
    /// Defines a new named constant.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="type">The type.</param>
    public ConstantSymbol(Utf8String name, CodeViewType type, ushort value)
    {
        _name = new LazyVariable<Utf8String>(name);
        _type = new LazyVariable<CodeViewType>(type);
        Value = value;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Constant;

    /// <summary>
    /// Gets or sets the value type of the constant.
    /// </summary>
    public CodeViewType Type
    {
        get => _type.Value;
        set => _type.Value = value;
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
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
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
    /// This method is called upon initialization of the <see cref="Type"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetConstantType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{CodeViewSymbolType}: {Type} {Name} = {Value}";
}
