using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Represents a single icon group.
/// </summary>
public class IconGroup
{
    private IList<IconEntry>? _icons;

    /// <summary>
    /// Creates a new icon group.
    /// </summary>
    /// <param name="id">The identifier of the icon group.</param>
    /// <param name="lcid">The language specifier of the icon group.</param>
    public IconGroup(uint id, uint lcid)
    {
        Id = id;
        Lcid = lcid;
    }

    /// <summary>
    /// Creates a new icon group.
    /// </summary>
    /// <param name="name">The identifier of the icon group.</param>
    /// <param name="lcid">The language specifier of the icon group.</param>
    public IconGroup(string name, uint lcid)
    {
        Name = name;
        Lcid = lcid;
    }

    /// <summary>
    /// Gets or sets the identifier of the icon.
    /// </summary>
    /// <remarks>
    /// This value has no meaning if <see cref="Name"/> is not <c>null</c>.
    /// </remarks>
    public uint Id
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the icon. When <c>null</c>, <see cref="Id"/> is used instead.
    /// </summary>
    public string? Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the language specifier of the group.
    /// </summary>
    public uint Lcid
    {
        get;
    }

    /// <summary>
    /// Reserved, should be zero.
    /// </summary>
    public ushort Reserved
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of icon that is stored in the group.
    /// </summary>
    public IconType Type
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the icons stored in the group.
    /// </summary>
    public IList<IconEntry> Icons
    {
        get
        {
            if (_icons is null)
                Interlocked.CompareExchange(ref _icons, GetIcons(), null);
            return _icons;
        }
    }

    /// <summary>
    /// Obtains the icons stored in the group.
    /// </summary>
    /// <returns>The icons.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Icons"/> property.
    /// </remarks>
    protected virtual IList<IconEntry> GetIcons() => new List<IconEntry>();

    /// <summary>
    /// Serializes the icon group and inserts all icons in the group into the provided resource directory.
    /// </summary>
    /// <param name="entryDirectory">The icon entry directory.</param>
    /// <param name="writer">The output stream.</param>
    public void Write(ResourceDirectory entryDirectory, BinaryStreamWriter writer)
    {
        writer.WriteUInt16(Reserved);
        writer.WriteUInt16((ushort) Type);
        writer.WriteUInt16((ushort) Icons.Count);

        for (int i = 0; i < Icons.Count; i++)
            Icons[i].Write(entryDirectory, writer);
    }
}
