using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Provides a lazy implementation of <see cref="C13LinesSection"/> that is read from an input file.
/// </summary>
public class SerializedC13LinesSection : C13LinesSection
{
    private readonly C13LinesAttributes _originalAttributes;
    private readonly BinaryStreamReader _blocksReader;

    /// <summary>
    /// Reads a C13 lines section from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    public SerializedC13LinesSection(BinaryStreamReader reader)
    {
        SectionContributionOffset = reader.ReadUInt32();
        SectionContributionIndex = reader.ReadUInt16();
        Attributes = _originalAttributes = (C13LinesAttributes) reader.ReadUInt16();
        CodeSize = reader.ReadUInt32();

        _blocksReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override IList<C13FileBlock> GetBlocks()
    {
        var result = new List<C13FileBlock>();

        var reader = _blocksReader;

        while (reader.CanRead(C13FileBlock.HeaderSize))
            result.Add(new SerializedC13FileBlock(ref reader, (_originalAttributes & C13LinesAttributes.HasColumns) != 0));

        return result;
    }
}
