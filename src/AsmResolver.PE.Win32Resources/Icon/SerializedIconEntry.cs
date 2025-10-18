using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Provides a lazy-initialized implementation of <see cref="IconEntry"/> that reads from an existing resource.
/// </summary>
public class SerializedIconEntry : IconEntry
{
    private readonly ResourceDirectory _iconsDirectory;
    private readonly ushort _originalId;
    private readonly uint _originalLcid;

    /// <summary>
    /// Reads an icon entry from the provided input stream and corresponding icons directory.
    /// </summary>
    /// <param name="lcid">The language specifier.</param>
    /// <param name="iconsDirectory">The icons directory.</param>
    /// <param name="reader">The input stream.</param>
    public SerializedIconEntry(uint lcid, ResourceDirectory iconsDirectory, ref BinaryStreamReader reader)
    {
        _iconsDirectory = iconsDirectory;

        Width = reader.ReadByte();
        Height = reader.ReadByte();
        ColorCount = reader.ReadByte();
        Reserved = reader.ReadByte();
        Planes = reader.ReadUInt16();
        BitsPerPixel = reader.ReadUInt16();
        _ = reader.ReadUInt32(); // dwBytesInRes.
        Id = _originalId = reader.ReadUInt16();
        Lcid = _originalLcid = lcid;
    }

    /// <inheritdoc />
    protected override ISegment? GetPixelData()
    {
        if (!_iconsDirectory.TryGetDirectory(_originalId, out var directory))
            return null;

        if (!directory.TryGetData(_originalLcid, out var iconData))
            return null;

        return iconData.Contents;
    }
}
