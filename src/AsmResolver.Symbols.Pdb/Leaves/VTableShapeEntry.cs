namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all possible types that a single entry in a <see cref="VTableShape"/> can be.
/// </summary>
public enum VTableShapeEntry : byte
{
#pragma warning disable CS1591
    Near = 0x00,
    Far = 0x01,
    Thin = 0x02,
    Outer = 0x03,
    Meta = 0x04,
    Near32 = 0x05,
    Far32 = 0x06,
#pragma warning restore CS1591
}
