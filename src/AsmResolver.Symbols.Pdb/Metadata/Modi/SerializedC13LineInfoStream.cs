using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Provides a lazy implementation of <see cref="C13LineInfoStream"/> that is read from an input file.
/// </summary>
public class SerializedC13LineInfoStream : C13LineInfoStream
{
    private readonly BinaryStreamReader _sectionsReader;

    /// <summary>
    /// Reads a C13 line info stream from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    public SerializedC13LineInfoStream(BinaryStreamReader reader)
    {
        _sectionsReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override IList<C13SubSection> GetSections()
    {
        var result = new List<C13SubSection>();

        var reader = _sectionsReader;

        while (reader.CanRead(1))
        {
            uint type = reader.ReadUInt32();
            uint length = reader.ReadUInt32();

            var sectionType = (C13SubSectionType) (type & 0x7FFFFFFF);
            var contentsReader = reader.ForkRelative(reader.RelativeOffset, length);
            C13SubSection section = sectionType switch
            {
                C13SubSectionType.FileChecksums => new SerializedC13FileChecksumsSection(contentsReader),
                C13SubSectionType.Lines => new SerializedC13LinesSection(contentsReader),
                _ => new CustomC13SubSection(sectionType, contentsReader.ReadSegment(length)),
            };

            result.Add(section);

            reader.Offset += length;
        }

        return result;
    }
}
