using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a file static variable symbol.
/// </summary>
public class FileStaticSymbol : CodeViewSymbol, IVariableSymbol
{
    private readonly LazyVariable<Utf8String?> _name;
    private readonly LazyVariable<CodeViewTypeRecord?> _variableType;

    /// <summary>
    /// Initializes an empty file static symbol.
    /// </summary>
    protected FileStaticSymbol()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(GetVariableType);
    }

    /// <summary>
    /// Creates a new file static variable symbol.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="variableType">The data type of the variable.</param>
    /// <param name="attributes">The attributes describing the variable.</param>
    public FileStaticSymbol(Utf8String name, CodeViewTypeRecord variableType, LocalAttributes attributes)
    {
        Attributes = attributes;
        _name = new LazyVariable<Utf8String?>(name);
        _variableType = new LazyVariable<CodeViewTypeRecord?>(variableType);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.FileStatic;

    /// <summary>
    /// Gets or sets the index of the module's file name within the string table.
    /// </summary>
    public uint ModuleFileNameOffset
    {
        get;
        set;
    }

    /// <inheritdoc />
    public LocalAttributes Attributes
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
    /// Obtains the type of the variable.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="VariableType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetVariableType() => null;

    /// <summary>
    /// Obtains the name of the variable.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

}
