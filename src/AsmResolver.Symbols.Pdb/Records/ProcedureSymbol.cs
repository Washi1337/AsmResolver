using System.Collections.Generic;
using System.Threading;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents either a local or a global procedure symbol in a PDB file.
/// </summary>
public partial class ProcedureSymbol : CodeViewSymbol, IScopeCodeViewSymbol
{
    private IList<ICodeViewSymbol>? _symbols;

    /// <summary>
    /// Initializes an empty procedure symbol.
    /// </summary>
    protected ProcedureSymbol()
    {
    }

    /// <summary>
    /// Creates a new procedure with the provided identifier.
    /// </summary>
    /// <param name="name">The name of the procedure.</param>
    /// <param name="id">The function identifier of the procedure.</param>
    public ProcedureSymbol(Utf8String name, FunctionIdentifier id)
    {
        Name = name;
        FunctionType = id;
    }

    /// <summary>
    /// Creates a new procedure with the provided procedure type.
    /// </summary>
    /// <param name="name">The name of the procedure.</param>
    /// <param name="type">The type describing the shape of the procedure.</param>
    public ProcedureSymbol(Utf8String name, ProcedureTypeRecord type)
    {
        Name = name;
        FunctionType = type;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType
    {
        get
        {
            if (ProcedureType is not null)
            {
                return IsGlobal
                    ? CodeViewSymbolType.GProc32
                    : CodeViewSymbolType.LProc32;
            }

            return IsGlobal
                ? CodeViewSymbolType.GProc32Id
                : CodeViewSymbolType.LProc32Id;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the symbol is a global procedure symbol.
    /// </summary>
    public bool IsGlobal
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a value indicating whether the symbol is a local procedure symbol.
    /// </summary>
    public bool IsLocal
    {
        get => !IsGlobal;
        set => IsGlobal = !value;
    }

    /// <inheritdoc />
    public IList<ICodeViewSymbol> Symbols
    {
        get
        {
            if (_symbols is null)
                Interlocked.CompareExchange(ref _symbols, GetSymbols(), null);
            return _symbols;
        }
    }

    /// <summary>
    /// Gets or sets the size in bytes of the procedure.
    /// </summary>
    public uint Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the start offset where the debug code or data begins of the procedure.
    /// </summary>
    public uint DebugStartOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the end offset where the debug code or data begins of the procedure.
    /// </summary>
    public uint DebugEndOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type or identifier leaf describing the identifier or shape of the function.
    /// </summary>
    [LazyProperty]
    public partial CodeViewLeaf? FunctionType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the function identifier of the procedure (if available).
    /// </summary>
    public FunctionIdentifier? FunctionId
    {
        get => FunctionType as FunctionIdentifier;
        set => FunctionType = value;
    }

    /// <summary>
    /// Gets or sets the type describing the shape of the procedure (if available).
    /// </summary>
    public ProcedureTypeRecord? ProcedureType
    {
        get => FunctionType as ProcedureTypeRecord;
        set => FunctionType = value;
    }

    /// <summary>
    /// Gets or sets the segment index in which the procedure is defined in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the segment in which the procedure starts at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes describing the nature of the procedure.
    /// </summary>
    public ProcedureAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the procedure.
    /// </summary>
    [LazyProperty]
    public partial Utf8String? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the sub-symbols defined in this procedure.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<ICodeViewSymbol> GetSymbols() => new List<ICodeViewSymbol>();

    /// <summary>
    /// Obtains the function type of this procedure.
    /// </summary>
    /// <returns>The function type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="FunctionType"/> property.
    /// </remarks>
    protected virtual CodeViewLeaf? GetFunctionType() => null;

    /// <summary>
    /// Obtains the name of this procedure.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        string prefix = CodeViewSymbolType.ToString().ToUpper();
        return $"S_{prefix}: [{SegmentIndex:X4}:{Offset:X8}] {Name} ({FunctionType})";
    }
}
