namespace AsmResolver.PE.Win32Resources.Dialog;

/// <summary>
/// Represents a control within a dialog resource.
/// </summary>
public class DialogControl
{
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
    /// Gets or sets the x-coordinate of the control.
    /// </summary>
    public short X { get; set; }

    /// <summary>
    /// Gets or sets the y-coordinate of the control.
    /// </summary>
    public short Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the control.
    /// </summary>
    public short Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the control.
    /// </summary>
    public short Height { get; set; }

    /// <summary>
    /// Gets or sets the control identifier.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the window class, if specified by ordinal.
    /// </summary>
    public ushort? ClassOrdinal { get; set; }

    /// <summary>
    /// Gets or sets the name of the window class, if specified by name.
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the text/resource, if specified by ordinal.
    /// </summary>
    public ushort? TextOrdinal { get; set; }

    /// <summary>
    /// Gets or sets the text of the control.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets extra creation data appended after the control.
    /// </summary>
    public byte[]? ExtraData { get; set; }
}
