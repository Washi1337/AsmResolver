namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a using directive that allows for types to be used without specifying its declaring namespace.
/// </summary>
public partial class UsingNamespaceSymbol : CodeViewSymbol
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new empty using namespace.
    /// </summary>
    protected UsingNamespaceSymbol()
    {
    }

    /// <summary>
    /// Creates a new using namespace record.
    /// </summary>
    /// <param name="name">The namespace to use.</param>
    public UsingNamespaceSymbol(Utf8String name)
    {
        Name = name;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.UNamespace;

    /// <summary>
    /// Gets or sets the name of the namespace that is being used.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the name of the namespace.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => $"S_UNAMESPACE: {Name}";
}
