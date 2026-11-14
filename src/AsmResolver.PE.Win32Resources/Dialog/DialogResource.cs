using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Dialog;

/// <summary>
/// Represents a native dialog resource (RT_DIALOG) stored in a PE image. Supports both standard DIALOG and extended
/// DIALOGEX formats.
/// </summary>
public class DialogResource : IWin32Resource
{
    private const uint DsSetFont = 0x40;

    /// <summary>
    /// Creates a new empty dialog resource.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    public DialogResource(uint id)
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
    /// Gets or sets whether this dialog uses the extended DIALOGEX format.
    /// </summary>
    public bool IsExtended { get; set; }

    /// <summary>
    /// Gets or sets the help identifier (DIALOGEX only).
    /// </summary>
    public uint HelpId { get; set; }

    /// <summary>
    /// Gets or sets the window style flags.
    /// </summary>
    public uint Style { get; set; }

    /// <summary>
    /// Gets or sets the extended window style flags.
    /// </summary>
    public uint ExtendedStyle { get; set; }

    /// <summary>
    /// Gets or sets the x-coordinate of the dialog.
    /// </summary>
    public short X { get; set; }

    /// <summary>
    /// Gets or sets the y-coordinate of the dialog.
    /// </summary>
    public short Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the dialog.
    /// </summary>
    public short Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the dialog.
    /// </summary>
    public short Height { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the menu, if specified by ordinal.
    /// </summary>
    public ushort? MenuOrdinal { get; set; }

    /// <summary>
    /// Gets or sets the name of the menu, if specified by name.
    /// </summary>
    public string? MenuName { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the window class, if specified by ordinal.
    /// </summary>
    public ushort? ClassOrdinal { get; set; }

    /// <summary>
    /// Gets or sets the name of the window class, if specified by name.
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the caption (title) of the dialog.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Gets or sets the font point size.
    /// </summary>
    public ushort? FontSize { get; set; }

    /// <summary>
    /// Gets or sets the font weight (DIALOGEX only).
    /// </summary>
    public ushort? FontWeight { get; set; }

    /// <summary>
    /// Gets or sets the font italic flag (DIALOGEX only).
    /// </summary>
    public byte? FontItalic { get; set; }

    /// <summary>
    /// Gets or sets the font character set (DIALOGEX only).
    /// </summary>
    public byte? FontCharset { get; set; }

    /// <summary>
    /// Gets or sets the font typeface name.
    /// </summary>
    public string? FontName { get; set; }

    /// <summary>
    /// Gets the collection of controls in the dialog.
    /// </summary>
    public IList<DialogControl> Controls { get; } = new List<DialogControl>();

    /// <summary>
    /// Reads all dialog resources from the provided root win32 resources directory.
    /// </summary>
    /// <param name="rootDirectory">The root resources directory to extract dialog resources from.</param>
    /// <returns>The collection of dialog resources.</returns>
    public static IEnumerable<DialogResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Dialog, out var dialogDirectory))
            yield break;

        foreach (var idDir in dialogDirectory.Entries.OfType<ResourceDirectory>())
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
    /// Reads a dialog resource from an input stream.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed dialog resource.</returns>
    public static DialogResource FromReader(uint id, uint lcid, ref BinaryStreamReader reader)
    {
        // Discriminate: read first DWORD, check if bytes [2..4] are 0xFFFF.
        ulong startOffset = reader.Offset;
        ushort first = reader.ReadUInt16();
        ushort second = reader.ReadUInt16();
        reader.Offset = startOffset;

        bool isExtended = first == 1 && second == 0xFFFF;

        return isExtended
            ? ReadExtended(id, lcid, ref reader)
            : ReadStandard(id, lcid, ref reader);
    }

