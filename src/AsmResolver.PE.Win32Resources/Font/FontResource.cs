using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Font;

/// <summary>
/// Represents a single RT_FONT resource entry containing raw FNT font data.
/// </summary>
public class FontResource : IWin32Resource
{
    /// <summary>
    /// Creates a new font resource.
    /// </summary>
    /// <param name="id">The numeric identifier of the font resource.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="data">The raw FNT data.</param>
    public FontResource(uint id, uint lcid, ISegment data)
    {
        Id = id;
        Lcid = lcid;
        Data = data;
    }

    /// <summary>
    /// Gets the numeric identifier of the font resource.
    /// </summary>
    public uint Id
    {
        get;
    }

    /// <summary>
    /// Gets or sets the language identifier of the font resource.
    /// </summary>
    public uint Lcid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the raw FNT data of the font resource.
    /// </summary>
    public ISegment Data
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the FNT version by reading the first two bytes of the data, or 0 if unavailable.
    /// </summary>
    public ushort Version
    {
        get
        {
            if (Data is IReadableSegment readable && readable.GetPhysicalSize() >= 2)
            {
                var reader = readable.CreateReader();
                return reader.ReadUInt16();
            }

            return 0;
        }
    }

    /// <summary>
    /// Reads all font resources from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>A collection of font resources, or an empty collection if none were present.</returns>
    public static IEnumerable<FontResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Font, out var fontDirectory))
            yield break;

        foreach (var idDir in fontDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in idDir.Entries.OfType<ResourceData>())
            {
                if (language.Contents is not null)
                    yield return new FontResource(idDir.Id, language.Id, language.Contents);
            }
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Font, out var fontDirectory))
        {
            fontDirectory = new ResourceDirectory(ResourceType.Font);
            rootDirectory.InsertOrReplaceEntry(fontDirectory);
        }

        var idDir = new ResourceDirectory(Id);
        idDir.Entries.Add(new ResourceData(Lcid, Data));
        fontDirectory.InsertOrReplaceEntry(idDir);
    }
}
