using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records;

public class BuildInfoSymbol : CodeViewSymbol
{
    private readonly LazyVariable<BuildInfoLeaf?> _info;

    protected BuildInfoSymbol()
    {
        _info = new LazyVariable<BuildInfoLeaf?>(GetInfo);
    }

    public BuildInfoSymbol(BuildInfoLeaf info)
    {
        _info = new LazyVariable<BuildInfoLeaf?>(info);
    }

    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.BuildInfo;

    public BuildInfoLeaf? Info
    {
        get => _info.Value;
        set => _info.Value = value;
    }

    protected virtual BuildInfoLeaf? GetInfo() => null;
}
