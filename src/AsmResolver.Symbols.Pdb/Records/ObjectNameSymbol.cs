namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents an object name symbol in a PDB module.
/// </summary>
public class ObjectNameSymbol : CodeViewSymbol
{
    private readonly LazyVariable<Utf8String> _name;

    /// <summary>
    /// Initializes an empty object name symbol.
    /// </summary>
    protected ObjectNameSymbol()
    {
        _name = new LazyVariable<Utf8String>(GetName);
    }

    /// <summary>
    /// Creates a new object name symbol.
    /// </summary>
    /// <param name="signature">The signature of the object.</param>
    /// <param name="name">The name of the object.</param>
    public ObjectNameSymbol(uint signature, Utf8String name)
    {
        Signature = signature;
        _name = new LazyVariable<Utf8String>(name);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.ObjName;

    /// <summary>
    /// Gets or sets the signature of the object.
    /// </summary>
    public uint Signature
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the name of the object.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => $"S_OBJNAME: {Name}";
}
