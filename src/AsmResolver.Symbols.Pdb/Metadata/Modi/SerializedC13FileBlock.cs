using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Provides a lazy implementation of <see cref="C13FileBlock"/> that is read from an input file.
/// </summary>
public class SerializedC13FileBlock : C13FileBlock
{
    private readonly uint _linesCount;
    private readonly BinaryStreamReader _linesReader;
    private readonly BinaryStreamReader? _columnsReader;

    /// <summary>
    /// Reads a C13 file block from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <param name="hasColumns">Indicates if the block contains columns.</param>
    public SerializedC13FileBlock(ref BinaryStreamReader reader, bool hasColumns)
    {
        FileId = reader.ReadUInt32();
        _linesCount = reader.ReadUInt32();
        uint blockSize = reader.ReadUInt32();

        _linesReader = reader.Fork();
        if (hasColumns)
        {
            _columnsReader = reader.Fork();
        }

        reader.Offset += blockSize - 3 * sizeof(uint);
    }

    /// <inheritdoc />
    protected override IList<C13Line> GetLines()
    {
        var result = new List<C13Line>();

        var reader = _linesReader;
        for (int i = 0; i < _linesCount; i++)
            result.Add(C13Line.FromReader(ref reader));

        return result;
    }

    /// <inheritdoc />
    protected override IList<C13Column> GetColumns()
    {
        var result = new List<C13Column>();

        if (_columnsReader is null)
            return result;

        var reader = _columnsReader.Value;
        for (int i = 0; i < _linesCount; i++)
            result.Add(C13Column.FromReader(ref reader));

        return result;
    }
}
