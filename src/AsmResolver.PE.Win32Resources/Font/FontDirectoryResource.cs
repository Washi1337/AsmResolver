using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Font;

/// <summary>
/// Represents a RT_FONTDIR resource containing a directory of font entries.
/// </summary>
public class FontDirectoryResource : IWin32Resource
{
    /// <summary>
    /// Creates a new empty font directory resource.
    /// </summary>
    public FontDirectoryResource()
    {
    }

    /// <summary>
    /// Gets or sets the language identifier of the font directory resource.
    /// </summary>
    public uint Lcid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the list of font directory entries.
    /// </summary>
    public IList<FontDirectoryEntry> Entries
    {
        get;
    } = new List<FontDirectoryEntry>();

    /// <summary>
    /// Reads a font directory resource from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The font directory resource, or <c>null</c> if none was present.</returns>
    public static FontDirectoryResource? FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.FontDirectory, out var fontDirDirectory))
            return null;

        // RT_FONTDIR typically has one entry: ID=1 -> language -> data.
        foreach (var idDir in fontDirDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in idDir.Entries.OfType<ResourceData>())
            {
                if (!language.CanRead)
                    continue;

                var reader = language.CreateReader();
                return ReadFontDirectory(language.Id, ref reader);
            }
        }

        return null;
    }

    private static FontDirectoryResource ReadFontDirectory(uint lcid, ref BinaryStreamReader reader)
    {
        var result = new FontDirectoryResource { Lcid = lcid };

        if (!reader.CanRead(sizeof(ushort)))
            return result;

        ushort numFonts = reader.ReadUInt16();
        if (numFonts == 0)
            return result;

        uint remainingBytes = (uint)(reader.StartOffset + reader.Length - reader.Offset);

        // Determine per-entry size. The FONTDIRENTRY format varies across implementations.
        // Standard format: 2 (ordinal) + 113 (fixed header) + device name + face name.
        // Some implementations (e.g. Wine) use a compact format.
        // We use equal division when entries are uniform, otherwise read remaining for last.
        uint perEntrySize = remainingBytes / (uint)numFonts;

        for (int i = 0; i < numFonts; i++)
        {
            if (!reader.CanRead(sizeof(ushort)))
                break;

            ushort fontOrdinal = reader.ReadUInt16();

            uint rawDataSize;
            if (i < numFonts - 1)
                rawDataSize = perEntrySize - sizeof(ushort);
            else
                rawDataSize = (uint)(reader.StartOffset + reader.Length - reader.Offset);

            if (!reader.CanRead(rawDataSize))
                rawDataSize = (uint)(reader.StartOffset + reader.Length - reader.Offset);

            byte[] rawData = new byte[rawDataSize];
            for (uint j = 0; j < rawDataSize; j++)
                rawData[j] = reader.ReadByte();

            result.Entries.Add(new FontDirectoryEntry(fontOrdinal, new DataSegment(rawData)));
        }

        return result;
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        using var stream = new MemoryStream();
        var writer = new BinaryStreamWriter(stream);

        writer.WriteUInt16((ushort)Entries.Count);

        foreach (var entry in Entries)
        {
            writer.WriteUInt16(entry.FontOrdinal);

            if (entry.RawData is IReadableSegment readable)
            {
                var reader = readable.CreateReader();
                int length = (int)readable.GetPhysicalSize();
                for (int i = 0; i < length; i++)
                    writer.WriteByte(reader.ReadByte());
            }
        }

        var fontDirDirectory = new ResourceDirectory(ResourceType.FontDirectory);
        var idDir = new ResourceDirectory(1);
        idDir.Entries.Add(new ResourceData(Lcid, new DataSegment(stream.ToArray())));
        fontDirDirectory.Entries.Add(idDir);

        rootDirectory.InsertOrReplaceEntry(fontDirDirectory);
    }
}
