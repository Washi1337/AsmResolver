using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

public class FieldList : CodeViewLeaf
{
    private IList<CodeViewField>? _fields;

    protected FieldList(uint typeIndex)
        : base(typeIndex)
    {
    }

    public FieldList()
        : base(0)
    {
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.FieldList;

    public IList<CodeViewField> Fields
    {
        get
        {
            if (_fields is null)
                Interlocked.CompareExchange(ref _fields, GetFields(), null);
            return _fields;
        }
    }

    protected virtual IList<CodeViewField> GetFields() => new List<CodeViewField>();
}
