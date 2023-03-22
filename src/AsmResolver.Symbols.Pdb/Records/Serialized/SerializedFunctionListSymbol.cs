using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="FunctionListSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedFunctionListSymbol : FunctionListSymbol
{
    private readonly int _count;
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _entriesReader;

    /// <summary>
    /// Reads a function list symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="isCallers">Indicates the functions in the list are callers of the procedure.</param>
    public SerializedFunctionListSymbol(PdbReaderContext context, BinaryStreamReader reader, bool isCallers)
    {
        IsCallersList = isCallers;
        _context = context;
        _count = reader.ReadInt32();

        _entriesReader = reader;
    }

    /// <inheritdoc />
    protected override IList<FunctionCountPair> GetEntries()
    {
        var result = new List<FunctionCountPair>(_count);

        var indexReader = _entriesReader.Fork();
        var countReader = _entriesReader.ForkRelative((uint) (_count + 1) * sizeof(int));

        for (int i = 0; i < _count; i++)
        {
            uint typeIndex = indexReader.ReadUInt32();
            int count = countReader.CanRead(sizeof(int))
                ? countReader.ReadInt32()
                : 0;

            var function = _context.ParentImage.TryGetIdLeafRecord(typeIndex, out FunctionIdentifier? type)
                ? type
                : _context.Parameters.ErrorListener.BadImageAndReturn<FunctionIdentifier>(
                    $"Function list contains an invalid type index {typeIndex:X8}.");

            result.Add(new FunctionCountPair(function, count));
        }

        return result;
    }
}
