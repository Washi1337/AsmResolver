namespace AsmResolver.Symbols.Pdb.Metadata.Info;

/// <summary>
/// Provides members defining all possible stream file format versions that PDB defines.
/// </summary>
public enum InfoStreamVersion
{
#pragma warning disable CS1591
    VC2 = 19941610,
    VC4 = 19950623,
    VC41 = 19950814,
    VC50 = 19960307,
    VC98 = 19970604,
    VC70Dep = 19990604,
    VC70 = 20000404,
    VC80 = 20030901,
    VC110 = 20091201,
    VC140 = 20140508,
#pragma warning restore CS1591
}