    private static DialogResource ReadStandard(uint id, uint lcid, ref BinaryStreamReader reader)
    {
        uint style = reader.ReadUInt32();
        uint exStyle = reader.ReadUInt32();
        ushort cdit = reader.ReadUInt16();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        short cx = reader.ReadInt16();
        short cy = reader.ReadInt16();

        var resource = new DialogResource(id)
        {
            Lcid = lcid,
            IsExtended = false,
            Style = style,
            ExtendedStyle = exStyle,
            X = x,
            Y = y,
            Width = cx,
            Height = cy
        };

        // Menu
        ReadVariableField(ref reader, out ushort? menuOrd, out string? menuName);
        resource.MenuOrdinal = menuOrd;
        resource.MenuName = menuName;

        // Class
        ReadVariableField(ref reader, out ushort? classOrd, out string? className);
        resource.ClassOrdinal = classOrd;
        resource.ClassName = className;

        // Caption
        resource.Caption = reader.ReadUnicodeString();

        // Font
        if ((style & DsSetFont) != 0)
        {
            resource.FontSize = reader.ReadUInt16();
            resource.FontName = reader.ReadUnicodeString();
        }

        // Controls
        for (int i = 0; i < cdit; i++)
        {
            reader.Align(4);

            var control = new DialogControl
            {
                Style = reader.ReadUInt32(),
                ExtendedStyle = reader.ReadUInt32(),
                X = reader.ReadInt16(),
                Y = reader.ReadInt16(),
                Width = reader.ReadInt16(),
                Height = reader.ReadInt16(),
                Id = reader.ReadUInt16()
            };

            ReadVariableField(ref reader, out ushort? clsOrd, out string? clsName);
            control.ClassOrdinal = clsOrd;
            control.ClassName = clsName;

            ReadVariableField(ref reader, out ushort? txtOrd, out string? txtName);
            control.TextOrdinal = txtOrd;
            control.Text = txtName;

            ushort extraCount = reader.ReadUInt16();
            if (extraCount > 0)
            {
                control.ExtraData = new byte[extraCount];
                reader.ReadBytes(control.ExtraData, 0, extraCount);
            }

            resource.Controls.Add(control);
        }

        return resource;
    }

    private static DialogResource ReadExtended(uint id, uint lcid, ref BinaryStreamReader reader)
    {
        reader.ReadUInt16(); // dlgVer
        reader.ReadUInt16(); // signature
        uint helpId = reader.ReadUInt32();
        uint exStyle = reader.ReadUInt32();
        uint style = reader.ReadUInt32();
        ushort cdit = reader.ReadUInt16();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        short cx = reader.ReadInt16();
        short cy = reader.ReadInt16();

        var resource = new DialogResource(id)
        {
            Lcid = lcid,
            IsExtended = true,
            HelpId = helpId,
            Style = style,
            ExtendedStyle = exStyle,
            X = x,
            Y = y,
            Width = cx,
            Height = cy
        };

        // Menu
        ReadVariableField(ref reader, out ushort? menuOrd, out string? menuName);
        resource.MenuOrdinal = menuOrd;
        resource.MenuName = menuName;

        // Class
        ReadVariableField(ref reader, out ushort? classOrd, out string? className);
        resource.ClassOrdinal = classOrd;
        resource.ClassName = className;

        // Caption
        resource.Caption = reader.ReadUnicodeString();

        // Font (DS_SETFONT or DS_SHELLFONT)
        if ((style & DsSetFont) != 0)
        {
            resource.FontSize = reader.ReadUInt16();
            resource.FontWeight = reader.ReadUInt16();
            resource.FontItalic = reader.ReadByte();
            resource.FontCharset = reader.ReadByte();
            resource.FontName = reader.ReadUnicodeString();
        }

        // Controls
        for (int i = 0; i < cdit; i++)
        {
            reader.Align(4);

            var control = new DialogControl
            {
                HelpId = reader.ReadUInt32(),
                ExtendedStyle = reader.ReadUInt32(),
                Style = reader.ReadUInt32(),
                X = reader.ReadInt16(),
                Y = reader.ReadInt16(),
                Width = reader.ReadInt16(),
                Height = reader.ReadInt16(),
                Id = reader.ReadUInt32()
            };

            ReadVariableField(ref reader, out ushort? clsOrd, out string? clsName);
            control.ClassOrdinal = clsOrd;
            control.ClassName = clsName;

            ReadVariableField(ref reader, out ushort? txtOrd, out string? txtName);
            control.TextOrdinal = txtOrd;
            control.Text = txtName;

            ushort extraCount = reader.ReadUInt16();
            if (extraCount > 0)
            {
                control.ExtraData = new byte[extraCount];
                reader.ReadBytes(control.ExtraData, 0, extraCount);
            }

            resource.Controls.Add(control);
        }

        return resource;
    }

    private static void ReadVariableField(ref BinaryStreamReader reader, out ushort? ordinal, out string? name)
    {
        ushort first = reader.ReadUInt16();
        if (first == 0)
        {
            ordinal = null;
            name = null;
        }
        else if (first == 0xFFFF)
        {
            ordinal = reader.ReadUInt16();
            name = null;
        }
        else
        {
            ordinal = null;
            var sb = new StringBuilder();
            sb.Append((char)first);
            while (true)
            {
                ushort ch = reader.ReadUInt16();
                if (ch == 0)
                    break;
                sb.Append((char)ch);
            }
            name = sb.ToString();
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Dialog, out var dialogDirectory))
        {
            dialogDirectory = new ResourceDirectory(ResourceType.Dialog);
            rootDirectory.InsertOrReplaceEntry(dialogDirectory);
        }

