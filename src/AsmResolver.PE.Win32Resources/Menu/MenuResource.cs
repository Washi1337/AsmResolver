using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Menu;

/// <summary>
/// Represents a native menu resource (RT_MENU) stored in a PE image. Supports both standard MENU and extended MENUEX
/// formats.
/// </summary>
public class MenuResource : IWin32Resource
{
    private const ushort MfPopup = 0x0010;
    private const ushort MfEnd = 0x0080;

    /// <summary>
    /// Creates a new empty menu resource.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    public MenuResource(uint id)
    {
        Id = id;
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
    /// Gets or sets whether this menu uses the extended MENUEX format.
    /// </summary>
    public bool IsExtended { get; set; }

    /// <summary>
    /// Gets or sets the root help identifier (MENUEX only).
    /// </summary>
    public uint HelpId { get; set; }

    /// <summary>
    /// Gets the collection of top-level menu items.
    /// </summary>
    public IList<MenuItem> Items { get; } = new List<MenuItem>();

    /// <summary>
    /// Reads all menu resources from the provided root win32 resources directory.
    /// </summary>
    /// <param name="rootDirectory">The root resources directory to extract menu resources from.</param>
    /// <returns>The collection of menu resources.</returns>
    public static IEnumerable<MenuResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Menu, out var menuDirectory))
            yield break;

        foreach (var idDir in menuDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var langData in idDir.Entries.OfType<ResourceData>())
            {
                if (!langData.CanRead)
                    continue;

                var reader = langData.CreateReader();
                var resource = FromReader(idDir.Id, langData.Id, ref reader);
                yield return resource;
            }
        }
    }

    /// <summary>
    /// Reads a menu resource from an input stream.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed menu resource.</returns>
    public static MenuResource FromReader(uint id, uint lcid, ref BinaryStreamReader reader)
    {
        ushort version = reader.ReadUInt16();

        var resource = new MenuResource(id) { Lcid = lcid };

        if (version == 0)
        {
            // Standard MENU
            resource.IsExtended = false;
            reader.ReadUInt16(); // wOffset (skip)
            ReadStandardItems(ref reader, resource.Items);
        }
        else if (version == 1)
        {
            // Extended MENUEX
            resource.IsExtended = true;
            reader.ReadUInt16(); // wOffset
            resource.HelpId = reader.ReadUInt32();
            ReadExtendedItems(ref reader, resource.Items);
        }
        else
        {
            throw new FormatException($"Unsupported menu version: {version}.");
        }

        return resource;
    }

    private static void ReadStandardItems(ref BinaryStreamReader reader, IList<MenuItem> items)
    {
        while (reader.CanRead(2))
        {
            ushort flags = reader.ReadUInt16();
            bool isPopup = (flags & MfPopup) != 0;
            bool isEnd = (flags & MfEnd) != 0;

            var item = new MenuItem { Flags = flags };

            if (!isPopup)
            {
                item.Id = reader.ReadUInt16();
            }

            item.Text = reader.ReadUnicodeString();

            if (isPopup)
            {
                item.Type = MenuItemType.Popup;
                ReadStandardItems(ref reader, item.SubItems);
            }
            else if (string.IsNullOrEmpty(item.Text) && item.Id == 0)
            {
                item.Type = MenuItemType.Separator;
            }
            else
            {
                item.Type = MenuItemType.Normal;
            }

            items.Add(item);

            if (isEnd)
                break;
        }
    }

    private static void ReadExtendedItems(ref BinaryStreamReader reader, IList<MenuItem> items)
    {
        while (reader.CanRead(14))
        {
            reader.Align(4);

            uint dwType = reader.ReadUInt32();
            uint dwState = reader.ReadUInt32();
            uint dwMenuId = reader.ReadUInt32();
            ushort bResInfo = reader.ReadUInt16();

            string text = reader.ReadUnicodeString();

            bool isPopup = (bResInfo & 0x01) != 0;
            bool isEnd = (bResInfo & 0x80) != 0;

            var item = new MenuItem
            {
                TypeFlags = dwType,
                State = dwState,
                Id = dwMenuId,
                Text = text
            };

            if (isPopup)
            {
                item.Type = MenuItemType.Popup;
                reader.Align(4);
                item.HelpId = reader.ReadUInt32();
                ReadExtendedItems(ref reader, item.SubItems);
            }
            else if ((dwType & 0x800) != 0)
            {
                item.Type = MenuItemType.Separator;
            }
            else
            {
                item.Type = MenuItemType.Normal;
            }

            items.Add(item);

            if (isEnd)
                break;
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Menu, out var menuDirectory))
        {
            menuDirectory = new ResourceDirectory(ResourceType.Menu);
            rootDirectory.InsertOrReplaceEntry(menuDirectory);
        }

        if (!menuDirectory.TryGetDirectory(Id, out var idDirectory))
        {
            idDirectory = new ResourceDirectory(Id);
            menuDirectory.InsertOrReplaceEntry(idDirectory);
        }

        using var stream = new MemoryStream();
        var writer = new BinaryStreamWriter(stream);
        Write(writer);

        idDirectory.InsertOrReplaceEntry(new ResourceData(Lcid, new DataSegment(stream.ToArray())));
    }

    private void Write(BinaryStreamWriter writer)
    {
        if (IsExtended)
            WriteExtended(writer);
        else
            WriteStandard(writer);
    }

    private void WriteStandard(BinaryStreamWriter writer)
    {
        writer.WriteUInt16(0); // wVersion
        writer.WriteUInt16(0); // wOffset

        WriteStandardItems(writer, Items);
    }

    private static void WriteStandardItems(BinaryStreamWriter writer, IList<MenuItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ushort flags = item.Flags;

            // Set MF_END on last item
            if (i == items.Count - 1)
                flags |= MfEnd;
            else
                flags = (ushort)(flags & ~MfEnd);

            // Set MF_POPUP for popup items
            if (item.Type == MenuItemType.Popup)
                flags |= MfPopup;

            writer.WriteUInt16(flags);

            if (item.Type != MenuItemType.Popup)
                writer.WriteUInt16((ushort)item.Id);

            WriteUnicodeString(writer, item.Text ?? string.Empty);

            if (item.Type == MenuItemType.Popup)
                WriteStandardItems(writer, item.SubItems);
        }
    }

    private void WriteExtended(BinaryStreamWriter writer)
    {
        writer.WriteUInt16(1); // wVersion
        writer.WriteUInt16(4); // wOffset
        writer.WriteUInt32(HelpId);

        WriteExtendedItems(writer, Items);
    }

    private static void WriteExtendedItems(BinaryStreamWriter writer, IList<MenuItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            writer.Align(4);

            writer.WriteUInt32(item.TypeFlags);
            writer.WriteUInt32(item.State);
            writer.WriteUInt32(item.Id);

            ushort bResInfo = 0;
            if (item.Type == MenuItemType.Popup)
                bResInfo |= 0x01;
            if (i == items.Count - 1)
                bResInfo |= 0x80;

            writer.WriteUInt16(bResInfo);
            WriteUnicodeString(writer, item.Text ?? string.Empty);

            if (item.Type == MenuItemType.Popup)
            {
                writer.Align(4);
                writer.WriteUInt32(item.HelpId);
                WriteExtendedItems(writer, item.SubItems);
            }
        }
    }

    private static void WriteUnicodeString(BinaryStreamWriter writer, string value)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(value);
        writer.WriteBytes(bytes);
        writer.WriteUInt16(0); // null terminator
    }
}
