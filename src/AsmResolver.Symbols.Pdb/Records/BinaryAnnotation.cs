namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single binary annotation within an inline call site.
/// </summary>
/// <param name="OpCode">The opcode.</param>
/// <param name="Operand1">The first operand.</param>
/// <param name="Operand2">The second operand (if applicable).</param>
public record struct BinaryAnnotation(BinaryAnnotationOpCode OpCode, uint Operand1, uint Operand2)
{
    /// <summary>
    /// Gets the number of operands this binary annotation contains.
    /// </summary>
    public int OperandCount => OpCode == BinaryAnnotationOpCode.ChangeCodeLengthAndCodeOffset ? 2 : 1;

    /// <inheritdoc />
    public override string ToString() => OperandCount == 1
        ? $"{OpCode} {Operand1:X}"
        : $"{OpCode} {Operand1:X} {Operand2:X}";
}
