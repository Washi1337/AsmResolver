using System.Collections.Generic;

namespace AsmResolver.Symbols.Pdb.Leaves;

public class BuildInfoLeaf : CodeViewLeaf
{
    protected BuildInfoLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BuildInfo;

    public IList<string> Entries
    {
        get;
    }

    protected virtual IList<string> GetEntries() => new List<string>();
}