        if (!dialogDirectory.TryGetDirectory(Id, out var idDirectory))
        {
            idDirectory = new ResourceDirectory(Id);
            dialogDirectory.InsertOrReplaceEntry(idDirectory);
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
        writer.WriteUInt32(Style);
        writer.WriteUInt32(ExtendedStyle);
        writer.WriteUInt16((ushort)Controls.Count);
        writer.WriteInt16(X);
        writer.WriteInt16(Y);
        writer.WriteInt16(Width);
        writer.WriteInt16(Height);

        WriteVariableField(writer, MenuOrdinal, MenuName);
        WriteVariableField(writer, ClassOrdinal, ClassName);
        WriteUnicodeString(writer, Caption ?? string.Empty);

        if ((Style & DsSetFont) != 0 && FontSize.HasValue)
        {
            writer.WriteUInt16(FontSize.Value);
            WriteUnicodeString(writer, FontName ?? string.Empty);
        }

        foreach (var control in Controls)
        {
            writer.Align(4);

            writer.WriteUInt32(control.Style);
            writer.WriteUInt32(control.ExtendedStyle);
            writer.WriteInt16(control.X);
            writer.WriteInt16(control.Y);
            writer.WriteInt16(control.Width);
            writer.WriteInt16(control.Height);
            writer.WriteUInt16((ushort)control.Id);

            WriteVariableField(writer, control.ClassOrdinal, control.ClassName);
            WriteVariableField(writer, control.TextOrdinal, control.Text);

            if (control.ExtraData is { Length: > 0 })
            {
                writer.WriteUInt16((ushort)control.ExtraData.Length);
                writer.WriteBytes(control.ExtraData);
            }
            else
            {
                writer.WriteUInt16(0);
            }
        }
    }

    private void WriteExtended(BinaryStreamWriter writer)
    {
        writer.WriteUInt16(1); // dlgVer
        writer.WriteUInt16(0xFFFF); // signature
        writer.WriteUInt32(HelpId);
        writer.WriteUInt32(ExtendedStyle);
        writer.WriteUInt32(Style);
        writer.WriteUInt16((ushort)Controls.Count);
        writer.WriteInt16(X);
        writer.WriteInt16(Y);
        writer.WriteInt16(Width);
        writer.WriteInt16(Height);

        WriteVariableField(writer, MenuOrdinal, MenuName);
        WriteVariableField(writer, ClassOrdinal, ClassName);
        WriteUnicodeString(writer, Caption ?? string.Empty);

        if ((Style & DsSetFont) != 0 && FontSize.HasValue)
        {
            writer.WriteUInt16(FontSize.Value);
            writer.WriteUInt16(FontWeight ?? 0);
            writer.WriteByte(FontItalic ?? 0);
            writer.WriteByte(FontCharset ?? 0);
            WriteUnicodeString(writer, FontName ?? string.Empty);
        }

        foreach (var control in Controls)
        {
            writer.Align(4);

            writer.WriteUInt32(control.HelpId);
            writer.WriteUInt32(control.ExtendedStyle);
            writer.WriteUInt32(control.Style);
            writer.WriteInt16(control.X);
            writer.WriteInt16(control.Y);
            writer.WriteInt16(control.Width);
            writer.WriteInt16(control.Height);
            writer.WriteUInt32(control.Id);

            WriteVariableField(writer, control.ClassOrdinal, control.ClassName);
            WriteVariableField(writer, control.TextOrdinal, control.Text);

            if (control.ExtraData is { Length: > 0 })
            {
                writer.WriteUInt16((ushort)control.ExtraData.Length);
                writer.WriteBytes(control.ExtraData);
            }
            else
            {
                writer.WriteUInt16(0);
            }
        }
    }

    private static void WriteVariableField(BinaryStreamWriter writer, ushort? ordinal, string? name)
    {
        if (ordinal.HasValue)
        {
            writer.WriteUInt16(0xFFFF);
            writer.WriteUInt16(ordinal.Value);
        }
        else if (name is not null)
        {
            WriteUnicodeString(writer, name);
        }
        else
        {
            writer.WriteUInt16(0);
        }
    }

    private static void WriteUnicodeString(BinaryStreamWriter writer, string value)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(value);
        writer.WriteBytes(bytes);
        writer.WriteUInt16(0); // null terminator
    }
}
