using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Represents a single icon entry in an icon group.
/// </summary>
public class IconEntry
{
    private readonly LazyVariable<IconEntry, ISegment?> _pixelData;

    /// <summary>
    /// Initializes an empty icon entry.
    /// </summary>
    protected IconEntry()
    {
        _pixelData = new LazyVariable<IconEntry, ISegment?>(x => x.GetData());
    }

    /// <summary>
    /// Creates a new icon entry with the provided identifier and language specifier.
    /// </summary>
    /// <param name="id">The identifier of the icon.</param>
    /// <param name="lcid">The language identifier.</param>
    public IconEntry(ushort id, uint lcid)
        : this()
    {
        Id = id;
        Lcid = lcid;
    }

    /// <summary>
    /// Gets or sets the identifier of the icon.
    /// </summary>
    public ushort Id
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the language identifier of the icon.
    /// </summary>
    public uint Lcid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the width of the icon in pixels. A width of 0 indicates 256 pixels.
    /// </summary>
    public byte Width
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the height of the icon in pixels. A width of 0 indicates 256 pixels.
    /// </summary>
    public byte Height
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of colors in the color palette. Should be 0 if the image does not use a color palette.
    /// </summary>
    public byte ColorCount
    {
        get;
        set;
    }

    /// <summary>
    /// Reserved, should be zero.
    /// </summary>
    public byte Reserved
    {
        get;
        set;
    }

    /// <summary>
    /// When the icon is in ICO format: Gets or sets the number of color planes. Should be 0 or 1.
    /// When the icon is in CUR format: Gets or sets the horizontal coordinates of the hotspot in number of pixels from
    /// the left.
    /// </summary>
    public ushort Planes
    {
        get;
        set;
    }

    /// <summary>
    /// When the icon is in ICO format: Gets or sets the number of bits per pixel.
    /// When the icon is in CUR format: Gets or sets the vertical coordinates of the hotspot in number of pixels from
    /// the top.
    /// </summary>
    public ushort BitsPerPixel
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the raw pixel data of the icon.
    /// </summary>
    public ISegment? PixelData
    {
        get => _pixelData.GetValue(this);
        set => _pixelData.SetValue(value);
    }

    /// <summary>
    /// Obtains the raw pixel data of the icon.
    /// </summary>
    /// <returns>The data.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="PixelData"/> property.
    /// </remarks>
    protected virtual ISegment? GetData() => null;

    /// <summary>
    /// Serializes the icon entry to the provided output stream and inserts the icon pixel data into the provided
    /// icons resource directory.
    /// </summary>
    /// <param name="entryDirectory">The icon entry directory to insert into.</param>
    /// <param name="writer">The output stream.</param>
    /// <exception cref="ArgumentException">Occurs when there is already an icon added with the same ID.</exception>
    /// <exception cref="ArgumentNullException">Occurs when <see cref="PixelData"/> is <c>nulL</c>.</exception>
    public void Write(ResourceDirectory entryDirectory, BinaryStreamWriter writer)
    {
        writer.WriteByte(Width);
        writer.WriteByte(Height);
        writer.WriteByte(ColorCount);
        writer.WriteByte(Reserved);
        writer.WriteUInt16(Planes);
        writer.WriteUInt16(BitsPerPixel);
        writer.WriteUInt32(PixelData?.GetPhysicalSize() ?? 0);
        writer.WriteUInt16(Id);

        if (entryDirectory.TryGetEntry(Id, out _))
            throw new ArgumentException($"Duplicate icon resource with ID {Id}.");

        if (PixelData is null)
            throw new ArgumentNullException($"Icon resource ID {Id} has no pixel data.");

        var entry = new ResourceDirectory(Id);
        entry.Entries.Add(new ResourceData(Lcid, PixelData));
        entryDirectory.InsertOrReplaceEntry(entry);
    }
}
