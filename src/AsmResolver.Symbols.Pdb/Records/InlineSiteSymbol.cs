using System.Collections.Generic;
using System.Threading;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides information about an inlined call site.
/// </summary>
public class InlineSiteSymbol : CodeViewSymbol, IScopeCodeViewSymbol
{
    private readonly LazyVariable<FunctionIdLeaf?> _inlinee;

    private IList<ICodeViewSymbol>? _symbols;
    private IList<BinaryAnnotation>? _annotations;

    /// <summary>
    /// Initializes an empty inline call site symbol.
    /// </summary>
    protected InlineSiteSymbol()
    {
        _inlinee = new LazyVariable<FunctionIdLeaf?>(GetInlinee);
    }

    /// <summary>
    /// Creates a new inline site symbol.
    /// </summary>
    /// <param name="inlinee">The function that is being inlined.</param>
    public InlineSiteSymbol(FunctionIdLeaf inlinee)
    {
        _inlinee = new LazyVariable<FunctionIdLeaf?>(inlinee);
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.InlineSite;

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
    /// Gets or sets the identifier of the function that is being inlined.
    /// </summary>
    public FunctionIdLeaf? Inlinee
    {
        get => _inlinee.Value;
        set => _inlinee.Value = value;
    }

    /// <summary>
    /// Gets a collection of annotations to the instruction stream.
    /// </summary>
    public IList<BinaryAnnotation> Annotations
    {
        get
        {
            if (_annotations is null)
                Interlocked.CompareExchange(ref _annotations, GetAnnotations(), null);
            return _annotations;
        }
    }

    /// <summary>
    /// Obtains the inlinee.
    /// </summary>
    /// <returns>The inlinee</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Inlinee"/> property.
    /// </remarks>
    protected virtual FunctionIdLeaf? GetInlinee() => null;

    /// <summary>
    /// Obtains the sub-symbols defined in this inline site.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<ICodeViewSymbol> GetSymbols() => new List<ICodeViewSymbol>();

    /// <summary>
    /// Obtains the collection of annotations added to the instruction stream.
    /// </summary>
    /// <returns>The annotations.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Annotations"/> property.
    /// </remarks>
    protected virtual IList<BinaryAnnotation> GetAnnotations() => new List<BinaryAnnotation>();
}
