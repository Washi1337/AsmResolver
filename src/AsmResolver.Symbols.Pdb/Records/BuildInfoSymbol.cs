using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol containing build information for a file in a PDB image.
/// </summary>
public partial class BuildInfoSymbol : CodeViewSymbol
{
    /// <summary>
    /// Initializes an empty build information symbol.
    /// </summary>
    protected BuildInfoSymbol()
    {
    }

    /// <summary>
    /// Wraps a build information leaf into a symbol.
    /// </summary>
    /// <param name="info">The information to wrap.</param>
    public BuildInfoSymbol(BuildInfoLeaf info)
    {
        Info = info;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.BuildInfo;

    /// <summary>
    /// Gets or sets the information that is wrapped into a symbol.
    /// </summary>
    [LazyProperty]
    public partial BuildInfoLeaf? Info
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the wrapped build information.
    /// </summary>
    /// <returns>The information.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Info"/> property.
    /// </remarks>
    protected virtual BuildInfoLeaf? GetInfo() => null;
}
