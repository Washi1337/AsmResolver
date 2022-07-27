namespace AsmResolver.Symbols.Pdb.Leaves;

public abstract class CodeViewField : CodeViewLeaf
{
    /// <inheritdoc />
    protected CodeViewField(uint typeIndex)
        : base(typeIndex)
    {
    }
}
