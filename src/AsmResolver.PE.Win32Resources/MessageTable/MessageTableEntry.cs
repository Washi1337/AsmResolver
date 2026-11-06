namespace AsmResolver.PE.Win32Resources.MessageTable;

/// <summary>
/// Represents a single entry in a message table resource.
/// </summary>
public class MessageTableEntry
{
    /// <summary>
    /// Creates a new empty message table entry.
    /// </summary>
    public MessageTableEntry()
    {
    }

    /// <summary>
    /// Creates a new message table entry with the specified identifier and text.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="text">The message text.</param>
    /// <param name="isUnicode"><c>true</c> if the message is stored as Unicode, <c>false</c> for ANSI.</param>
    public MessageTableEntry(uint id, string text, bool isUnicode = false)
    {
        Id = id;
        Text = text;
        IsUnicode = isUnicode;
    }

    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the message text is stored as Unicode.
    /// When <c>false</c>, the text is stored as ANSI.
    /// </summary>
    public bool IsUnicode { get; set; }
}
