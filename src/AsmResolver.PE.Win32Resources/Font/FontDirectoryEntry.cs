using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Font;

/// <summary>
/// Represents a single entry in a RT_FONTDIR resource.
/// </summary>
public class FontDirectoryEntry
{
    /// <summary>
    /// The size in bytes of the fixed-length portion of the FONTDIRENTRY structure.
    /// </summary>
    public const int FixedHeaderSize = 113;

    /// <summary>
    /// Creates a new font directory entry.
    /// </summary>
    /// <param name="fontOrdinal">The ordinal matching the RT_FONT resource ID.</param>
    /// <param name="rawData">The raw FONTDIRENTRY data including device name and face name.</param>
    public FontDirectoryEntry(ushort fontOrdinal, ISegment rawData)
    {
        FontOrdinal = fontOrdinal;
        RawData = rawData;
    }

    /// <summary>
    /// Gets or sets the ordinal matching the RT_FONT resource ID.
    /// </summary>
    public ushort FontOrdinal
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the raw FONTDIRENTRY data including device name and face name strings.
    /// </summary>
    public ISegment RawData
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the font version from the raw data, or 0 if unavailable.
    /// </summary>
    public ushort FontVersion
    {
        get
        {
            if (RawData is IReadableSegment readable && readable.GetPhysicalSize() >= 2)
            {
                var reader = readable.CreateReader();
                return reader.ReadUInt16();
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the face name from the raw data by scanning past the fixed header and device name string,
    /// or <c>null</c> if unavailable.
    /// </summary>
    public string? FaceName
    {
        get
        {
            if (RawData is not IReadableSegment readable || readable.GetPhysicalSize() <= FixedHeaderSize)
                return null;

            var reader = readable.CreateReader();
            reader.Offset += (ulong)FixedHeaderSize;

            // Skip device name (null-terminated ANSI string).
            while (reader.CanRead(1))
            {
                if (reader.ReadByte() == 0)
                    break;
            }

            // Read face name (null-terminated ANSI string).
            ulong start = reader.Offset;
            while (reader.CanRead(1))
            {
                if (reader.ReadByte() == 0)
                {
                    int length = (int)(reader.Offset - start - 1);
                    reader.Offset = start;
                    byte[] bytes = new byte[length];
                    for (int i = 0; i < length; i++)
                        bytes[i] = reader.ReadByte();
                    return System.Text.Encoding.ASCII.GetString(bytes);
                }
            }

            return null;
        }
    }
}
