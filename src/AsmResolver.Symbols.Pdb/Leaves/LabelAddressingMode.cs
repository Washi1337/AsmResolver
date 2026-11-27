namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining the addressing modes for label types.
/// </summary>
public enum LabelAddressingMode : ushort
{
    /// <summary>
    /// Near addressing mode.
    /// </summary>
    Near = 0,

    /// <summary>
    /// Far addressing mode.
    /// </summary>
    Far = 4,
}
