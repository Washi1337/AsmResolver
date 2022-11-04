namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a procedure reference symbol stored in a PDB symbol stream.
/// </summary>
public class ProcedureReferenceSymbol : CodeViewSymbol
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly bool _local;

    /// <summary>
    /// Initializes a new empty symbol.
    /// </summary>
    /// <param name="local">If true, this represents a local procedure reference.</param>
    protected ProcedureReferenceSymbol(bool local)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _local = local;
    }

    /// <summary>
    /// Creates a new symbol.
    /// </summary>
    /// <param name="checksum">The checksum of the referenced symbol name.</param>
    /// <param name="offset">The offset within the segment the symbol starts at.</param>
    /// <param name="module">Index of the module that contains this procedure record.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="local">If true, this represents a local procedure reference.</param>
    public ProcedureReferenceSymbol(uint checksum, uint offset, ushort module, Utf8String name, bool local)
    {
        Checksum = checksum;
        Offset = offset;
        Module = module;
        _name = new LazyVariable<Utf8String>(name);
        _local = local;
    }

    /// <inheritdoc/>
    public override CodeViewSymbolType CodeViewSymbolType
    {
        get
        {
            return _local ? CodeViewSymbolType.LProcRef : CodeViewSymbolType.ProcRef;
        }
    }

    /// <summary>
    /// Is the symbol a Local Procedure Reference?
    /// </summary>
    public bool IsLocal => _local;

    /// <summary>
    /// Gets the checksum of the referenced symbol name. The checksum used is the
    /// one specified in the header of the global symbols stream or static symbols stream.
    /// </summary>
    public uint Checksum
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the offset of the procedure symbol record from the beginning of the
    /// $$SYMBOL table for the module.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Index of the module that contains this procedure record.
    /// </summary>
    public ushort Module
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the symbol.
    /// </summary>
    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the name of the symbol.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => $"{CodeViewSymbolType}: [{Module:X4}:{Offset:X8}] {Name}";
}
