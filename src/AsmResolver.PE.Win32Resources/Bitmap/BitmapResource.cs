using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Bitmap;

/// <summary>
/// Provides a high level view on a bitmap resource (RT_BITMAP) stored in a PE image.
/// </summary>
public class BitmapResource : IWin32Resource
{
    /// <summary>
    /// Creates a new bitmap resource.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="data">The raw DIB data.</param>
    public BitmapResource(uint id, ISegment data)
    {
        Id = id;
        Data = data;
    }

    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    public uint Lcid { get; set; }

    /// <summary>
    /// Gets or sets the raw DIB data (BITMAPINFOHEADER + pixel data, without BITMAPFILEHEADER).
    /// </summary>
    public ISegment Data { get; set; }

    /// <summary>
    /// Gets the bitmap width in pixels, read from the BITMAPINFOHEADER.
    /// </summary>
    public int Width
    {
        get
        {
            if (Data is IReadableSegment readable && readable.CreateReader() is var reader && reader.Length >= 8)
            {
                reader.Offset += 4; // skip biSize
                return reader.ReadInt32();
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the bitmap height in pixels, read from the BITMAPINFOHEADER.
    /// </summary>
    public int Height
    {
        get
        {
            if (Data is IReadableSegment readable && readable.CreateReader() is var reader && reader.Length >= 12)
            {
                reader.Offset += 8; // skip biSize + biWidth
                return reader.ReadInt32();
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the bits per pixel, read from the BITMAPINFOHEADER.
    /// </summary>
    public ushort BitCount
    {
        get
        {
            if (Data is IReadableSegment readable && readable.CreateReader() is var reader && reader.Length >= 16)
            {
                reader.Offset += 14; // skip biSize + biWidth + biHeight + biPlanes
                return reader.ReadUInt16();
            }

            return 0;
        }
    }

    /// <summary>
    /// Reads all bitmap resources from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The bitmap resources.</returns>
    public static IEnumerable<BitmapResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Bitmap, out var bitmapDirectory))
            yield break;

        foreach (var entry in bitmapDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in entry.Entries.OfType<ResourceData>())
            {
                ISegment data = language.Contents is not null
                    ? language.Contents
                    : new DataSegment(new byte[0]);

                yield return new BitmapResource(entry.Id, data)
                {
                    Lcid = language.Id,
                };
            }
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Bitmap, out var bitmapDirectory))
        {
            bitmapDirectory = new ResourceDirectory(ResourceType.Bitmap);
            rootDirectory.InsertOrReplaceEntry(bitmapDirectory);
        }

        // Remove existing entry with same ID if present.
        for (int i = bitmapDirectory.Entries.Count - 1; i >= 0; i--)
        {
            if (bitmapDirectory.Entries[i].Id == Id)
            {
                bitmapDirectory.Entries.RemoveAt(i);
                break;
            }
        }

        var entryDir = new ResourceDirectory(Id);
        entryDir.Entries.Add(new ResourceData(Lcid, Data));
        bitmapDirectory.Entries.Add(entryDir);
    }
}
