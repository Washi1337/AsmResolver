using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Provides a lazy-initialized implementation of <see cref="IconGroup"/> that reads its icons from an existing resource.
/// </summary>
public class SerializedIconGroup : IconGroup
{
    private readonly ResourceDirectory _iconDirectory;
    private readonly ushort _count;
    private readonly BinaryStreamReader _itemReader;

    /// <summary>
    /// Reads an icon group from the provided icon directory and input stream.
    /// </summary>
    /// <param name="id">The group identifier.</param>
    /// <param name="lcid">The group language identifier.</param>
    /// <param name="iconDirectory">The directory containing the individual icons.</param>
    /// <param name="reader">The input stream.</param>
    public SerializedIconGroup(uint id, uint lcid, ResourceDirectory iconDirectory, BinaryStreamReader reader)
        : base(id, lcid)
    {
        _iconDirectory = iconDirectory;

        Reserved = reader.ReadUInt16();
        Type = (IconType) reader.ReadUInt16();
        _count = reader.ReadUInt16();
        _itemReader = reader;
    }

    /// <inheritdoc />
    protected override IList<IconEntry> GetIcons()
    {
        var result = new List<IconEntry>();

        var reader = _itemReader;
        for (int i = 0; i < _count; i++)
            result.Add(new SerializedIconEntry(Lcid, _iconDirectory, ref reader));

        return result;
    }
}
