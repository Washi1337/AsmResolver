using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="InlineSiteSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedInlineSiteSymbol : InlineSiteSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _inlineeIndex;

    private readonly BinaryStreamReader _annotationsReader;

    /// <summary>
    /// Reads a function list symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedInlineSiteSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        _ = reader.ReadUInt32(); // pParent.
        _ = reader.ReadUInt32(); // pEnd.
        _inlineeIndex = reader.ReadUInt32();

        _annotationsReader = reader;
    }

    /// <inheritdoc />
    protected override FunctionIdLeaf? GetInlinee()
    {
        return _context.ParentImage.TryGetIdLeafRecord(_inlineeIndex, out FunctionIdLeaf? id)
            ? id
            : _context.Parameters.ErrorListener.BadImageAndReturn<FunctionIdLeaf>(
                $"Inline Site symbol contains an invalid type index {_inlineeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override IList<BinaryAnnotation> GetAnnotations()
    {
        var result = new List<BinaryAnnotation>();

        var reader = _annotationsReader.Fork();
        while (reader.TryReadCompressedUInt32(out uint raw))
        {
            var opCode = (BinaryAnnotationOpCode) raw;

            // All opcodes have at least one operand.
            if (!reader.TryReadCompressedUInt32(out uint operand1))
            {
                _context.Parameters.ErrorListener.BadImage(
                    $"Expected first operand of binary annotation at offset {reader.Offset:X8}.");
                break;
            }

            // Check if we need to read a second operand.
            uint operand2 = 0;
            if (opCode == BinaryAnnotationOpCode.ChangeCodeLengthAndCodeOffset
                && !reader.TryReadCompressedUInt32(out operand2))
            {
                _context.Parameters.ErrorListener.BadImage(
                    $"Expected second operand of binary annotation at offset {reader.Offset:X8}.");
                break;
            }

            // Add to result.
            result.Add(new BinaryAnnotation(opCode, operand1, operand2));
        }

        return result;
    }
}
