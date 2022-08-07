namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Provides members defining all valid versions of the Section Contribution sub stream's file format.
/// </summary>
public enum SectionContributionStreamVersion : uint
{
    /// <summary>
    /// Indicates version 6.0 is used.
    /// </summary>
    Ver60 = 0xeffe0000 + 19970605,

    /// <summary>
    /// Indicates version 2.0 is used.
    /// </summary>
    V2 = 0xeffe0000 + 20140516
}
