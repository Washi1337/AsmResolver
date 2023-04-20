namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a using directive that allows for types to be used without specifying its declaring namespace.
/// </summary>
public class UsingNamespaceSymbol : CodeViewSymbol
{
    private readonly LazyVariable<UsingNamespaceSymbol, Utf8String> _name;

    /// <summary>
    /// Initializes a new empty using namespace.
    /// </summary>
    protected UsingNamespaceSymbol()
    {
        _name = new LazyVariable<UsingNamespaceSymbol, Utf8String>(x => x.GetName());
    }

    /// <summary>
    /// Creates a new using namespace record.
    /// </summary>
    /// <param name="name">The namespace to use.</param>
    public UsingNamespaceSymbol(Utf8String name)
    {
        _name = new LazyVariable<UsingNamespaceSymbol, Utf8String>(name);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.UNamespace;

    /// <summary>
    /// Gets or sets the name of the namespace that is being used.
    /// </summary>
    public Utf8String Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
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
