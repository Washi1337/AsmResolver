namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Provides members defining all possible DBI stream format version numbers.
/// </summary>
public enum DbiStreamVersion
{
#pragma warning disable CS1591
    VC41 = 930803,
    V50 = 19960307,
    V60 = 19970606,
    V70 = 19990903,
    V110 = 20091201
#pragma warning restore CS1591
}
