using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a file static variable symbol.
/// </summary>
public partial class FileStaticSymbol : CodeViewSymbol, IVariableSymbol
{
    /// <summary>
    /// Initializes an empty file static symbol.
    /// </summary>
    protected FileStaticSymbol()
    {
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
        Name = name;
        VariableType = variableType;
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

    /// <summary>
    /// Gets or sets the attributes describing the variable.
    /// </summary>
    public LocalAttributes Attributes
    {
        get;
        set;
    }

    /// <inheritdoc />
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <inheritdoc />
    [LazyProperty]
    public partial CodeViewTypeRecord? VariableType
    {
        get;
        set;
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

    /// <inheritdoc />
    public override string ToString() => $"S_FILESTATIC: {VariableType} {Name}";

}
