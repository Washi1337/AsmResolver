namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members describing all possible opcodes that a binary annotation can use.
/// </summary>
public enum BinaryAnnotationOpCode
{
#pragma warning disable CS1591
    Illegal,
    CodeOffset,
    ChangeCodeOffsetBase,
    ChangeCodeOffset,
    ChangeCodeLength,
    ChangeFile,
    ChangeLineOffset,
    ChangeLineEndDelta,
    ChangeRangeKind,
    ChangeColumnStart,
    ChangeColumnEndDelta,
    ChangeCodeOffsetAndLineOffset,
    ChangeCodeLengthAndCodeOffset,
    ChangeColumnEnd,
#pragma warning restore CS1591
}
