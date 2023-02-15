namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a compile symbol that uses version 3 of the file format.
/// </summary>
public class Compile3Symbol : CompileSymbol
{
    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Compile3;

    /// <summary>
    /// Gets or sets the front-end QFE version of the file.
    /// </summary>
    public ushort FrontEndQfeVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the front-end QFE version of the file.
    /// </summary>
    public ushort BackEndQfeVersion
    {
        get;
        set;
    }
}

