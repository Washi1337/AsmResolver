namespace AsmResolver.Symbols.Pdb.Leaves;

public abstract class CodeViewType : CodeViewLeaf
{
    /// <inheritdoc />
    protected CodeViewType(uint typeIndex)
        : base(typeIndex)
    {
    }
}
